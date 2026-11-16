using UnityEngine;

namespace EduGallery
{
    /// <summary>
    /// Makes an object glow a little when the player points at it,
    /// so it is clear what can be grabbed or pressed.
    /// </summary>
    public class Highlightable : MonoBehaviour
    {
        [SerializeField] private Color glow = new Color(0.35f, 0.3f, 0.12f);

        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        private Renderer[] renderers;
        private Color[] ownGlow;   // the light a material already gives (the golden key, a lamp)
        private MaterialPropertyBlock block;
        private bool highlighted;

        private void Awake()
        {
            renderers = GetComponentsInChildren<Renderer>();
            ownGlow = new Color[renderers.Length];
            block = new MaterialPropertyBlock();
            for (int i = 0; i < renderers.Length; i++)
            {
                Material shared = renderers[i].sharedMaterial;
                if (shared != null && shared.IsKeywordEnabled("_EMISSION") && shared.HasProperty(EmissionColor))
                    ownGlow[i] = shared.GetColor(EmissionColor);
                // The glow uses the emission of the material, which must be switched on once.
                foreach (Material material in renderers[i].materials) material.EnableKeyword("_EMISSION");
            }
        }

        public void SetHighlighted(bool value)
        {
            if (highlighted == value || renderers == null) return;
            highlighted = value;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer target = renderers[i];
                if (target == null) continue;
                target.GetPropertyBlock(block);
                block.SetColor(EmissionColor, value ? ownGlow[i] + glow : ownGlow[i]);
                target.SetPropertyBlock(block);
            }
        }
    }
}
