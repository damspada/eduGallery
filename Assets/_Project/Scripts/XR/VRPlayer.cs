using UnityEngine;

namespace EduGallery
{
    /// <summary>The VR player. Put it on the XR Origin.</summary>
    public class VRPlayer : PlayerRig
    {
        [SerializeField] private Transform headCamera;

        public override Transform Head => headCamera;

        public override void TeleportTo(Vector3 position, Quaternion rotation)
        {
            // Turn the rig so the player faces the wanted direction...
            float turn = rotation.eulerAngles.y - headCamera.eulerAngles.y;
            transform.RotateAround(headCamera.position, Vector3.up, turn);

            // ...then move it so the headset (not the rig centre) is above the target.
            Vector3 headOffset = headCamera.position - transform.position;
            headOffset.y = 0f;
            transform.position = position - headOffset;
        }
    }
}
