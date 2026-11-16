using System.Linq;
using UnityEngine;

namespace EduGallery
{
    /// <summary>One room of the museum: it owns its puzzles and opens its door when they are all solved.</summary>
    public class RoomManager : MonoBehaviour
    {
        public string roomName;
        [TextArea(2, 6)] public string introduction;
        public Transform spawnPoint;
        public PuzzleBase[] puzzles;
        public ExitDoor door;

        public int SolvedCount => puzzles.Count(p => p.IsSolved);
        public bool IsCompleted => puzzles.Length > 0 && SolvedCount == puzzles.Length;

        private void OnEnable() => GameEvents.PuzzleSolved += OnPuzzleSolved;

        private void OnDisable() => GameEvents.PuzzleSolved -= OnPuzzleSolved;

        /// <summary>Called by the GameManager when the player arrives in this room.</summary>
        public void Enter()
        {
            foreach (var puzzle in puzzles) puzzle.Activate();
            GameEvents.RaiseRoomEntered(this);
        }

        private void OnPuzzleSolved(PuzzleBase puzzle)
        {
            if (!puzzles.Contains(puzzle) || !IsCompleted) return;
            if (door != null) door.Open();
            GameEvents.RaiseRoomCompleted(this);
        }
    }
}
