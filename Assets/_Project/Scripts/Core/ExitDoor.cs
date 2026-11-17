using UnityEngine;

namespace EduGallery
{
    /// <summary>
    /// The exit door of a room. It slides open, then the player walks through it
    /// to reach the next room.
    /// </summary>
    public class ExitDoor : MonoBehaviour
    {
        [Tooltip("The part of the door that moves.")]
        public Transform panel;
        public Vector3 openOffset = new Vector3(0f, 2.3f, 0f);
        [SerializeField] private float openSeconds = 1.5f;
        [Tooltip("The player goes to the next room when closer than this to the door.")]
        [SerializeField] private float passDistance = 0.8f;

        public bool IsOpen { get; private set; }

        private Vector3 closedPosition;
        private float progress;
        private bool used;

        private void Awake() => closedPosition = panel.localPosition;

        public void Open() => IsOpen = true;

        private void Update()
        {
            // The door stays shut until the game is running (the tutorial can be solved before START).
            if (!IsOpen || GameManager.Instance.State != GameState.Playing) return;
            progress = Mathf.MoveTowards(progress, 1f, Time.deltaTime / openSeconds);
            panel.localPosition = closedPosition + openOffset * Mathf.SmoothStep(0f, 1f, progress);
            if (progress >= 1f && !used && PlayerIsAtDoor()) PassThrough();
        }

        private bool PlayerIsAtDoor()
        {
            if (PlayerRig.Current == null) return false;
            Vector3 offset = PlayerRig.Current.Head.position - transform.position;
            offset.y = 0f;
            return offset.magnitude <= passDistance;
        }

        private void PassThrough()
        {
            used = true;
            GameManager.Instance.GoToNextRoom();
        }
    }
}
