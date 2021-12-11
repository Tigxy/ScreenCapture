using System;
using System.ComponentModel;

namespace ScreenCapture
{
    public class SettingsData : INotifyPropertyChanged
    {
        public string ImagePath { get; set; } = Configuration.DefaultImageLocation;

        private bool _isAutostartEnabled = true;
        public bool IsAutostartEnabled
        {
            get => _isAutostartEnabled;
            set
            {
                if (_isAutostartEnabled == value)
                    return;

                if (value)
                    Configuration.EnableAutostart();
                else
                    Configuration.DisableAutostart();

                _isAutostartEnabled = value;
                OnPropertyChanged(nameof(IsAutostartEnabled));
            }
        }

        private bool _isPrintKeyOverridden = false;
        public bool IsPrintKeyOverridden
        {
            get => _isPrintKeyOverridden;
            set
            {
                if (_isPrintKeyOverridden == value)
                    return;

                _isPrintKeyOverridden = value;
                PrintHotKeyStatusChanged?.Invoke(_isPrintKeyOverridden);
            }
        }

        public bool IsScreenshotViewShownOnNewScreenshot { get; set; } = true;

        public bool IsAutomaticPasteToClipboardEnabled { get; set; } = true;

        public bool IsCopiedNotificationHidden { get; set; } = false;

        public bool IsSelectionLinesShowed { get; set; } = true;

        public bool IsZoomBubbleShown { get; set; } = true;

        public int MinimumScreenshotSpanXY { get; set; } = 5;

        private bool _isControlShownOnDesktop = true;
        public bool IsControlShownOnDesktop
        {
            get => _isControlShownOnDesktop;
            set
            {
                _isControlShownOnDesktop = value;
                OnPropertyChanged(nameof(IsControlShownOnDesktop));
            }
        }

        private bool _isControlShownOnLeftSide = false;
        public bool IsControlShownOnLeftSide
        {
            get => _isControlShownOnLeftSide;
            set
            {
                _isControlShownOnLeftSide = value;
                ControlSettingsChanged?.Invoke();
            }
        }

        private int _screenNumber;
        public int ScreenNumber
        {
            get => _screenNumber;
            set
            {
                _screenNumber = value;
                ControlSettingsChanged?.Invoke();
            }
        }

        private int _maximumScreenshotsInMemory = 10;
        public int MaximumScreenshotsInMemory
        {
            get => _maximumScreenshotsInMemory;
            set
            {
                _maximumScreenshotsInMemory = value;
                ImageLoadCountChanged?.Invoke();
            }
        }

        public delegate void HotKeyStatusChangedEvent(bool override_enabled);
        public event HotKeyStatusChangedEvent PrintHotKeyStatusChanged;

        public delegate void ControlSettingsChangedEvent();
        public event ControlSettingsChangedEvent ControlSettingsChanged;

        public delegate void ImageLoadCountChangedEvent();
        public event ImageLoadCountChangedEvent ImageLoadCountChanged;

        public virtual void OnPropertyChanged(string property)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
