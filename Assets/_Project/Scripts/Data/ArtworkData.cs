using UnityEngine;

namespace EduGallery
{
    /// <summary>Data of one artwork. Create it with Assets > Create > EduGallery > Artwork.</summary>
    [CreateAssetMenu(menuName = "EduGallery/Artwork", fileName = "Artwork")]
    public class ArtworkData : ScriptableObject
    {
        public string title;
        public string artist;
        public string year;
        public string era;
        [TextArea(3, 8)] public string description;
        public Texture2D image;

        public string Caption => $"{title}\n{artist}, {year}";
    }
}
