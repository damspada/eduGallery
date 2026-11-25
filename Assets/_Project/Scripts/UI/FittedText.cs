using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace EduGallery
{
    /// <summary>
    /// Keeps a world text as big as its card allows. Every time the text changes, it picks the
    /// biggest letter size that still fits (up to "maxSize"), so short messages are easy to read
    /// from far away and long ones never overflow.
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class FittedText : MonoBehaviour
    {
        [Tooltip("Biggest letter size allowed, in canvas units.")]
        public int maxSize = 30;
        [Tooltip("Average width of a letter, as a part of its size (wider for bold fonts).")]
        public float letterWidth = 0.52f;

        private const int MinSize = 5;
        private const float LineHeight = 1.32f;
        private const int LetterPixels = 96;
        private static readonly System.Collections.Generic.HashSet<Font> warmFonts =
            new System.Collections.Generic.HashSet<Font>();
        private Text label;
        private string shown;

        [Tooltip("The letter size is chosen once and stays the same when the text changes (displays).")]
        public bool keepSize;

        private void LateUpdate()
        {
            if (keepSize) return;
            if (label == null) label = GetComponent<Text>();
            if (label.text != shown) Apply();
        }

        /// <summary>Recomputes the letter size for the current text.</summary>
        public void Apply()
        {
            if (label == null) label = GetComponent<Text>();
            shown = label.text;
            Vector2 box = ((RectTransform)transform).rect.size;
            string plain = Regex.Replace(shown ?? "", "<.*?>", "");

            int size = maxSize;
            while (size > MinSize && Lines(plain, box.x / (letterWidth * size)) * size * LineHeight > box.y) size--;
            label.fontSize = size;

            Sharpen(label);
        }

        /// <summary>
        /// Every text of the game is drawn with letters of exactly the same number of pixels, whatever its
        /// size in the world. So the font texture holds one copy of each letter, it is filled once
        /// (see Warm) and never has to be rebuilt while playing: a rebuild is what made letters look broken.
        /// </summary>
        public static void Sharpen(Text text)
        {
            var scaler = text.GetComponentInParent<CanvasScaler>();
            if (scaler != null) scaler.dynamicPixelsPerUnit = (float)LetterPixels / text.fontSize;
        }

        /// <summary>Loads all the letters of a font in its texture, once per font.</summary>
        public static void Warm(Font font)
        {
            if (font == null || !font.dynamic || !warmFonts.Add(font)) return;
            var letters = new System.Text.StringBuilder();
            for (char letter = ' '; letter <= '~'; letter++) letters.Append(letter);
            for (char letter = '\u00a1'; letter <= '\u00ff'; letter++) letters.Append(letter);
            letters.Append("\u2018\u2019\u201c\u201d\u2013\u2014\u2026\u2022");
            foreach (FontStyle style in new[] { FontStyle.Normal, FontStyle.Bold, FontStyle.Italic })
                font.RequestCharactersInTexture(letters.ToString(), LetterPixels, style);
        }

        private void Awake() => Warm(GetComponent<Text>().font);

        /// <summary>Width of a word in average letters: capital letters and digits are wider.</summary>
        private static float Width(string word)
        {
            float width = 0f;
            foreach (char letter in word) width += char.IsUpper(letter) || char.IsDigit(letter) ? 1.35f : 1f;
            return width;
        }

        /// <summary>Number of lines the text needs when a line holds "lineLength" letters.</summary>
        private static int Lines(string content, float lineLength)
        {
            int lines = 0;
            foreach (string paragraph in content.Split('\n'))
            {
                lines++;
                float used = 0f;
                foreach (string word in paragraph.Split(' '))
                {
                    float length = Width(word);
                    if (length > lineLength) return int.MaxValue / 1024;
                    if (used > 0f && used + 1 + length > lineLength)
                    {
                        lines++;
                        used = 0f;
                    }
                    used += (used > 0f ? 1 : 0) + length;
                }
            }
            return lines;
        }
    }
}
