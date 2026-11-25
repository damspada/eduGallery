namespace EduGallery
{
    public static class TimeFormat
    {
        /// <summary>Turns seconds into "mm:ss".</summary>
        public static string Clock(float seconds)
        {
            int total = (int)seconds;
            return $"{total / 60:00}:{total % 60:00}";
        }
    }
}
