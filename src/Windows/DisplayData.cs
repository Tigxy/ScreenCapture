using ScreenCapture.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace ScreenCapture.Windows
{
    public class DisplayData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged(string property)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

        private static DisplayData _instance;
        public static DisplayData Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (new object())
                    {
                        if (_instance == null)
                            _instance = new DisplayData();
                    }
                }

                return _instance;
            }
        }

        public BitmapImageData LatestScreenShot => ScreenShots.First();
        public ObservableCollection<BitmapImageData> ScreenShots { get; private set; }
            = new ObservableCollection<BitmapImageData>();

        public delegate void NewScreenShot();
        public event NewScreenShot OnNewScreenshot;

        /// <summary>
        /// Constructor for singleton
        /// </summary>
        private DisplayData()
        {
            LoadPrevious();
            Configuration.Settings.ImageLoadCountChanged += AdjustLoadedElements;
        }

        /// <summary>
        /// Adds a newly made screenshot to the collection
        /// </summary>
        /// <param name="screenShot"></param>
        public void AddScreenshot(Bitmap screenShot)
        {
            var targetPath = Path.Combine(
                Configuration.Settings.ImagePath,
                $"{DateTime.Now.Year}{DateTime.Now.Month}{DateTime.Now.Day}_{DateTime.Now.ToLongTimeString().Replace(":", "")}.png");

            try
            {
                using (var sw = new StreamWriter(targetPath))
                    screenShot.Save(sw.BaseStream, ImageFormat.Bmp);

                ScreenShots.Insert(
                    0,
                    new BitmapImageData()
                    {
                        ImagePath = targetPath,
                        Image = screenShot.ToBitmapImage()
                    });

                AdjustLoadedElements();
                screenShot.Dispose();
                OnNewScreenshot?.Invoke();
            }
            catch (Exception e)
            {
                Instances.Notification.ShowBalloonTip(
                    1000,
                    "Failed to store image",
                    e.Message,
                    System.Windows.Forms.ToolTipIcon.Warning);
            }
        }

        public void RemoveScreenshot(string imagePath)
        {
            var data = ScreenShots.Where(s => s.ImagePath == imagePath).FirstOrDefault();

            // If default was returned, image path doesn't match anymore
            if (data.ImagePath == imagePath)
                RemoveScreenshot(data);
        }

        public void RemoveScreenshot(BitmapImage image)
        {
            var data = ScreenShots.Where(s => s.Image == image).FirstOrDefault();

            // If default was returned, image path doesn't match anymore
            if (data.Image == image)
                RemoveScreenshot(image);
        }

        public void RemoveScreenshot(BitmapImageData data)
        {
            if (ScreenShots.Contains(data))
            {
                try
                {
                    File.Delete(data.ImagePath);
                }
                catch (FileNotFoundException) { }
                catch (DirectoryNotFoundException) { }
                catch (ArgumentException) { }
                catch (Exception e)
                {
                    Instances.Notification.ShowBalloonTip(
                        1000,
                        "Failed to delete screenshot",
                        $"{e.Message}",
                        System.Windows.Forms.ToolTipIcon.Warning);
                    return;
                }
                ScreenShots.Remove(data);
                LoadPrevious();
            }
        }

        /// <summary>
        /// Loads all previous collected screenshots
        /// </summary>
        private void LoadPrevious()
        {
            try
            {
                IEnumerable<string> paths = Directory.GetFiles(Configuration.Settings.ImagePath, "*.png", SearchOption.TopDirectoryOnly);
                if (Configuration.Settings.MaximumScreenshotsInMemory != -1)
                    paths = paths
                        .OrderBy(p => p)                                            // Sort by date
                        .Reverse()                                                  // Change so that latest is at the beginning 
                        .Take(Configuration.Settings.MaximumScreenshotsInMemory);   // Select images

                paths = paths.Where(p => !ScreenShots.Any(d => d.ImagePath == p));   // Only select those that aren't yet loaded

                foreach (var imagePath in paths)
                {
                    try
                    {
                        ScreenShots.Add(
                            new BitmapImageData()
                            {
                                ImagePath = imagePath,
                                Image = DrawingUtils.LoadImageIntoMemory(imagePath)
                            });
                    }
                    catch { }
                }
            }
            catch { }
        }

        private void AdjustLoadedElements()
        {
            foreach (var p in ScreenShots)
                if (!File.Exists(p.ImagePath))
                    ScreenShots.Remove(p);

            if (Configuration.Settings.MaximumScreenshotsInMemory != -1 && ScreenShots.Count > Configuration.Settings.MaximumScreenshotsInMemory)
            {
                for (int i = ScreenShots.Count; i > Configuration.Settings.MaximumScreenshotsInMemory; i--)
                    ScreenShots.RemoveAt(i - 1);
            }
            else if (Configuration.Settings.MaximumScreenshotsInMemory != -1 && ScreenShots.Count < Configuration.Settings.MaximumScreenshotsInMemory)
                LoadPrevious();
        }
    }
}
