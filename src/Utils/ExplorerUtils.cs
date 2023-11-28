using System.Diagnostics;

namespace ScreenCapture.Utils
{
    public static class ExplorerUtils
    {
        public static void ShowInExplorer(string path)
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = "explorer.exe",
                Arguments = $"/select,\"{path}\"" 
            });
        }

        public static bool OpenInDefault(string path) {
            if (System.IO.File.Exists(path)) {
                try {
                    Process.Start(new ProcessStartInfo() {
                        FileName = path
                    });
                    return true;
                }
                catch (System.ComponentModel.Win32Exception) {
                    return false;
                }
            }
            return false;
        }
    }
}
