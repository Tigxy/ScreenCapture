using System;
using System.IO;
using System.Windows;

namespace ScreenCapture
{
    /// <summary>
    /// Interaction logic for Settings_Window.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            this.Title = Configuration.AppName + " - Settings";
            this.Left = Configuration.SessionSettings.SettingsLeft;
            this.Top = Configuration.SessionSettings.SettingsTop;
            this.DataContext = Configuration.Settings;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Configuration.SessionSettings.SettingsLeft = this.Left;
            Configuration.SessionSettings.SettingsTop = this.Top;

            Configuration.SaveSettings();
            Configuration.SaveSessionSettings();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Configuration.Settings.ImagePath))
                Configuration.Settings.ImagePath = Configuration.DefaultImageLocation;

            try
            {
                Directory.CreateDirectory(Configuration.Settings.ImagePath);
            }
            catch (PathTooLongException)
            {
                MessageBox.Show("The specified path of the storage directory is too long");
                return;
            }
            catch
            {
                MessageBox.Show("Could not create image storage directory");
                return;
            }

            this.Close();
        }
    }
}
