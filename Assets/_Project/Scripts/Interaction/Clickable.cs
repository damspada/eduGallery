using System;
using UnityEngine;

namespace EduGallery
{
    /// <summary>A simple 3D button. Other scripts subscribe to <see cref="Clicked"/>.</summary>
    public class Clickable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string prompt = "Press";

        public event Action<Clickable> Clicked;

        public string Prompt => prompt;

        public void SetPrompt(string text) => prompt = text;

        public void Interact() => Clicked?.Invoke(this);
    }
}
