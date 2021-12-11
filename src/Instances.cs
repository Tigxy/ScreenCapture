using System.Windows.Forms;

namespace ScreenCapture
{
    public static class Instances
    {
        private static NotifyIcon _notification;
        public static NotifyIcon Notification
        {
            get
            {
                if (_notification == null)
                    lock (new object())
                    {
                        if (_notification == null)
                            _notification = new NotifyIcon();
                    }
                return _notification;
            }
        }

        public static ControlPanel Control => ControlPanel.Instance;
        public static DisplayWindow Display => DisplayWindow.Instance;
    }
}
