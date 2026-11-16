using UnityEngine;

namespace EduGallery
{
    /// <summary>
    /// Common base for the desktop player and the VR player.
    /// The rest of the game only uses this class, so the two players can be swapped.
    /// </summary>
    public abstract class PlayerRig : MonoBehaviour
    {
        public static PlayerRig Current { get; private set; }

        /// <summary>The player's eyes: the camera (desktop) or the headset (VR).</summary>
        public abstract Transform Head { get; }

        protected virtual void OnEnable() => Current = this;

        protected virtual void OnDisable()
        {
            if (Current == this) Current = null;
        }

        public abstract void TeleportTo(Vector3 position, Quaternion rotation);
    }
}
