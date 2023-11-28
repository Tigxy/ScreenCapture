using ScreenCapture.Utils;
using ScreenCapture.Windows;
using System;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;

namespace ScreenCapture
{
    /// <summary>
    /// Interaction logic for ScreenShotTake_Window.xaml
    /// </summary>
    public partial class TakeWindow : Window
    {
        Bitmap _bmpScreenshot;
        System.Drawing.Rectangle _screenSpan;
        System.Windows.Point _firstPoint;
        System.Windows.Point _secondPoint;

        private TakeWindow()
        {
            InitializeComponent();
        }

        private void ScreenShotDisplay_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"Detected key down: {sender} {e.Key}");
            if (e.Key == Key.Escape || e.Key == Key.LWin || e.Key == Key.RWin || e.Key == Key.Tab
                || e.Key == Key.System)
                this.FinishScreenshot();
        }

        private void Window_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            this.FinishScreenshot();
        }

        private void CollectScreenData()
        {
            _screenSpan = DetermineScreenRange();
            DisplayWindow();

            // Create a new bitmap.
            _bmpScreenshot = new System.Drawing.Bitmap(
                _screenSpan.Width,
                _screenSpan.Height,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            // Create a graphics object from the bitmap.
            using (var gfxScreenshot = Graphics.FromImage(_bmpScreenshot))
            {
                // Take the screenshot from the upper left corner to the right bottom corner.
                gfxScreenshot.CopyFromScreen(
                    _screenSpan.X,
                    _screenSpan.Y,
                    0,
                    0,
                    _bmpScreenshot.Size,
                    CopyPixelOperation.SourceCopy);

                var bmi = _bmpScreenshot.ToBitmapImage();
                ScreenShotDisplay.Source = bmi;
            }

            DisplayWindow();
            AdjustRectangle();
        }

        private double getScaling() {
            uint dpi = DPIUtil.GetDpi(null, new DPIUtil.POINTSTRUCT((int)_secondPoint.X, (int)_secondPoint.Y));
            return dpi / 96;
        }

        private Rectangle DetermineScreenRange()
        {
            var rect = new Rectangle()
            {
                X = Screen.AllScreens.Select(s => s.Bounds.Left).Min(),
                Y = Screen.AllScreens.Select(s => s.Bounds.Top).Min(),
            };

            rect.Width = Screen.AllScreens.Select(s => s.Bounds.Right).Max() - rect.X;
            rect.Height = Screen.AllScreens.Select(s => s.Bounds.Bottom).Max() - rect.Y;

            return rect;
        }

        private (System.Windows.Point, System.Windows.Point) GetMinMaxPoints() {
            var pnt_1 = new System.Windows.Point(Math.Min(_firstPoint.X, _secondPoint.X), Math.Min(_firstPoint.Y, _secondPoint.Y));
            var pnt_2 = new System.Windows.Point(Math.Max(_firstPoint.X, _secondPoint.X), Math.Max(_firstPoint.Y, _secondPoint.Y));
            return (pnt_1, pnt_2);
        }


        private void AdjustRectangle()
        {
            var (pnt_1, pnt_2) = GetMinMaxPoints();

            Rect_Left.Visibility = Visibility.Visible;
            Rect_Right.Visibility = Visibility.Visible;
            Rect_Top.Visibility = Visibility.Visible;
            Rect_Bottom.Visibility = Visibility.Visible;

            //Rect_Left.Visibility = Visibility.Hidden;
            //Rect_Right.Visibility = Visibility.Hidden;
            //Rect_Top.Visibility = Visibility.Hidden;
            //Rect_Bottom.Visibility = Visibility.Hidden;

            /*
             *  ------------------------
             * |           top          |
             *  ------------------------
             * |      |          |      |
             * |left  |  screen  | right|
             * |      |          |      |
             *  ------------------------
             * |         bottom         |                
             *  ------------------------
             */

            // position left rectangle
            Rect_Left.Height = pnt_2.Y - pnt_1.Y;
            Rect_Left.Width = pnt_1.X;
            Canvas.SetLeft(Rect_Left, 0);
            Canvas.SetTop(Rect_Left, pnt_1.Y);

            // position right rectangle
            Rect_Right.Height = pnt_2.Y - pnt_1.Y;
            Rect_Right.Width = _screenSpan.Width - pnt_2.X;
            Canvas.SetLeft(Rect_Right, pnt_2.X);
            Canvas.SetTop(Rect_Right, pnt_1.Y);

            // position top rectangle
            Rect_Top.Height = pnt_1.Y;
            Rect_Top.Width = _screenSpan.Width;
            Canvas.SetLeft(Rect_Top, 0);
            Canvas.SetTop(Rect_Top, 0);

            // position bottom rectangle
            Rect_Bottom.Height = _screenSpan.Height - pnt_2.Y;
            Rect_Bottom.Width = _screenSpan.Width;
            Canvas.SetLeft(Rect_Bottom, 0);
            Canvas.SetTop(Rect_Bottom, pnt_2.Y);

            //System.Diagnostics.Debug.WriteLine($"==========================================");
            //System.Diagnostics.Debug.WriteLine($"Mouse: pnt_1: ({pnt_1.X} / {pnt_1.Y}), pnt_2: ({pnt_2.X} / {pnt_2.Y})");
            //System.Diagnostics.Debug.WriteLine($"Difference: X: {pnt_2.X - pnt_1.X}, Y: {pnt_2.Y - pnt_1.Y}");
            //System.Diagnostics.Debug.WriteLine($"Rect_Left: Width: {Rect_Left.Width} Height: {Rect_Left.Height}");
            //System.Diagnostics.Debug.WriteLine($"Rect_Right: Width: {Rect_Right.Width} Height: {Rect_Right.Height}");
            //System.Diagnostics.Debug.WriteLine($"Rect_Top: Width: {Rect_Top.Width} Height: {Rect_Top.Height}");
            //System.Diagnostics.Debug.WriteLine($"Rect_Bottom: Width: {Rect_Bottom.Width} Height: {Rect_Bottom.Height}");
        }

        private void AdjustSelectionLines(System.Windows.Point mousePosition)
            => AdjustSelectionLines(mousePosition.X, mousePosition.Y);

        private void AdjustSelectionLines(double mouse_x, double mouse_y)
        {
            HorizontalSelectionLine.Visibility = UIUtils.BoolToVisibility(Configuration.Settings.IsSelectionLinesShowed);
            VerticalSelectionLine.Visibility = UIUtils.BoolToVisibility(Configuration.Settings.IsSelectionLinesShowed);

            if (!Configuration.Settings.IsSelectionLinesShowed)
                return;

            Canvas.SetLeft(VerticalSelectionLine, mouse_x);
            Canvas.SetTop(HorizontalSelectionLine, mouse_y);
            //System.Diagnostics.Debug.WriteLine(VerticalSelectionLine.Width);
            //System.Diagnostics.Debug.WriteLine(VerticalSelectionLine.Height);
        }

        private void PreparePrompt() {
            // ensure that prompt is visible
            ActionPrompt.Visibility = Visibility.Visible;
            this.UpdateLayout();

            double width = ActionPrompt.ActualWidth;
            double height = ActionPrompt.ActualHeight;

            int screenNumber = Configuration.Settings.ScreenNumber;
            int screenNumberForPrompt = Math.Max(0, Math.Min(Screen.AllScreens.Length, screenNumber));
            var screen = Screen.AllScreens[screenNumberForPrompt];

            var screenCenterHorizontal = screen.Bounds.X + (screen.Bounds.Width / 2);
            var screenCenterVertical = screen.Bounds.Y + (screen.Bounds.Height / 2);

            Canvas.SetLeft(ActionPrompt, screenCenterHorizontal - (width / 2)); 
            Canvas.SetTop(ActionPrompt, screenCenterVertical - (height / 2)); 
        }

        private void DisplayWindow()
        {
            this.Visibility = Visibility.Visible;
            this.Topmost = true;

            // some offset to hide window boarders
            int adjustValue = 7;
            this.Top = _screenSpan.Y - adjustValue;
            this.Left = _screenSpan.X - adjustValue;
            this.Width = _screenSpan.Width + 2*adjustValue;
            this.Height = _screenSpan.Height + 2*adjustValue;
        }

        private void DrawingBoard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _firstPoint = e.GetPosition(DrawingBoard);
            ActionPrompt.Visibility = Visibility.Hidden;
        }

        private void DrawingBoard_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var mousePos = e.GetPosition(DrawingBoard);
            AdjustSelectionLines(mousePos);

            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            _secondPoint = mousePos;
            AdjustRectangle();
        }

        private void DrawingBoard_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var (pnt_min, pnt_max) = GetMinMaxPoints();

            int screenshotWidth = (int)(pnt_max.X - pnt_min.X);
            int screenshotHeight = (int)(pnt_max.Y - pnt_min.Y);

            var minSpan = Math.Max(1, Configuration.Settings.MinimumScreenshotSpanXY);
            if (screenshotWidth < minSpan || screenshotHeight < minSpan)
                return;

            double scaling = getScaling();
            var areaRectangle = new Rectangle(
                        (int)(pnt_min.X * scaling),
                        (int)(pnt_min.Y * scaling),
                        (int)(screenshotWidth * scaling),
                        (int)(screenshotHeight * scaling)
                        );
            var screenShot = _bmpScreenshot.Clone(areaRectangle, System.Drawing.Imaging.PixelFormat.Format64bppPArgb);
            DisplayData.Instance.AddScreenshot(screenShot);

            if (_openDisplayOnFinished)
                Instances.Display.Show();
            this.FinishScreenshot();
        }

        private static TakeWindow _instance;
        private static readonly object _lock = new object();
        private bool _running;

        private bool _openDisplayOnFinished;

        public static void TakeScreenshot(bool? openDisplayOnFinished = null)
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new TakeWindow();
                }
            }

            _instance._openDisplayOnFinished = openDisplayOnFinished ?? Configuration.Settings.IsScreenshotViewShownOnNewScreenshot;
            _instance.StartScreenshot();
        }

        public static void Dispose()
        {
            _instance?.Close();
            _instance = null;
        }

        private void StartScreenshot()
        {
            if (_running)
                return;
            _running = true;

            ActionPrompt.Visibility = Visibility.Visible;
            this.CollectScreenData();
            this.Show();
            PreparePrompt();
            this.Focus();

            DrawingBoard.MouseDown += DrawingBoard_MouseDown;
            DrawingBoard.MouseMove += DrawingBoard_MouseMove;
            DrawingBoard.MouseUp += DrawingBoard_MouseUp;
            this.KeyDown += ScreenShotDisplay_KeyDown;
        }

        private void FinishScreenshot()
        {
            this.KeyDown -= ScreenShotDisplay_KeyDown;
            DrawingBoard.MouseDown -= DrawingBoard_MouseDown;
            DrawingBoard.MouseMove -= DrawingBoard_MouseMove;
            DrawingBoard.MouseUp -= DrawingBoard_MouseUp;

            // Clean up Topmost from 'DisplayWindow' function
            this.Topmost = false;

            this.Hide();
            this._firstPoint = default;
            this._secondPoint = default;
            this._screenSpan = default;
            this._bmpScreenshot = default;

            _running = false;
        }
    }
}
