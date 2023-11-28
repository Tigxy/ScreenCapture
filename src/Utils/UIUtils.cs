using System.Windows;

namespace ScreenCapture.Utils
{
    public static class UIUtils
    {
        public static Visibility BoolToVisibility(bool value, bool invert = false)
        {
            return value ^ invert ? Visibility.Visible : Visibility.Hidden;
        }
    }
}
