using UnityEngine;

namespace EduGallery
{
    /// <summary>Score, time and room name in the corner of the screen (desktop only).</summary>
    public class DesktopHud : MonoBehaviour
    {
        private void OnGUI()
        {
            var game = GameManager.Instance;
            if (game == null || game.State != GameState.Playing) return;

            var style = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold };
            string text = $"{game.CurrentRoom.roomName}\n" +
                          $"Score: {ScoreManager.Instance.Score}\n" +
                          $"Time: {TimeFormat.Clock(game.ElapsedSeconds)}";
            GUI.Label(new Rect(16, 12, 400, 90), text, style);
        }
    }
}
