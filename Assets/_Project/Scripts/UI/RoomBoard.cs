using UnityEngine;
using UnityEngine.UI;

namespace EduGallery
{
    /// <summary>
    /// The information board of a room: introduction, progress, score and time.
    /// It is a board in the world, so it is readable on desktop and in VR.
    /// </summary>
    public class RoomBoard : MonoBehaviour
    {
        public RoomManager room;
        public Text titleLabel;
        public Text bodyLabel;
        public Text statsLabel;

        private void Start()
        {
            titleLabel.text = room.roomName;
            bodyLabel.text = room.introduction;
        }

        private void Update()
        {
            if (GameManager.Instance == null || ScoreManager.Instance == null) return;
            string progress = room.puzzles.Length == 0
                ? ""
                : room.IsCompleted
                    ? "All puzzles solved! The door is open.\n"
                    : $"Puzzles solved: {room.SolvedCount}/{room.puzzles.Length}\n";
            statsLabel.text = $"{progress}Score: {ScoreManager.Instance.Score}   " +
                              $"Time: {TimeFormat.Clock(GameManager.Instance.ElapsedSeconds)}";
        }
    }
}
