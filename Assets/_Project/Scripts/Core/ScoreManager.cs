using System.Collections.Generic;
using UnityEngine;

namespace EduGallery
{
    /// <summary>Keeps the score. It only listens to events, it does not know the puzzles.</summary>
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        [Header("Points")]
        [SerializeField] private int pointsForSolving = 100;
        [SerializeField] private int maxSpeedBonus = 50;
        [Tooltip("The speed bonus goes down to zero after this many seconds.")]
        [SerializeField] private float speedBonusSeconds = 90f;
        [SerializeField] private int penaltyForWrongAnswer = 10;
        [SerializeField] private int penaltyForHint = 20;
        [SerializeField] private int pointsForReadingArtwork = 10;

        public int Score { get; private set; }

        private readonly HashSet<ArtworkData> artworksRead = new HashSet<ArtworkData>();

        private void Awake() => Instance = this;

        private void OnEnable()
        {
            GameEvents.PuzzleSolved += OnPuzzleSolved;
            GameEvents.WrongAnswer += OnWrongAnswer;
            GameEvents.HintShown += OnHintShown;
            GameEvents.ArtworkInspected += OnArtworkInspected;
        }

        private void OnDisable()
        {
            GameEvents.PuzzleSolved -= OnPuzzleSolved;
            GameEvents.WrongAnswer -= OnWrongAnswer;
            GameEvents.HintShown -= OnHintShown;
            GameEvents.ArtworkInspected -= OnArtworkInspected;
        }

        public void SetScore(int value)
        {
            Score = Mathf.Max(0, value);
            GameEvents.RaiseScoreChanged(Score);
        }

        private void OnPuzzleSolved(PuzzleBase puzzle)
        {
            if (!puzzle.countsForScore) return;
            float speed = 1f - Mathf.Clamp01(puzzle.SecondsSpent / speedBonusSeconds);
            SetScore(Score + pointsForSolving + Mathf.RoundToInt(maxSpeedBonus * speed));
        }

        private void OnWrongAnswer(PuzzleBase puzzle)
        {
            if (puzzle.countsForScore) SetScore(Score - penaltyForWrongAnswer);
        }

        private void OnHintShown(PuzzleBase puzzle)
        {
            if (puzzle.countsForScore && !puzzle.HintIsFree) SetScore(Score - penaltyForHint);
        }

        private void OnArtworkInspected(ArtworkData artwork)
        {
            // Reading about an artwork gives points only the first time.
            if (artworksRead.Add(artwork)) SetScore(Score + pointsForReadingArtwork);
        }
    }
}
