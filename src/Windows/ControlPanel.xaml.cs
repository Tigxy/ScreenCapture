using ScreenCapture.Utils;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;

namespace ScreenCapture
{
    /// <summary>
    /// Interaction logic for ControlPanel.xaml
    /// </summary>
    public partial class ControlPanel : Window
    {
        private static ControlPanel _lazyControl;
        public static ControlPanel Instance
        {
            get
            {
                if (_lazyControl == null)
                    lock (new object())
                    {
                        if (_lazyControl == null)
                            _lazyControl = new ControlPanel();
                    }
                return _lazyControl;
            }
        }

        private ControlPanel()
        {
            System.Diagnostics.Debug.WriteLine("New Display");
            InitializeComponent();
            this.DataContext = Configuration.Settings;
            this.Hide();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.SizeToContent = SizeToContent.Manual;
            this.SizeToContent = SizeToContent.WidthAndHeight;
            HideFromAltTab();
            AdjustOnScreen();

            Configuration.Settings.ControlSettingsChanged += () => AdjustOnScreen();
            Configuration.Settings.PropertyChanged += (s, args) =>
            {
                if (args.PropertyName == nameof(Configuration.Settings.IsControlShownOnDesktop))
                    this.Visibility = UIUtils.BoolToVisibility(Configuration.Settings.IsControlShownOnDesktop);
            };
        }

        public void AdjustOnScreen()
        {
            Screen usedScreen;
            if (Screen.AllScreens.Length == 0)
                return;

            usedScreen = Screen.AllScreens[Math.Min(Math.Max(0, Configuration.Settings.ScreenNumber), Screen.AllScreens.Length - 1)];

            this.Top = usedScreen.Bounds.Top + ((usedScreen.Bounds.Height - this.ActualHeight) / 2);

            if (Configuration.Settings.IsControlShownOnLeftSide)
                this.Left = usedScreen.Bounds.Left;
            else
                this.Left = usedScreen.Bounds.Right - this.ActualWidth;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Configuration.Settings.IsControlShownOnDesktop = false;
        }

        private void NewBtn_Click(object sender, RoutedEventArgs e)
        {
            TakeWindow.TakeScreenshot();
        }

        private void SettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            new SettingsWindow().Show();
        }
    }
}
