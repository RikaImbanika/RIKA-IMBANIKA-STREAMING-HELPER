// MainWindow.xaml.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;

namespace RIKA_TIMER
{ 
    public partial class MainWindow : Window
    {
        private double _initialFontSize;
        private System.Windows.Point _initialMouseScreenPoint;
        private double _initialWidth;
        private double _initialHeight;
        private double _initialLeft;
        private double _initialTop;
        private Point _initialClickPoint;
        //private Point _resizeStartScreenPoint;
        //private double _ratioX;
        //private double _ratioY;

        private DateTime _lastStartTime;
        private TimeSpan _actualElapsed;
        private TimeSpan _lastStartElapsed;

        private DispatcherTimer _timer;
        private DateTime _lastSaveTime;
        private int _clickCount;
        private DateTime _lastClickTime;
        private const string _saveFile = "timer_state.txt";
        private const string _MFile = "M.txt";
        private bool _isResizing;
        //private Point _resizeStartPoint;
        //private double _resizeStartWidth;
        //private double _resizeStartHeight;
        private DispatcherTimer _resizeTimer;
        private bool _resizePending = false;
        private bool _alreadyResizing = false;

        private readonly int _colorDurationMinutes = 1;
        private readonly Random _random;
        private int _currentColor;
        private int _previousColor;
        private int _nextColor;
        private int _lastProcessedInterval = -1;

        private AudioPlayer _ap;
        private int _lastDayCheck;
        private int _M;
        private double _scale;
        private double _initialScale;
        private DispatcherTimer _clickTimer;

        private string[] _avas;

        private int[] _reids;
        private int[] _quotesIds;

        private int _avaId;

        private bool[] _mirrowMask;

        bool _frontOne;

        DispatcherTimer _quoteTimer;

        string[] _quotes;

        int _currentQuoteIndex;

        private readonly List<Color> _colors = new List<Color>
        {
            Colors.OrangeRed,
            Colors.BlueViolet,
            Colors.LightSeaGreen,
            Colors.Lime,
            Colors.Fuchsia,
            Colors.MediumBlue,
            Colors.Aqua
        };

        public MainWindow()
        {
            _scale = 1;

            InitializeComponent();

            GlobalScale.ScaleX = _scale;
            GlobalScale.ScaleY = _scale;

            LoadState();
            InitializeTimers();
            _random = new Random();
            _currentColor = _random.Next(_colors.Count);
            _nextColor = GetNextColor();
            Rika.Text = $"Rika Imbanika";

            _ap = new AudioPlayer();
            _ap._MW = this;
            _ap.PlayNext();

            _clickTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.4) };
            _clickTimer.Tick += OnClickTimerElapsed;
            RippleManager.Initialize();

            this.Closing += MainWindow_Closing;

            string nick = File.ReadAllText($"{Environment.CurrentDirectory}\\Text\\Nick.txt");
            Rika.Text = nick;

            FillAvas();

            var timer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(0.65) };
            timer.Tick += (s, e) => SwitchAvas();
            timer.Start();

            _quotes = File.ReadAllLines($"{Environment.CurrentDirectory}\\Text\\Quotes.txt");
            _quotesIds = new int[_quotes.Length];
            ShuffleQuotes();
            Quote.Text = _quotes[_quotesIds[_currentQuoteIndex]];

            _quoteTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            _quoteTimer.Tick += (s, e) => ChangeQuote();
            _quoteTimer.Start();

            MouseWheelOverlay mwo = new MouseWheelOverlay();
            mwo.Show();
        }

        private void ChangeQuote()
        {
            _quoteTimer.Stop();

            var fadeOut = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = TimeSpan.FromSeconds(1)
            };

            fadeOut.Completed += (s, _) =>
            {
                _currentQuoteIndex++;
                if (_currentQuoteIndex >= _quotes.Length)
                {
                    _currentQuoteIndex = 0;
                    ShuffleQuotes();
                }

                Quote.Text = _quotes[_quotesIds[_currentQuoteIndex]];

                var fadeIn = new DoubleAnimation
                {
                    From = 0.0,
                    To = 1.0,
                    Duration = TimeSpan.FromSeconds(1),
                    EasingFunction = new SineEase()
                };

                fadeIn.Completed += (s_, _) => _quoteTimer.Start();
                Quote.BeginAnimation(TextBlock.OpacityProperty, fadeIn);
            };

            Quote.BeginAnimation(TextBlock.OpacityProperty, fadeOut);
        }

        private void ShuffleQuotes()
        {
            _quotes = File.ReadAllLines($"{Environment.CurrentDirectory}\\Text\\Quotes.txt");
            for (int i = 0; i < _quotes.Length; i++)
                if (_quotes[i].Contains('|'))
                    _quotes[i] = _quotes[i].Replace('|', '\n');

            // Store the last quote ID from the previous shuffle.  This is crucial for the next step.
            int lastQuoteId = _quotesIds.Length > 0 ? _quotesIds.Last() : -1;
            _quotesIds = Enumerable.Range(0, _quotes.Length).ToArray(); //More efficient way to initialize

            Random rnd = new Random();

            //Fisher-Yates shuffle algorithm (optimized)
            for (int i = _quotesIds.Length - 1; i > 0; i--)
            {
                int j = rnd.Next(i + 1);
                (_quotesIds[i], _quotesIds[j]) = (_quotesIds[j], _quotesIds[i]); //Tuple deconstruction for cleaner swap
            }

            //Handle the last quote separately to prevent repetition.
            if (_quotesIds.Length > 1 && _quotesIds[0] == lastQuoteId)
            {
                //Find a different index for the last quote, ensuring it's not the first element.
                int newIndex = rnd.Next(1, _quotesIds.Length); //Exclude index 0
                (_quotesIds[0], _quotesIds[newIndex]) = (_quotesIds[newIndex], _quotesIds[0]);
            }
        }

        public void ChangeAudioName(string text)
        {
            var fadeOut = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = TimeSpan.FromSeconds(1)
            };

            fadeOut.Completed += (s, _) =>
            {
                Track.Text = text;

                var fadeIn = new DoubleAnimation
                {
                    From = 0.0,
                    To = 1.0,
                    Duration = TimeSpan.FromSeconds(1),
                    EasingFunction = new SineEase()
                };

                fadeIn.Completed += (s_, _) => _quoteTimer.Start();
                Track.BeginAnimation(TextBlock.OpacityProperty, fadeIn);
            };

            Track.BeginAnimation(TextBlock.OpacityProperty, fadeOut);
        }

        private void FillAvas()
        {
            _avas = Directory.GetFiles($"{Environment.CurrentDirectory}\\Avas");
            _mirrowMask = new bool[_avas.Length];

            Random rnd = new Random();

            for (int i = 0; i < _mirrowMask.Length; i++)
                _mirrowMask[i] = rnd.Next(2) == 0;

            _reids = new int[_avas.Length];

            for (int i = 0; i < _reids.Length; i++)
                _reids[i] = i;

            for (int i = _reids.Length - 1; i > 0; i--)
            {
                int j = rnd.Next(i + 1); 

                int temp = _reids[i];
                _reids[i] = _reids[j];
                _reids[j] = temp;
            }

            _avaId = rnd.Next(_avas.Length);
            _frontOne = true;
            LoadAva();
            _frontOne = false;
        }

        private void SwitchAvas()
        {
            LoadAva();

            if (_frontOne)
                FadeOut();
            else
                FadeIn();

            _frontOne = !_frontOne;
        }

        private void LoadAva()
        {
            _avaId++;
            bool part2 = false;
            int id = _avaId;
            if (_avaId >= _avas.Length * 2)
            {
                _avaId = 0;
                id = 0;
            }
            else if (_avaId >= _avas.Length)
            {
                part2 = true;
                id = _avaId - _avas.Length;
            }

            bool mirrow = _mirrowMask[id];
            if (part2)
                mirrow = !mirrow;

            id = _reids[id];

            string path = _avas[id];

            var image = _frontOne ? Ava1 : Ava2;

            var bitmap = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));

            var transform = new ScaleTransform
            {
                ScaleX = 1,
                CenterX = bitmap.PixelWidth / 2.0
            };

            if (mirrow)
                transform.ScaleX = -1;

            image.Source = bitmap;
            image.RenderTransform = transform;
        }

        private void FadeIn()
        {
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(4))
            {
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseIn }
            };

            Ava2.BeginAnimation(OpacityProperty, fadeIn);
        }

        private void FadeOut()
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(4))
            {
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseOut }
            };

            Ava2.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void OnClickTimerElapsed(object sender, EventArgs e)
        {
            _clickTimer.Stop();

            if (_clickCount >= 10)
                ResetTimer();
            else if (_clickCount >= 2)
                _ap.PlayNext();

            _clickCount = 0;
        }

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(TransparentRectangle);

            if (position.X >= 0 && position.X <= TransparentRectangle.ActualWidth &&
                position.Y >= 0 && position.Y <= TransparentRectangle.ActualHeight)
            {
                if (e.ChangedButton == MouseButton.Left)
                    Window_MouseLeftButtonDown(this, e);
                else if (e.ChangedButton == MouseButton.Right)
                    Window_MouseRightButtonDown(this, e);
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && !_isResizing)
            {
                _isResizing = true;
                _initialClickPoint = e.GetPosition(this);
                _initialMouseScreenPoint = PointToScreen(_initialClickPoint);
                _initialWidth = ActualWidth;
                _initialHeight = ActualHeight;
                _initialLeft = Left;
                _initialTop = Top;
                _initialFontSize = TimerText.FontSize;
                _initialScale = _scale;

                _resizeTimer.Start(); // Запускаем таймер обработки
                CaptureMouse();
            }
            else
            {
                DragMove();
                HandleMultiClick();
            }
        }

        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_timer.IsEnabled)
            {
                _timer.Stop();
                _lastStartTime = DateTime.Now;
                _lastStartElapsed = _actualElapsed;
                _actualElapsed = _lastStartElapsed;
            }
            else
            {
                _lastStartTime = DateTime.Now;
                _lastStartElapsed = _actualElapsed;
                _actualElapsed = _lastStartElapsed;
                _timer.Start();
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
        }

        private void ResizeTimer_Tick(object sender, EventArgs e)
        {
            if (!_isResizing || _alreadyResizing) return;
            _alreadyResizing = true;

            Point currentScreenPoint = PointToScreen(Mouse.GetPosition(this));
            double deltaY = currentScreenPoint.Y - _initialMouseScreenPoint.Y;
            double deltaScale = Math.Exp(-deltaY / 400.0);

            _scale = Math.Clamp(_initialScale * deltaScale, 0.25, 5);

            var left = _initialLeft + (_initialClickPoint.X * (1 - deltaScale));
            var top = _initialTop + (_initialClickPoint.Y * (1 - deltaScale));

            this.Dispatcher.InvokeAsync(() =>
            {
                GlobalScale.ScaleX = _scale;
                Left = left;
                GlobalScale.ScaleY = _scale;
                Top = top;

                _alreadyResizing = false;
            }, DispatcherPriority.Background);
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isResizing)
            {
                _isResizing = false;
                _resizeTimer.Stop();
                ReleaseMouseCapture();
            }
        }

        private void InitializeTimers()
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.95) };
            _timer.Tick += (s, e) => UpdateDisplay();
            _timer.Start();

            var systemTimeTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            systemTimeTimer.Tick += (s, e) => UpdateSystemTime();
            systemTimeTimer.Start();
        }

        private void HandleMultiClick()
        {
            var now = DateTime.Now;

            if ((now - _lastClickTime).TotalSeconds < 0.4)
                _clickCount++;
            else
                _clickCount = 1;

            _lastClickTime = now;

            _clickTimer.Stop();
            _clickTimer.Start();
        }

        private void ResetTimer()
        {
            _lastStartTime = DateTime.Now;
            _actualElapsed = TimeSpan.Zero;
            _lastStartElapsed = TimeSpan.Zero;

            _clickCount = 0;
        }

        private void LoadState()
        {
            if (File.Exists(_saveFile))
                _lastStartElapsed = TimeSpan.FromSeconds(double.Parse(File.ReadAllText(_saveFile)));

            _lastStartTime = DateTime.Now;

            _actualElapsed = _lastStartElapsed;

            _lastDayCheck = _lastStartElapsed.Days;

            if (File.Exists(_MFile))
                _M = int.Parse(File.ReadAllText(_MFile));
            else
                SaveM(0);

            TheM.Text = $"M{_M}";
        }

        private void SaveState()
        {
            if ((DateTime.Now - _lastSaveTime).TotalSeconds >= 10)
            {
                _lastSaveTime = DateTime.Now;

                var elapsed = _actualElapsed.TotalSeconds;
                File.WriteAllText(_saveFile, elapsed.ToString());
            }
        }

        private void SaveM(int M)
        {
            File.WriteAllText(_MFile, _M.ToString());
        }

        private int GetNextColor()
        {
            int id = 0;

            do id = _random.Next(_colors.Count);
            while (id == _currentColor || id == _previousColor);

            return id;
        }

        private void UpdateDisplay()
        {
            if (ActualHeight != ActualWidth)
                Height = ActualWidth;

            var deltaElapsed = DateTime.Now - _lastStartTime;
            _actualElapsed = _lastStartElapsed + deltaElapsed;
            TimerText.Text = $"{_actualElapsed.Hours:00}:{_actualElapsed.Minutes:00}:{_actualElapsed.Seconds:00}";

            double totalMinutes = _actualElapsed.TotalMinutes;
            int currentInterval = (int)(totalMinutes / _colorDurationMinutes);
            double progress = (totalMinutes % _colorDurationMinutes) / _colorDurationMinutes;

            if (_actualElapsed.Days > _lastDayCheck)
            {
                _M++;
                SaveM(_M);
                TheM.Text = $"M{_M}";
                _lastDayCheck = _actualElapsed.Days;
            }

            if (currentInterval != _lastProcessedInterval)
            {
                _previousColor = _currentColor;
                _currentColor = _nextColor;
                _nextColor = GetNextColor();
                _lastProcessedInterval = currentInterval;
            }

            var hsl1 = RgbToHsl(_colors[_currentColor]);
            var hsl2 = RgbToHsl(_colors[_nextColor]);

            double h1 = hsl1.Item1;
            double s1 = hsl1.Item2;
            double l1 = hsl1.Item3;

            double h2 = hsl2.Item1;
            double s2 = hsl2.Item2;
            double l2 = hsl2.Item3;

            if (l2 <= 0)
                h2 = h1;
            if (l1 <= 0)
                h1 = h2;

            double h = SineLerp(h1, h2, progress);
            double s = SineLerp(s1, s2, progress);
            double l = SineLerp(l1, l2, progress);

            s = s + (1 - s) / 2;

            var interpolatedColor = HSLToRGB(h, s, l);

/*            double gray1 = (_colors[_currentColor].R + _colors[_currentColor].G + _colors[_currentColor].B) / 3;
            double dred1 = _colors[_currentColor].R - gray1;
            double dgreen1 = _colors[_currentColor].G - gray1;
            double dblue1 = _colors[_currentColor].B - gray1;

            double gray2 = (_colors[_nextColor].R + _colors[_nextColor].G + _colors[_nextColor].B) / 3;
            double dred2 = _colors[_nextColor].R - gray2;
            double dgreen2 = _colors[_nextColor].G - gray2;
            double dblue2 = _colors[_nextColor].B - gray2;

            var interpolatedColor = Color.FromArgb(
                255,
                (byte)SineLerp(_colors[_currentColor].R, _colors[_nextColor].R, progress),
                (byte)SineLerp(_colors[_currentColor].G, _colors[_nextColor].G, progress),
                (byte)SineLerp(_colors[_currentColor].B, _colors[_nextColor].B, progress)
            );

            double gray = (interpolatedColor.R + interpolatedColor.G + interpolatedColor.B) / 3;
            double dRed = interpolatedColor.R - gray;
            double dGreen = interpolatedColor.G - gray;
            double dBlue = interpolatedColor.B - gray;

            double targetRed = 

            double d = targetGray / gray;

            interpolatedColor = Color.FromHsv(

            var ensaturatedColor = SaturateRGB(interpolatedColor, 3);*/

            var gradient = new LinearGradientBrush(
                Colors.White,
                interpolatedColor,
                new Point(0.5, 0),
                new Point(0.5, 1)
            );

            TimerText.Foreground = gradient;
            SystemTimeText.Foreground = gradient;
            TheM.Foreground = gradient;
            Rika.Foreground = gradient;
            Track.Foreground = gradient;
            Quote.Foreground = gradient;

            SaveState();
            if ((DateTime.Now - _lastClickTime).TotalSeconds > 2) _clickCount = 0;
        }

        double SineLerp(double a, double b, double t) =>
    a + (b - a) * (1 - Math.Cos(t * Math.PI)) * 0.5;

        private void UpdateSystemTime()
        {
            SystemTimeText.Text = DateTime.Now.ToString("HH:mm");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _initialFontSize = TimerText.FontSize;
            _resizeTimer = new DispatcherTimer();
            _resizeTimer.Interval = TimeSpan.FromMilliseconds(33);
            _resizeTimer.Tick += ResizeTimer_Tick;
        }

        private static double Lerp(double a, double b, double t)
        {
            return a + (b - a) * t;
        }

        private static double ExponentialLerp(double a, double b, double t)
        {
            double difference = b - a;
            double linearPart = a + difference * t;
            double boost = Math.Abs(difference) * t * (1 - t);
            return linearPart + boost;
        }

        public static Color SaturateRGB(Color color, float strength)
        {
            // Находим среднее значение каналов (серую составляющую)
            float avg = (color.R + color.G + color.B) / 3f;

            // Усиливаем отклонение от серого
            byte r = (byte)Math.Clamp(color.R + (int)((color.R - avg) * strength), 0, 255);
            byte g = (byte)Math.Clamp(color.G + (int)((color.G - avg) * strength), 0, 255);
            byte b = (byte)Math.Clamp(color.B + (int)((color.B - avg) * strength), 0, 255);

            return Color.FromArgb(255, r, g, b);
        }

        public static (double Hue, double Saturation, double Lightness) RgbToHsl(System.Windows.Media.Color color)
        {
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;

            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));

            double h, s, l;
            l = (max + min) / 2;

            if (max == min)
            {
                h = s = 0; // achromatic
            }
            else
            {
                double d = max - min;
                s = l > 0.5 ? d / (2 - max - min) : d / (max + min);

                if (max == r)
                    h = (g - b) / d + (g < b ? 6 : 0);
                else if (max == g)
                    h = (b - r) / d + 2;
                else if (max == b)
                    h = (r - g) / d + 4;
                else
                    h = 0;

                h /= 6;
            }

            return (h, s, l);
        }

        public static System.Windows.Media.Color HSLToRGB(double h, double s, double l)
        {
            double r, g, b;

            if (s == 0)
            {
                r = g = b = l; // achromatic
            }
            else
            {
                double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
                double p = 2 * l - q;
                r = Hue2RGB(p, q, h + 1.0 / 3);
                g = Hue2RGB(p, q, h);
                b = Hue2RGB(p, q, h - 1.0 / 3);
            }

            return System.Windows.Media.Color.FromRgb(
                (byte)(r * 255),
                (byte)(g * 255),
                (byte)(b * 255));
        }

        private static double Hue2RGB(double p, double q, double t)
        {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1.0 / 6) return p + (q - p) * 6 * t;
            if (t < 1.0 / 2) return q;
            if (t < 2.0 / 3) return p + (q - p) * (2.0 / 3 - t) * 6;
            return p;
        }
    }
}