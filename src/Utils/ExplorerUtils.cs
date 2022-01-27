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
    }
}
