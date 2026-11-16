using System;
using UnityEngine;

namespace EduGallery
{
    /// <summary>
    /// An object the player can pick up. It does not know who holds it:
    /// the desktop player and the VR bridge both call <see cref="NotifyGrabbed"/> and <see cref="NotifyReleased"/>.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Grabbable : MonoBehaviour
    {
        [Tooltip("Used by sockets and puzzles to recognise this object.")]
        public string id;
        [Tooltip("Desktop only: the object is thrown forward when released.")]
        public bool throwOnRelease;
        [Tooltip("Desktop only: the object points where the player looks while held (flashlight).")]
        public bool alignWithViewWhenHeld;

        public event Action<Grabbable> Grabbed;
        public event Action<Grabbable> Released;

        /// <summary>Raised when <see cref="CanBeGrabbed"/> changes.</summary>
        public event Action<Grabbable> GrabAllowedChanged;

        public bool IsHeld { get; private set; }

        /// <summary>False when the object is fixed in place (a piece of a solved puzzle).</summary>
        public bool CanBeGrabbed
        {
            get => canBeGrabbed;
            set
            {
                if (canBeGrabbed == value) return;
                canBeGrabbed = value;
                GrabAllowedChanged?.Invoke(this);
            }
        }

        private bool canBeGrabbed = true;
        public Rigidbody Body { get; private set; }

        private Vector3 homePosition;
        private Quaternion homeRotation;
        private Transform homeParent;
        private bool homeKinematic;
        private const float FallLimit = -5f;

        private void Awake()
        {
            Body = GetComponent<Rigidbody>();
            homePosition = transform.position;
            homeRotation = transform.rotation;
            homeParent = transform.parent;
            homeKinematic = Body.isKinematic;
        }

        private void Update()
        {
            // Safety net: an object that falls out of the room goes back to its start position.
            if (transform.position.y < homePosition.y + FallLimit) ReturnHome();
        }

        public void NotifyGrabbed()
        {
            IsHeld = true;
            Grabbed?.Invoke(this);
            GameEvents.RaiseObjectGrabbed(this);
        }

        public void NotifyReleased()
        {
            IsHeld = false;
            Released?.Invoke(this);
        }

        public void ReturnHome()
        {
            transform.SetParent(homeParent, true);
            if (!Body.isKinematic)
            {
                Body.linearVelocity = Vector3.zero;
                Body.angularVelocity = Vector3.zero;
            }
            Body.isKinematic = homeKinematic;
            transform.SetPositionAndRotation(homePosition, homeRotation);
        }
    }
}
