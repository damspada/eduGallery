using System.Linq;
using UnityEngine;

namespace EduGallery
{
    /// <summary>
    /// Several sockets, each one expects a specific object.
    /// Used for the jigsaw (pieces of a painting) and for the timeline (artworks in order).
    /// The answer is checked when every socket is full.
    /// </summary>
    public class SocketMatchPuzzle : PuzzleBase
    {
        [Header("Socket match puzzle")]
        public SnapSocket[] sockets;
        [Tooltip("Optional: shown when the puzzle is solved (e.g. the complete painting).")]
        public GameObject revealOnSolved;
        [Tooltip("Optional: hidden when the puzzle is solved (e.g. the loose pieces).")]
        public GameObject hideOnSolved;
        [SerializeField] private float ejectDelay = 1f;

        private void OnEnable()
        {
            foreach (var socket in sockets) socket.Snapped += OnObjectPlaced;
        }

        private void OnDisable()
        {
            foreach (var socket in sockets) socket.Snapped -= OnObjectPlaced;
        }

        private void OnObjectPlaced(SnapSocket socket, Grabbable placed)
        {
            MarkStarted();
            if (sockets.Any(s => s.Occupant == null)) return;

            if (sockets.All(s => s.HasCorrectOccupant))
            {
                foreach (var s in sockets) s.Locked = true;
                if (revealOnSolved != null) revealOnSolved.SetActive(true);
                if (hideOnSolved != null) hideOnSolved.SetActive(false);
                Solve();
            }
            else
            {
                Fail();
                Invoke(nameof(EjectWrongObjects), ejectDelay);
            }
        }

        private void EjectWrongObjects()
        {
            if (IsSolved) return;
            foreach (var socket in sockets)
                if (!socket.HasCorrectOccupant) socket.Eject();
        }
    }
}
