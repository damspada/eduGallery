using System;
using System.Collections;
using UnityEngine;

namespace EduGallery
{
    /// <summary>
    /// A place where a released <see cref="Grabbable"/> snaps in. Needs a trigger collider.
    /// Works the same on desktop and in VR because it only looks at released objects.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class SnapSocket : MonoBehaviour
    {
        [Tooltip("If not empty, only the grabbable with this id snaps in.")]
        public string acceptedId;
        [Tooltip("The id this socket expects (used by puzzles to check the answer).")]
        public string expectedId;
        [Tooltip("Optional: glows while the player holds a fitting object over the socket.")]
        public Highlightable indicator;
        [SerializeField] private float snapSeconds = 0.15f;

        public event Action<SnapSocket, Grabbable> Snapped;
        public event Action<SnapSocket, Grabbable> Removed;

        public Grabbable Occupant { get; private set; }
        /// <summary>A locked socket keeps its object: the player cannot take it out any more.</summary>
        public bool Locked
        {
            get => locked;
            set
            {
                locked = value;
                if (Occupant != null) Occupant.CanBeGrabbed = !value;
            }
        }

        private bool locked;
        public bool HasCorrectOccupant => Occupant != null && Occupant.id == expectedId;

        private bool heldObjectInside;
        private float aimedUntil;

        /// <summary>True if this object can go in the socket now.</summary>
        public bool Accepts(Grabbable grabbable)
        {
            if (grabbable == null || Occupant != null || Locked) return false;
            return string.IsNullOrEmpty(acceptedId) || grabbable.id == acceptedId;
        }

        /// <summary>Puts a released object in the socket from any distance (desktop: point and click).</summary>
        public bool TryPlace(Grabbable grabbable)
        {
            if (!Accepts(grabbable) || grabbable.IsHeld) return false;
            Snap(grabbable);
            return true;
        }

        /// <summary>The player points at the socket with a fitting object in the hand: the socket glows.</summary>
        public void ShowAim() => aimedUntil = Time.time + 0.1f;

        private void OnTriggerStay(Collider other)
        {
            var grabbable = other.GetComponentInParent<Grabbable>();
            if (!Accepts(grabbable)) return;

            if (grabbable.IsHeld) heldObjectInside = true;
            else Snap(grabbable);
        }

        private void FixedUpdate()
        {
            // OnTriggerStay runs after FixedUpdate, so this shows the result of the last physics step.
            bool wanted = heldObjectInside || Time.time < aimedUntil;
            if (indicator != null) indicator.SetHighlighted(wanted && Occupant == null);
            heldObjectInside = false;
        }

        private void Snap(Grabbable grabbable)
        {
            Occupant = grabbable;
            grabbable.Body.isKinematic = true;
            grabbable.Grabbed += OnOccupantGrabbed;
            StartCoroutine(SlideIntoPlace(grabbable));
            GameEvents.RaiseObjectSnapped(grabbable);
            Snapped?.Invoke(this, grabbable);
        }

        /// <summary>Moves the object smoothly into the socket instead of jumping there.</summary>
        private IEnumerator SlideIntoPlace(Grabbable grabbable)
        {
            Transform moved = grabbable.transform;
            Vector3 startPosition = moved.position;
            Quaternion startRotation = moved.rotation;
            for (float t = 0f; t < 1f && Occupant == grabbable; t += Time.deltaTime / snapSeconds)
            {
                float eased = Mathf.SmoothStep(0f, 1f, t);
                moved.SetPositionAndRotation(
                    Vector3.Lerp(startPosition, transform.position, eased),
                    Quaternion.Slerp(startRotation, transform.rotation, eased));
                yield return null;
            }
            if (Occupant == grabbable) moved.SetPositionAndRotation(transform.position, transform.rotation);
        }

        private void OnOccupantGrabbed(Grabbable grabbable)
        {
            if (Locked) return;
            Release();
            Removed?.Invoke(this, grabbable);
        }

        /// <summary>Sends the object in the socket back to where it started.</summary>
        public void Eject()
        {
            if (Occupant == null || Locked) return;
            var grabbable = Occupant;
            Release();
            grabbable.ReturnHome();
        }

        private void Release()
        {
            Occupant.Grabbed -= OnOccupantGrabbed;
            Occupant = null;
        }
    }
}
