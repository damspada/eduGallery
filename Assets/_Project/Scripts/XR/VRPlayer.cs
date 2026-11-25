using Unity.XR.CoreUtils;
using UnityEngine;

namespace EduGallery
{
    /// <summary>
    /// The player in VR. Put it on the XR Origin (XR Rig) from the XRI Starter Assets.
    /// On start it switches off the desktop player and connects every game object
    /// (grabbables, buttons, artworks) to the VR controllers with <see cref="XRInteractionBridge"/>.
    /// </summary>
    [RequireComponent(typeof(XROrigin))]
    public class VRPlayer : PlayerRig
    {
        private XROrigin origin;

        public override Transform Head => origin.Camera.transform;

        private void Awake()
        {
            origin = GetComponent<XROrigin>();

            // Only one player at a time: the desktop one would fight for the camera and the input.
            foreach (var desktop in FindObjectsByType<DesktopPlayer>(FindObjectsSortMode.None))
                desktop.gameObject.SetActive(false);
        }

        private void Start() => XRInteractionBridge.ConnectScene();

        /// <summary>Puts the player's feet on <paramref name="position"/>, looking along <paramref name="rotation"/>.</summary>
        public override void TeleportTo(Vector3 position, Quaternion rotation)
        {
            // Turn the rig around the head, so the player looks where the spawn point looks.
            float yawDelta = Mathf.DeltaAngle(Head.eulerAngles.y, rotation.eulerAngles.y);
            origin.RotateAroundCameraUsingOriginUp(yawDelta);

            // Move the head above the spawn point, at the player's real height.
            origin.MoveCameraToWorldLocation(position + origin.transform.up * origin.CameraInOriginSpaceHeight);
        }
    }
}
