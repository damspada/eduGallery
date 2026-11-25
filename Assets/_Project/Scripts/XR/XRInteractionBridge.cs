using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace EduGallery
{
    /// <summary>
    /// Connects the game objects, made for the mouse, to the VR controllers.
    /// It adds the XRI components at runtime, so the prefabs and scenes of the team do not change:
    /// - <see cref="Grabbable"/> → XRGrabInteractable (grip to take, release to drop or snap in a socket)
    /// - <see cref="IInteractable"/> (buttons, artworks) → XRSimpleInteractable (grip on it to use it)
    /// - <see cref="Highlightable"/> → glows while a controller points at it
    /// </summary>
    public static class XRInteractionBridge
    {
        public static void ConnectScene()
        {
            foreach (var grabbable in Object.FindObjectsByType<Grabbable>(FindObjectsSortMode.None))
                Connect(grabbable);

            foreach (var behaviour in Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
                if (behaviour is IInteractable interactable) Connect(behaviour, interactable);
        }

        private static void Connect(Grabbable grabbable)
        {
            if (grabbable.GetComponent<XRBaseInteractable>() != null) return;

            var grab = grabbable.gameObject.AddComponent<XRGrabInteractable>();
            grab.throwOnDetach = true;
            grab.enabled = grabbable.CanBeGrabbed;
            grabbable.GrabAllowedChanged += g => grab.enabled = g.CanBeGrabbed;

            grab.selectEntered.AddListener(_ => grabbable.NotifyGrabbed());
            grab.selectExited.AddListener(_ =>
            {
                // XRI gives the object back the physics it had when grabbed. Taken out of a socket,
                // that was "kinematic", and it would float in the air: let it fall, like on desktop.
                grabbable.Body.isKinematic = false;
                grabbable.NotifyReleased();
            });

            AddHighlight(grab);
        }

        private static void Connect(MonoBehaviour behaviour, IInteractable interactable)
        {
            if (behaviour.GetComponent<XRBaseInteractable>() != null) return;

            var simple = behaviour.gameObject.AddComponent<XRSimpleInteractable>();
            simple.selectEntered.AddListener(_ => interactable.Interact());
            AddHighlight(simple);
        }

        private static void AddHighlight(XRBaseInteractable interactable)
        {
            var highlight = interactable.GetComponentInParent<Highlightable>();
            if (highlight == null) return;
            interactable.hoverEntered.AddListener(_ => highlight.SetHighlighted(true));
            interactable.hoverExited.AddListener(_ =>
            {
                if (!interactable.isHovered) highlight.SetHighlighted(false);
            });
        }
    }
}
