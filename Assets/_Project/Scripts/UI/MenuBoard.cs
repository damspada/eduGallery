using UnityEngine;
using UnityEngine.UI;

namespace EduGallery
{
    /// <summary>The start menu, shown on a board in the lobby.</summary>
    public class MenuBoard : MonoBehaviour
    {
        public Text infoLabel;
        public Clickable startButton;
        public Clickable quitButton;
        [Tooltip("Everything that disappears when the game starts.")]
        public GameObject menuRoot;

        private void OnEnable()
        {
            startButton.Clicked += OnStart;
            quitButton.Clicked += OnQuit;
            GameEvents.GameStateChanged += OnGameStateChanged;
        }

        private void OnDisable()
        {
            startButton.Clicked -= OnStart;
            quitButton.Clicked -= OnQuit;
            GameEvents.GameStateChanged -= OnGameStateChanged;
        }

        private void Start()
        {
            infoLabel.text = "Explore four art eras.\nSolve the puzzles to open each door.";
        }

        private void OnStart(Clickable button) => GameManager.Instance.StartNewGame();

        private void OnQuit(Clickable button) => GameManager.Instance.QuitGame();

        private void OnGameStateChanged(GameState state) => menuRoot.SetActive(state == GameState.MainMenu);
    }
}
