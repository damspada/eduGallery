using UnityEngine;

namespace EduGallery
{
    /// <summary>
    /// Controls the whole game loop: main menu, room after room, final screen.
    /// There is only one GameManager in the scene (singleton).
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Tooltip("Rooms in play order. The first is the lobby (tutorial), the last is the finale (back in the lobby).")]
        public RoomManager[] rooms;

        public GameState State { get; private set; } = GameState.MainMenu;
        public int CurrentRoomIndex { get; private set; }
        public RoomManager CurrentRoom => rooms[CurrentRoomIndex];

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            MovePlayerTo(rooms[0]);
            SetState(GameState.MainMenu);
        }

        public void StartNewGame()
        {
            SetState(GameState.Playing);
            EnterRoom(0, false);
        }

        /// <summary>Called by the exit door when the player walks through it.</summary>
        public void GoToNextRoom()
        {
            if (State != GameState.Playing) return;
            int next = CurrentRoomIndex + 1;
            // The player walks through the door: the rooms are next to each other.
            EnterRoom(next, false);
            bool isFinalHall = next == rooms.Length - 1;
            if (isFinalHall) SetState(GameState.Finished);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void EnterRoom(int index, bool movePlayer)
        {
            CurrentRoomIndex = index;
            if (movePlayer) MovePlayerTo(CurrentRoom);
            CurrentRoom.Enter();
        }

        private void MovePlayerTo(RoomManager room)
        {
            if (PlayerRig.Current == null) return;
            PlayerRig.Current.TeleportTo(room.spawnPoint.position, room.spawnPoint.rotation);
        }

        private void SetState(GameState state)
        {
            State = state;
            GameEvents.RaiseGameStateChanged(state);
        }
    }
}
