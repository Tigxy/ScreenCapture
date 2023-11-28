using ScreenCapture.Windows;
using System;
using System.Windows;
using System.Windows.Controls;
using ScreenCapture.Utils;

namespace ScreenCapture
{
    /// <summary>
    /// Interaction logic for ScreenshotDisplay_Window.xaml
    /// </summary>
    public partial class DisplayWindow : Window
    {
        private static DisplayWindow _lazyDisplay;
        public static DisplayWindow Instance
        {
            get
            {
                if (_lazyDisplay == null)
                    lock (new object())
                    {
                        if (_lazyDisplay == null)
                        {
                            _lazyDisplay = new DisplayWindow() { Visibility = Visibility.Hidden };
                        }
                    }
                return _lazyDisplay;
            }
        }

        private string _currentImagePath;

        private DisplayWindow()
        {
            InitializeComponent();
            this.Title = Configuration.AppName;
            this.DataContext = DisplayData.Instance;
            DisplayData.Instance.OnNewScreenshot += OnNewScreenshot;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SelectAndDisplay(0);
            PrevScreenshotsView.Focus();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            this.Height = Configuration.SessionSettings.ViewerHeight;
            this.Width = Configuration.SessionSettings.ViewerWidth;
            this.Top = Configuration.SessionSettings.ViewerTop;
            this.Left = Configuration.SessionSettings.ViewerLeft;
        }

        new public void Close()
        {
            _closeByMethod = true;
            base.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Visibility = Visibility.Hidden;

            // Prevent closure by the 'Close' button as we want to keep our 
            // display running in the background
            if (!_closeByMethod)
                e.Cancel = true;

            Configuration.SessionSettings.ViewerHeight = this.Height;
            Configuration.SessionSettings.ViewerWidth = this.Width;
            Configuration.SessionSettings.ViewerTop = this.Top;
            Configuration.SessionSettings.ViewerLeft = this.Left;
        }

        private void OnNewScreenshot()
        {
            SelectAndDisplay(0);

            if (Configuration.Settings.IsAutomaticPasteToClipboardEnabled)
                CopyToClipboard(_currentImagePath);
        }

        private bool _closeByMethod;

        private void SelectAndDisplay(int idx = -1)
        {
            var selectedIdx =
                idx == -1
                ? Math.Max(0, PrevScreenshotsView.SelectedIndex)
                : Math.Min(idx, DisplayData.Instance.ScreenShots.Count - 1);

            if (idx != -1)
                PrevScreenshotsView.SelectedIndex = selectedIdx;

            if (DisplayData.Instance.ScreenShots.Count == 0)
            {
                CurrentlyDisplayedImage.Source = null;
                return;
            }

            CurrentlyDisplayedImage.Source = DisplayData.Instance.ScreenShots[selectedIdx].Image;
            _currentImagePath = DisplayData.Instance.ScreenShots[selectedIdx].ImagePath;
            PrevScreenshotsView.Focus();
        }

        private void ListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.OriginalSource is ListView lv) {
                SelectAndDisplay(lv.SelectedIndex);
            }
        }
        
        private void New_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            System.Threading.Thread.Sleep(400);
            TakeWindow.TakeScreenshot(true);

        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            CopyToClipboard(_currentImagePath, true);
        }

        private void Settings_Click(object sender, RoutedEventArgs e) { new SettingsWindow().Show(); }

        private void CopyToClipboard(string imagePath, bool fromUserInteraction = false, bool copyPath=false)
        {
            bool wasSuccessful = false; ;

            for (int i = 0; i < Configuration.CopyToClipboardTrials; i++)
            {
                try
                {
                    wasSuccessful = true;
                    if (copyPath)
                        System.Windows.Forms.Clipboard.SetText(imagePath);
                    else
                        using (var img = System.Drawing.Image.FromFile(imagePath))
                            System.Windows.Forms.Clipboard.SetImage(img);
                    break;
                }
                catch (System.Runtime.InteropServices.ExternalException)
                {
                    wasSuccessful = false;
                    System.Threading.Thread.Sleep(20);
                }
                catch { }
            }

            if (wasSuccessful)
            {
                if (!Configuration.Settings.IsCopiedNotificationHidden || fromUserInteraction)
                    Instances.Notification.ShowBalloonTip(
                        1000,
                        "Clipboard",
                        (copyPath ? "Image path" : "Image") + " was copied to clipboard",
                        System.Windows.Forms.ToolTipIcon.Info);
            }
            else
                Instances.Notification.ShowBalloonTip(
                    1000,
                    "Clipboard",
                    "Failed to copy " + (copyPath ? "image path" : "image") + "to clipboard (Clipboard was in use)",
                    System.Windows.Forms.ToolTipIcon.Info);
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is BitmapImageData data)
            {
                DisplayData.Instance.RemoveScreenshot(data);

                if (PrevScreenshotsView.SelectedValue is BitmapImageData bmiData
                    && (bmiData.Image != data.Image
                    || bmiData.ImagePath != data.ImagePath))
                    SelectAndDisplay(PrevScreenshotsView.SelectedIndex);
                else
                    SelectAndDisplay();
            }
        }

        new public void Show()
        {
            System.Diagnostics.Debug.WriteLine("Show on display");
            base.Show();
        }
        new public void Hide()
        {
            System.Diagnostics.Debug.WriteLine("Hide on display");
            base.Hide();
        }

        private void CopyPath_Click(object sender, RoutedEventArgs e)
        {
            CopyToClipboard(_currentImagePath, copyPath: true);
        }

        private void OpenImage_Click(object sender, RoutedEventArgs e)
        {
            bool result = ExplorerUtils.OpenInDefault(_currentImagePath);
            if (!result) {
                Instances.Notification.ShowBalloonTip(
                    1000,
                    Configuration.AppName,
                    $"Failed to open image '{_currentImagePath}'",
                    System.Windows.Forms.ToolTipIcon.Info);
            }
        }

        private void OpenInExplorer_Click(object sender, RoutedEventArgs e) {
            ExplorerUtils.ShowInExplorer(_currentImagePath);
        }
    }
}
