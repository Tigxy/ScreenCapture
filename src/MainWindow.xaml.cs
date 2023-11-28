using ScreenCapture.Utils;
using System;
using System.Windows;
using System.Windows.Forms;

namespace ScreenCapture
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotifyIcon NotifyIcon => Instances.Notification;

        private ControlPanel Control => Instances.Control;

        private DisplayWindow Display => Instances.Display;

        private HotKeyMngr _mngr;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            InitializeHotKey();
            Display.Hide();
        }

        private const long _minTickDifference = 1000;
        private long _lastPerformedEvent;
        private int? _hotkey_id;
        private void InitializeHotKey()
        {
            lock (new object())
            {
                if (Control.IsLoaded)
                    InitHotKeyManager();
                else
                    Control.Loaded += (s, e) => { InitHotKeyManager(); };
            }
        }

        private void InitHotKeyManager()
        {
            _mngr = new HotKeyMngr(Control);
            if (Configuration.Settings.IsPrintKeyOverridden) { EnablePrintHotKey(); } else DisablePrintHotKey();
            Configuration.Settings.PrintHotKeyStatusChanged += (status) => { if (status) { EnablePrintHotKey(); } else DisablePrintHotKey(); };
        }

        private void EnablePrintHotKey()
        {
            _hotkey_id = _mngr?.EnableHotKey(
                Configuration.PrintHotKey.key,
                Configuration.PrintHotKey.modifier,
                () =>
                {
                    long current = DateTime.UtcNow.Ticks;
                    if (current - _lastPerformedEvent > _minTickDifference)
                    {
                        _lastPerformedEvent = current;
                        TakeWindow.TakeScreenshot();
                    }
                },
                true);
        }

        private void DisablePrintHotKey()
        {
            if (_hotkey_id is int id)
                _mngr?.DisableHotKey(id);
            System.Diagnostics.Debug.Print("Deactivated hotkey");
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            InitNotify();
            Control.Visibility = UIUtils.BoolToVisibility(Configuration.Settings.IsControlShownOnDesktop);
        }

        private void InitNotify()
        {
            NotifyIcon.DoubleClick += NotifyIcon_Click;
            NotifyIcon.Text = "A screen capturing tool";
            NotifyIcon.Icon = Properties.Resources.camera_filled;
            NotifyIcon.ContextMenu = BuildContextMenu();
            NotifyIcon.Visible = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Configuration.SaveSettings();
            Configuration.SaveSessionSettings();
        }

        private ContextMenu BuildContextMenu()
        {
            var ctxMenu = new ContextMenu();

            var mni_showOnDesktop = new MenuItem() { Text = "Show on desktop", Checked = Configuration.Settings.IsControlShownOnDesktop };
            mni_showOnDesktop.Click += (s, e) =>
            {
                Configuration.Settings.IsControlShownOnDesktop ^= true;
                (s as MenuItem).Checked = Configuration.Settings.IsControlShownOnDesktop;
            };

            var mni_taskScreenShot = new MenuItem() { Text = "Take screenshot" };
            mni_taskScreenShot.Click += (s, e) => { TakeWindow.TakeScreenshot(); };

            var mni_showScreenshotViewer = new MenuItem() { Text = "Screenshot viewer..." };
            mni_showScreenshotViewer.Click += (s, e) => { Display.Visibility = Visibility.Visible; Display.BringIntoView(); };

            var mni_Settings = new MenuItem() { Text = "Settings..." };
            mni_Settings.Click += (s, e) => { new SettingsWindow().Show(); };

            var mni_close = new MenuItem() { Text = "Close" };
            mni_close.Click += (s, e) => { CloseAllWindows(); };

            ctxMenu.MenuItems.Add(mni_showOnDesktop);
            ctxMenu.MenuItems.Add(mni_taskScreenShot);
            ctxMenu.MenuItems.Add(mni_showScreenshotViewer);
            ctxMenu.MenuItems.Add(mni_Settings);
            ctxMenu.MenuItems.Add(mni_close);

            Configuration.Settings.PropertyChanged += (s, args) =>
            {
                if (args.PropertyName == nameof(Configuration.Settings.IsControlShownOnDesktop))
                    mni_showOnDesktop.Checked = Configuration.Settings.IsControlShownOnDesktop;
            };

            return ctxMenu;
        }

        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            Display.Visibility = Visibility.Visible;
        }

        private void CloseAllWindows()
        {
            try
            {
                Control?.Close();
                TakeWindow.Dispose();
                NotifyIcon?.Dispose();
                Display?.Close();
                _mngr?.CleanUpHotKeys();
                this.Close();
            }
            catch (Exception)
            { }
        }
    }
}
