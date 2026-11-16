namespace EduGallery
{
    /// <summary>Anything the player can click (desktop) or select with a controller (VR).</summary>
    public interface IInteractable
    {
        /// <summary>Short text shown to the player, e.g. "Read about the painting".</summary>
        string Prompt { get; }

        void Interact();
    }
}
