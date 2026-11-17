using UnityEngine;
using UnityEngine.UI;

namespace EduGallery
{
    /// <summary>
    /// A sentence about an artwork with a missing word.
    /// The player places the right word tile in the socket.
    /// </summary>
    public class WordTilePuzzle : PuzzleBase
    {
        [Header("Word tile puzzle")]
        [Tooltip("Use ____ where the word is missing.")]
        [TextArea(2, 4)] public string sentence;
        public string correctWord;
        public SnapSocket socket;
        public Text sentenceLabel;
        [SerializeField] private float ejectDelay = 0.8f;

        private const string Blank = "____";

        private void OnEnable() => socket.Snapped += OnTilePlaced;

        private void OnDisable() => socket.Snapped -= OnTilePlaced;

        private void Start() => sentenceLabel.text = sentence;

        private void OnTilePlaced(SnapSocket placedSocket, Grabbable tile)
        {
            if (tile.id == correctWord)
            {
                socket.Locked = true;
                sentenceLabel.text = sentence.Replace(Blank, correctWord.ToUpper());
                Solve();
            }
            else
            {
                Fail();
                Invoke(nameof(EjectWrongTile), ejectDelay);
            }
        }

        private void EjectWrongTile()
        {
            if (!IsSolved) socket.Eject();
        }
    }
}
