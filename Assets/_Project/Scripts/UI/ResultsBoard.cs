using UnityEngine;
using UnityEngine.UI;

namespace EduGallery
{
    /// <summary>The final screen: score and time.</summary>
    public class ResultsBoard : MonoBehaviour
    {
        public Text summaryLabel;
        public Clickable playAgainButton;
        public Clickable quitButton;
        [Tooltip("The boards and buttons of the results: hidden until the game is finished.")]
        public GameObject resultsRoot;
        [Tooltip("What the results replace on the wall.")]
        public GameObject hideWhenFinished;

        private void Start()
        {
            if (resultsRoot != null) resultsRoot.SetActive(false);
        }

        private void OnEnable()
        {
            playAgainButton.Clicked += OnPlayAgain;
            quitButton.Clicked += OnQuit;
            GameEvents.GameStateChanged += OnGameStateChanged;
        }

        private void OnDisable()
        {
            playAgainButton.Clicked -= OnPlayAgain;
            quitButton.Clicked -= OnQuit;
            GameEvents.GameStateChanged -= OnGameStateChanged;
        }

        private void OnPlayAgain(Clickable button) => GameManager.Instance.RestartGame();

        private void OnQuit(Clickable button) => GameManager.Instance.QuitGame();

        private void OnGameStateChanged(GameState state)
        {
            if (state != GameState.Finished) return;
            if (resultsRoot != null) resultsRoot.SetActive(true);
            if (hideWhenFinished != null) hideWhenFinished.SetActive(false);
            ShowResults();
        }

        private void ShowResults()
        {
            summaryLabel.text =
                $"You escaped the museum!\n\n" +
                $"Score: {ScoreManager.Instance.Score}\n" +
                $"Time: {TimeFormat.Clock(GameManager.Instance.ElapsedSeconds)}";

        }
    }
}
