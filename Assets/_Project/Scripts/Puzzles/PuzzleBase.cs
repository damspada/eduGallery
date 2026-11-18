using UnityEngine;
using UnityEngine.UI;

namespace EduGallery
{
    /// <summary>
    /// Shared logic of every puzzle: state, attempts and time spent.
    /// A new puzzle only has to call <see cref="Solve"/> and <see cref="Fail"/>.
    /// </summary>
    public abstract class PuzzleBase : MonoBehaviour
    {
        [Header("Description")]
        public string puzzleId;
        public string title;
        [TextArea(2, 5)] public string instructions;
        [TextArea(2, 5)] public string hint;
        [TextArea(2, 5)] public string solvedText;
        public ArtworkData artwork;

        [Header("Scene references")]
        [Tooltip("Text near the puzzle that shows instructions, hint and result.")]
        public Text statusLabel;
        [Tooltip("Tutorial puzzles do not change the score.")]
        public bool countsForScore = true;

        public bool IsActive { get; private set; }
        public bool IsSolved { get; private set; }
        public bool HintWasShown { get; private set; }
        public int Attempts { get; private set; }
        /// <summary>Time since the player started working on this puzzle (not since entering the room).</summary>
        public float SecondsSpent { get; private set; }
        /// <summary>True when the hint appeared before the player touched the puzzle: it costs no points.</summary>
        public bool HintIsFree { get; private set; }

        private bool started;

        /// <summary>Called by the room when the player enters it.</summary>
        public void Activate()
        {
            if (IsSolved) return;
            IsActive = true;
            SetStatus(instructions);
            OnActivated();
        }

        protected virtual void OnActivated() { }

        /// <summary>Runs every frame while the puzzle is active and not solved.</summary>
        protected virtual void Tick() { }

        private void Update()
        {
            if (!IsActive || IsSolved) return;
            // A room has several puzzles: the clock of a puzzle runs only once the player works on it.
            if (started) SecondsSpent += Time.deltaTime;
            Tick();
        }

        /// <summary>Puzzles call this at the first action of the player on them.</summary>
        protected void MarkStarted() => started = true;

        protected void Solve()
        {
            if (IsSolved) return;
            IsSolved = true;
            SetStatus(string.IsNullOrEmpty(solvedText) ? "Solved!" : solvedText);
            GameEvents.RaisePuzzleSolved(this);
        }

        protected void Fail()
        {
            if (IsSolved) return;
            MarkStarted();
            Attempts++;
            GameEvents.RaiseWrongAnswer(this);
        }

        protected void SetStatus(string text)
        {
            if (statusLabel != null) statusLabel.text = text;
        }
    }
}
