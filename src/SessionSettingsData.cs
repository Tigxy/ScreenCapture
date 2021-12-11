using System;

namespace ScreenCapture
{
    public class SessionSettingsData
    {
        public double ViewerLeft;
        public double ViewerTop;
        public double ViewerWidth;
        public double ViewerHeight;

        public double SettingsLeft;
        public double SettingsTop;

        public bool IsDesktopControlExpanded = true;

        /// <summary>
        /// Validates whether the values are feasable and if not, adjusts them
        /// </summary>
        public void Validate()
        {
            ViewerWidth = Math.Max(300, ViewerWidth);
            ViewerHeight = Math.Max(300, ViewerHeight);
        }
    }
}
