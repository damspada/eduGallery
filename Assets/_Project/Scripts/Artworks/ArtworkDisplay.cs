using UnityEngine;
using UnityEngine.UI;

namespace EduGallery
{
    /// <summary>An artwork on show. When the player selects it, its information panel opens.</summary>
    public class ArtworkDisplay : MonoBehaviour, IInteractable
    {
        public ArtworkData artwork;
        [Tooltip("The panel with the description. Hidden until the artwork is selected.")]
        public GameObject infoPanel;
        public Text infoLabel;

        public string Prompt => infoPanel.activeSelf ? "Close" : $"Read about \"{artwork.title}\"";

        private void Start()
        {
            infoLabel.text = $"<b>{artwork.title}</b>\n{artwork.artist}, {artwork.year}\n\n{artwork.description}";
            infoPanel.SetActive(false);
        }

        public void Interact()
        {
            infoPanel.SetActive(!infoPanel.activeSelf);
            if (infoPanel.activeSelf) GameEvents.RaiseArtworkInspected(artwork);
        }
    }
}
