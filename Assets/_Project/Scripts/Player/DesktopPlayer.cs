using UnityEngine;
using UnityEngine.InputSystem;

namespace EduGallery
{
    /// <summary>
    /// First-person player for keyboard and mouse.
    /// WASD or arrow keys: move. Mouse: look. Left click: interact, grab, use, release.
    /// A held object stays in the hand, in front of the player. Pointing at the place where it goes
    /// (a socket, a paint pot...) and clicking puts it there, so no precise movement is needed.
    /// Mouse wheel: closer or farther. Hold R and move the mouse: turn the object.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class DesktopPlayer : PlayerRig
    {
        [SerializeField] private Camera playerCamera;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 2.5f;
        [SerializeField] private float lookSensitivity = 0.12f;

        [Header("Interaction")]
        [SerializeField] private float reach = 3f;
        [Tooltip("Pointing this close to a small object is enough to select it.")]
        [SerializeField] private float aimAssistRadius = 0.07f;
        [Tooltip("With an object in the hand, pointing this close to its place is enough.")]
        [SerializeField] private float socketAssistRadius = 0.2f;
        [SerializeField] private float minHoldDistance = 0.35f;
        [SerializeField] private float maxHoldDistance = 1.4f;
        [Tooltip("How fast a held object follows the hand.")]
        [SerializeField] private float followSpeed = 22f;
        [SerializeField] private float turnSensitivity = 0.4f;
        [SerializeField] private float throwSpeed = 9f;

        private CharacterController controller;
        private Grabbable held;
        private Collider[] heldColliders;
        private float heldRadius;
        private float holdDistance;
        private Quaternion holdRotation;   // rotation of the held object, relative to the camera
        private Vector3 heldVelocity;
        private Highlightable hovered;
        private float pitch;
        private string currentPrompt = "";

        // What the player is pointing at in this frame.
        private Grabbable aimedGrabbable;
        private float aimedDistance;
        private IInteractable aimedInteractable;
        private SnapSocket aimedSocket;
        private Vector3 aimedPoint;

        public override Transform Head => playerCamera.transform;

        /// <summary>The object in the hand of the player, or null.</summary>
        public Grabbable Held => held;

        // The body of the player is on the "Ignore Raycast" layer. Without this, looking down from very
        // close made the pointer hit the player's own capsule, and nothing under it could be selected.
        private const int IgnoreRaycastLayer = 2;
        private const int SightMask = Physics.DefaultRaycastLayers;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            gameObject.layer = IgnoreRaycastLayer;
        }

        private void Start() => LockCursor(true);

        private void Update()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            if (keyboard == null || mouse == null) return;

            if (keyboard.escapeKey.wasPressedThisFrame) LockCursor(false);
            if (mouse.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
            {
                LockCursor(true);
                return;
            }

            bool turningObject = held != null && keyboard.rKey.isPressed;
            if (turningObject) TurnHeldObject(mouse);
            else Look(mouse);
            Move(keyboard);

            if (held != null)
            {
                float wheel = mouse.scroll.ReadValue().y;
                if (Mathf.Abs(wheel) > 0.01f)
                    holdDistance = Mathf.Clamp(holdDistance + Mathf.Sign(wheel) * 0.1f, minHoldDistance, maxHoldDistance);
            }
            UpdateAim();
            if (mouse.leftButton.wasPressedThisFrame) Click();
        }

        private void LateUpdate() => CarryHeldObject();

        private void Look(Mouse mouse)
        {
            if (Cursor.lockState != CursorLockMode.Locked) return;
            Vector2 delta = mouse.delta.ReadValue() * lookSensitivity;
            pitch = Mathf.Clamp(pitch - delta.y, -85f, 85f);
            transform.Rotate(0f, delta.x, 0f);
            playerCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        private void Move(Keyboard keyboard)
        {
            float x = (Pressed(keyboard.dKey, keyboard.rightArrowKey) ? 1f : 0f) -
                      (Pressed(keyboard.aKey, keyboard.leftArrowKey) ? 1f : 0f);
            float z = (Pressed(keyboard.wKey, keyboard.upArrowKey) ? 1f : 0f) -
                      (Pressed(keyboard.sKey, keyboard.downArrowKey) ? 1f : 0f);
            Vector3 direction = Vector3.ClampMagnitude(transform.right * x + transform.forward * z, 1f);
            controller.SimpleMove(direction * moveSpeed);
        }

        private static bool Pressed(UnityEngine.InputSystem.Controls.KeyControl a,
            UnityEngine.InputSystem.Controls.KeyControl b) => a.isPressed || b.isPressed;

        // ------------------------------------------------------------------
        // Pointing: what is under the pointer, and what a click would do
        // ------------------------------------------------------------------

        /// <summary>Finds what the player points at and updates the glow and the prompt.</summary>
        public void UpdateAim()
        {
            aimedGrabbable = null;
            aimedInteractable = null;
            aimedSocket = null;
            currentPrompt = "";
            if (held != null && !held.gameObject.activeInHierarchy) held = null;

            var ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (held != null) AimWithObjectInHand(ray);
            else AimWithEmptyHand(ray);
        }

        private void AimWithEmptyHand(Ray ray)
        {
            bool hitSomething = Physics.Raycast(ray, out RaycastHit hit, reach, SightMask, QueryTriggerInteraction.Ignore);
            if (hitSomething) ReadTarget(hit);

            // Aim assist: a small object (a key, a brush) is selected even if the pointer is just next to it.
            if (aimedGrabbable == null && aimedInteractable == null)
            {
                float limit = hitSomething ? hit.distance + 0.15f : reach;
                RaycastHit[] near = Physics.SphereCastAll(ray, aimAssistRadius, limit, SightMask, QueryTriggerInteraction.Ignore);
                System.Array.Sort(near, (a, b) => a.distance.CompareTo(b.distance));
                foreach (RaycastHit candidate in near)
                {
                    var grabbable = candidate.collider.GetComponentInParent<Grabbable>();
                    if (grabbable == null || !grabbable.CanBeGrabbed) continue;
                    hit = candidate;
                    hitSomething = true;
                    ReadTarget(hit);
                    break;
                }
            }

            SetHovered(hitSomething ? hit.collider.GetComponentInParent<Highlightable>() : null);
            if (aimedGrabbable != null) currentPrompt = "Click: grab";
            else if (aimedInteractable != null) currentPrompt = aimedInteractable.Prompt;
        }

        private void ReadTarget(RaycastHit hit)
        {
            var grabbable = hit.collider.GetComponentInParent<Grabbable>();
            if (grabbable != null && grabbable.CanBeGrabbed)
            {
                aimedGrabbable = grabbable;
                aimedDistance = hit.distance;
                return;
            }
            aimedInteractable = hit.collider.GetComponentInParent<IInteractable>();
        }

        private void AimWithObjectInHand(Ray ray)
        {
            SetHovered(null);
            float solidDistance = reach;
            RaycastHit[] hits = Physics.RaycastAll(ray, reach, SightMask, QueryTriggerInteraction.Collide);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (RaycastHit hit in hits)
            {
                if (System.Array.IndexOf(heldColliders, hit.collider) >= 0) continue;

                var socket = hit.collider.GetComponent<SnapSocket>();
                if (socket != null && socket.Accepts(held))
                {
                    aimedSocket = socket;
                    socket.ShowAim();
                    break;
                }
                if (!hit.collider.isTrigger)
                {
                    solidDistance = hit.distance;   // a wall or a table: nothing behind it counts
                    break;
                }
            }

            // Aim assist: pointing next to the right place (or at the statue under it) is enough.
            if (aimedSocket == null)
            {
                RaycastHit[] near = Physics.SphereCastAll(ray, socketAssistRadius, Mathf.Min(reach, solidDistance + 0.3f),
                    SightMask, QueryTriggerInteraction.Collide);
                System.Array.Sort(near, (a, b) => a.distance.CompareTo(b.distance));
                foreach (RaycastHit hit in near)
                {
                    var socket = hit.collider.GetComponent<SnapSocket>();
                    if (socket == null || !socket.Accepts(held)) continue;
                    aimedSocket = socket;
                    socket.ShowAim();
                    break;
                }
            }

            if (aimedSocket != null) currentPrompt = "Click: put it here";
            else currentPrompt = (held.throwOnRelease ? "Click: throw" : "Click: let go") +
                                 "    Wheel: closer / farther    Hold R + mouse: turn";
        }

        /// <summary>Does what a left click does: grab, press, use the held object, or let it go.</summary>
        public void Click()
        {
            if (held == null)
            {
                if (aimedGrabbable != null) Grab(aimedGrabbable);
                else if (aimedInteractable != null) aimedInteractable.Interact();
                return;
            }

            if (aimedSocket != null)
            {
                SnapSocket socket = aimedSocket;
                Grabbable placed = held;
                ReleaseHeld(false);
                socket.TryPlace(placed);
            }
            else
            {
                ReleaseHeld(true);
            }
        }

        private void SetHovered(Highlightable target)
        {
            if (hovered == target) return;
            if (hovered != null) hovered.SetHighlighted(false);
            hovered = target;
            if (hovered != null) hovered.SetHighlighted(true);
        }

        // ------------------------------------------------------------------
        // Holding an object
        // ------------------------------------------------------------------

        private void Grab(Grabbable grabbable)
        {
            held = grabbable;
            heldColliders = held.GetComponentsInChildren<Collider>();
            heldRadius = 0.05f;
            foreach (Collider part in heldColliders)
            {
                if (part.isTrigger) continue;
                heldRadius = Mathf.Max(heldRadius, part.bounds.extents.magnitude);
            }
            heldRadius = Mathf.Min(heldRadius, 0.45f);
            holdDistance = Mathf.Clamp(0.45f + heldRadius, minHoldDistance, maxHoldDistance);
            holdRotation = held.alignWithViewWhenHeld
                ? Quaternion.identity
                : Quaternion.Inverse(playerCamera.transform.rotation) * held.transform.rotation;
            heldVelocity = Vector3.zero;

            // In the hand the object is not a physical body: it cannot get stuck, shake or push the player.
            held.Body.isKinematic = true;
            held.NotifyGrabbed();
        }

        private void ReleaseHeld(bool allowThrow)
        {
            Grabbable released = held;
            held = null;
            released.Body.isKinematic = false;
            released.Body.useGravity = true;
            released.Body.angularVelocity = Vector3.zero;
            if (allowThrow && released.throwOnRelease)
            {
                // Thrown at the point under the pointer, even if the hand is a little to the side.
                Transform cam = playerCamera.transform;
                Vector3 target = Physics.Raycast(cam.position, cam.forward, out RaycastHit hit, 30f, SightMask,
                    QueryTriggerInteraction.Ignore) && hit.distance > 1f
                    ? hit.point
                    : cam.position + cam.forward * 10f;
                // The throw is aimed a little higher, so the ball falls exactly on that point.
                Vector3 toTarget = target - released.Body.position;
                float flightSeconds = toTarget.magnitude / throwSpeed;
                released.Body.linearVelocity = toTarget / flightSeconds - 0.5f * flightSeconds * Physics.gravity;
            }
            else
            {
                released.Body.linearVelocity = Vector3.ClampMagnitude(heldVelocity, 2f);
            }
            released.NotifyReleased();
        }

        private void TurnHeldObject(Mouse mouse)
        {
            Vector2 delta = mouse.delta.ReadValue() * turnSensitivity;
            TurnHeld(delta.y, -delta.x);
        }

        /// <summary>Turns the held object in the hand (degrees, seen from the player).</summary>
        public void TurnHeld(float pitchDegrees, float yawDegrees)
        {
            holdRotation = Quaternion.Euler(pitchDegrees, yawDegrees, 0f) * holdRotation;
        }

        /// <summary>Keeps the held object in the hand: in front of the camera, a little down and to the right.</summary>
        private void CarryHeldObject()
        {
            if (held == null) return;
            Transform cam = playerCamera.transform;
            Vector3 target;
                Vector3 offset = cam.forward * holdDistance + cam.right * (0.14f + heldRadius * 0.45f) -
                                 cam.up * (0.1f + heldRadius * 0.3f);
                // The hand does not go through walls and tables.
                float length = offset.magnitude;
                foreach (RaycastHit hit in Physics.RaycastAll(cam.position, offset / length, length + heldRadius,
                             SightMask, QueryTriggerInteraction.Ignore))
                {
                    if (System.Array.IndexOf(heldColliders, hit.collider) >= 0) continue;
                    if (hit.collider.GetComponentInParent<Grabbable>() != null) continue;
                    length = Mathf.Min(length, Mathf.Max(0.2f, hit.distance - heldRadius));
                }
                target = cam.position + offset.normalized * length;

            float blend = 1f - Mathf.Exp(-followSpeed * Time.deltaTime);
            Vector3 position = Vector3.Lerp(held.transform.position, target, blend);
            if (Time.deltaTime > 0f) heldVelocity = (position - held.transform.position) / Time.deltaTime;
            held.transform.SetPositionAndRotation(position,
                Quaternion.Slerp(held.transform.rotation, cam.rotation * holdRotation, blend));
        }

        // ------------------------------------------------------------------

        public override void TeleportTo(Vector3 position, Quaternion rotation)
        {
            if (held != null) ReleaseHeld(false);
            controller.enabled = false;
            transform.SetPositionAndRotation(position, Quaternion.Euler(0f, rotation.eulerAngles.y, 0f));
            pitch = 0f;
            playerCamera.transform.localRotation = Quaternion.identity;
            controller.enabled = true;
        }

        /// <summary>Turns the player so the camera looks at a point.</summary>
        public void LookAt(Vector3 point)
        {
            Vector3 direction = (point - playerCamera.transform.position).normalized;
            transform.rotation = Quaternion.Euler(0f, Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg, 0f);
            pitch = -Mathf.Asin(direction.y) * Mathf.Rad2Deg;
            playerCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        private static void LockCursor(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        // Pointer and prompt. OnGUI keeps the desktop HUD independent from the scene UI.
        private void OnGUI()
        {
            bool canAct = held != null || !string.IsNullOrEmpty(currentPrompt);
            var pointer = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter, fontSize = canAct ? 26 : 16
            };
            pointer.normal.textColor = canAct ? new Color(1f, 0.85f, 0.4f) : Color.white;
            GUI.Label(new Rect(Screen.width / 2f - 20, Screen.height / 2f - 20, 40, 40), canAct ? "◎" : "•", pointer);

            if (string.IsNullOrEmpty(currentPrompt)) return;
            var prompt = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 18 };
            var area = new Rect(Screen.width / 2f - 350, Screen.height / 2f + 30, 700, 30);
            GUI.color = new Color(0f, 0f, 0f, 0.55f);
            GUI.DrawTexture(new Rect(area.x + 150, area.y, 400, area.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(area, currentPrompt, prompt);
        }
    }
}
