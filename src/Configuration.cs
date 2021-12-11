using System;
using System.Windows;

namespace ScreenCapture
{
    public static class Configuration
    {
        public static string AppdataPath = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public static string AppName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
        public static string AppStoragePath = System.IO.Path.Combine(AppdataPath, AppName);
        public static string ConfigurationPath = System.IO.Path.Combine(AppStoragePath, "configuration.yaml");
        public static string SessionPath = System.IO.Path.Combine(AppStoragePath, "session.yaml");
        private const string _regPath = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

        public static string DefaultImageLocation = 
            System.IO.Path.Combine(
                System.Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), 
                Configuration.AppName);

        public const int CopyToClipboardTrials = 5;

        public static readonly (System.Windows.Forms.Keys key, HotKeyMngr.ModifierKeys modifier) PrintHotKey =
            (System.Windows.Forms.Keys.PrintScreen, HotKeyMngr.ModifierKeys.MOD_NOREPEAT);

        private static readonly object _lock = new object();

        private static SettingsData _settings;
        public static SettingsData Settings
        {
            get
            {
                if (_settings == null)
                {
                    lock (_lock)
                    {
                        if (_settings == null)
                            _settings = LoadSettings();
                    }
                }

                return _settings;
            }
        }

        private static SessionSettingsData _sessionSettings;
        public static SessionSettingsData SessionSettings
        {
            get
            {
                if (_sessionSettings == null)
                {
                    lock (_lock)
                    {
                        if (_sessionSettings == null)
                            _sessionSettings = LoadSessionSettings();
                    }
                }

                return _sessionSettings;
            }
        }

        public static T Load<T>(string path)
        {
            if (!System.IO.File.Exists(path))
                return default;

            var serializer = new YamlDotNet.Serialization.Deserializer();

            try
            {
                using (var sr = new System.IO.StreamReader(path, false))
                {
                    var des = serializer.Deserialize<T>(sr);
                    return des;
                }
            }
            catch (Exception) { }

            return default;
        }

        public static SettingsData LoadSettings()
        {
            var set = Load<SettingsData>(ConfigurationPath);
            if (set == default)
            {
                set = new SettingsData();
                try
                {
                    System.IO.Directory.CreateDirectory(AppStoragePath);
                    System.IO.Directory.CreateDirectory(set.ImagePath);
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Could not initialize application: {e.Message}");
                }
            }

            if (set.IsAutostartEnabled)
                EnableAutostart();
            else
                DisableAutostart();

            return set;
        }

        public static SessionSettingsData LoadSessionSettings()
        {
            var set = Load<SessionSettingsData>(SessionPath);
            if (set == default)
            {
                set = new SessionSettingsData() { ViewerHeight = 600, ViewerWidth = 800 };
            }
            set.Validate();
            return set;
        }

        public static bool Save<T>(T data, string path)
        {
            var serializer = new YamlDotNet.Serialization.Serializer();

            try
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
                using (var sw = new System.IO.StreamWriter(path, false))
                {
                    serializer.Serialize(sw, data);
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public static bool SaveSettings()
            => Save(Settings, ConfigurationPath);

        public static bool SaveSessionSettings()
            => Save(SessionSettings, SessionPath);

        public static void EnableAutostart()
        {
            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(_regPath, true);
            string str = System.Reflection.Assembly.GetExecutingAssembly().Location;
            key.SetValue(AppName, str);
        }

        public static void DisableAutostart()
        {
            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(_regPath, true);
            _ = System.Reflection.Assembly.GetExecutingAssembly().Location;
            key.DeleteValue(AppName, false);
        }
    }
}
