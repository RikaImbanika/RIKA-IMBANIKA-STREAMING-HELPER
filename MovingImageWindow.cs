using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace RIKA_IMBANIKA_LIFE_HELPER
{
    public class MovingImageWindow : Window
    {
        private enum Side { Top, Bottom, Left, Right }
        private enum Direction { Forward, Backward }

        private readonly Image _image;
        private readonly string _imagePath;
        private readonly double _screenWidth, _screenHeight;
        public int _lastIndex;

        public double SizeFactor { get; set; } = 0.2;
        public double Amplitude { get; set; } = 0.5;
        public double Frequency { get; set; } = 1.0;
        public double Duration { get; set; } = 5.0;

        private Side _currentSide;
        private Direction _currentDirection;
        private Point _startPos, _endPos;
        private DateTime _startTime;
        private BitmapSource[] _pulseImages;
        private DispatcherTimer _animationTimer;

        private BitmapSource _sourceBitmap;
        private BitmapSource _baseBitmap;
        private double _baseWidth;
        private double _baseHeight;
        private double _maxHeight;

        // Новые поля для WriteableBitmap
        private WriteableBitmap _writeableBitmap;
        private byte[][] _frameBuffers;
        private int _stride;

        public MovingImageWindow(string imagePath)
        {
            _imagePath = imagePath;
            _screenWidth = SystemParameters.PrimaryScreenWidth;
            _screenHeight = SystemParameters.PrimaryScreenHeight;

            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            ShowInTaskbar = false;
            Topmost = true;
            IsHitTestVisible = false;
            ShowActivated = false;
            Focusable = false;

            var canvas = new Canvas();
            _image = new Image();
            LoadSourceImage();
            canvas.Children.Add(_image);
            Content = canvas;

            _image.RenderTransformOrigin = new Point(0.5, 0.5);

            InitializeBuffer();

            Hide();
        }

        private void LoadSourceImage()
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(_imagePath, UriKind.RelativeOrAbsolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bitmap.EndInit();
            _sourceBitmap = bitmap;
        }

        private void InitializeBuffer()
        {
            _baseWidth = _screenWidth * SizeFactor;
            _baseHeight = _baseWidth * _sourceBitmap.PixelHeight / _sourceBitmap.PixelWidth;

            double scaleX = _baseWidth / _sourceBitmap.PixelWidth;
            double scaleY = _baseHeight / _sourceBitmap.PixelHeight;
            var transform = new ScaleTransform(scaleX, scaleY);
            _baseBitmap = new TransformedBitmap(_sourceBitmap, transform);
            _baseBitmap.Freeze();

            double minScale = 1.0 - Amplitude;
            double maxScale = 1.0 + Amplitude;
            _maxHeight = _baseHeight * maxScale;

            int count = 20;
            _pulseImages = new BitmapSource[count];
            double scaleStep = (maxScale - minScale) / (count - 1);

            for (int i = 0; i < count; i++)
            {
                double targetScaleY = minScale + scaleStep * i;
                double currentHeight = _baseHeight * targetScaleY;

                var scaleYTransform = new ScaleTransform(1, targetScaleY);
                var scaledBitmap = new TransformedBitmap(_baseBitmap, scaleYTransform);
                scaledBitmap.Freeze();

                var squareBitmap = new RenderTargetBitmap(
                    (int)_maxHeight, (int)_maxHeight, 96, 96, PixelFormats.Pbgra32);
                var visual = new DrawingVisual();

                using (var context = visual.RenderOpen())
                {
                    double x = (_maxHeight - _baseWidth) / 2;
                    double y = _maxHeight - currentHeight;
                    context.DrawImage(scaledBitmap, new Rect(x, y, _baseWidth, currentHeight));
                }

                squareBitmap.Render(visual);
                squareBitmap.Freeze();
                _pulseImages[i] = squareBitmap;
            }

            // Создаём WriteableBitmap и извлекаем байты всех кадров
            int width = (int)_maxHeight;
            int height = (int)_maxHeight;
            _writeableBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Pbgra32, null);
            _stride = width * 4; // Pbgra32 = 4 байта на пиксель

            _frameBuffers = new byte[count][];
            for (int i = 0; i < count; i++)
            {
                var frame = _pulseImages[i];
                byte[] pixels = new byte[height * _stride];
                frame.CopyPixels(pixels, _stride, 0);
                _frameBuffers[i] = pixels;
            }

            // Устанавливаем первый кадр как источник (будет обновляться через WritePixels)
            _writeableBitmap.WritePixels(new Int32Rect(0, 0, width, height), _frameBuffers[0], _stride, 0);
            _image.Source = _writeableBitmap;
        }

        public void StartAnimation(string side, string dir)
        {
            if (!Enum.TryParse(side, out Side parsedSide) ||
                !Enum.TryParse(dir, out Direction parsedDir))
                return;

            _currentSide = parsedSide;
            _currentDirection = parsedDir;

            var transformGroup = new TransformGroup();

            double rotationAngle = 0;
            switch (_currentSide)
            {
                case Side.Bottom: rotationAngle = 0; break;
                case Side.Top: rotationAngle = 180; break;
                case Side.Left: rotationAngle = 90; break;
                case Side.Right: rotationAngle = -90; break;
            }
            transformGroup.Children.Add(new RotateTransform(rotationAngle));

            double scaleX = 1, scaleY = 1;
            bool needReflection = (_currentDirection == Direction.Backward && (_currentSide == Side.Bottom || _currentSide == Side.Left))
                               || (_currentDirection == Direction.Forward && (_currentSide == Side.Top || _currentSide == Side.Right));
            if (needReflection)
            {
                if (_currentSide == Side.Bottom || _currentSide == Side.Top)
                    scaleX = -1;
                else
                    scaleY = -1;
            }
            transformGroup.Children.Add(new ScaleTransform(scaleX, scaleY));

            _image.RenderTransform = transformGroup;

            Width = _maxHeight;
            Height = _maxHeight;
            Canvas.SetLeft(_image, 0);
            Canvas.SetTop(_image, 0);

            CalculatePositions();
            Left = _startPos.X;
            Top = _startPos.Y;

            _startTime = DateTime.Now;

            CompositionTarget.Rendering += OnRendering;
            Show();
        }

        private void OnRendering(object sender, EventArgs e)
        {
            double elapsedSeconds = (DateTime.Now - _startTime).TotalSeconds;

            if (elapsedSeconds >= Duration)
            {
                CompositionTarget.Rendering -= OnRendering;
                Hide();
                return;
            }

            UpdateAnimation(elapsedSeconds);
        }

        private void CalculatePositions()
        {
            double windowSize = _maxHeight;

            switch (_currentSide)
            {
                case Side.Bottom:
                    _startPos.Y = _endPos.Y = _screenHeight - windowSize;
                    if (_currentDirection == Direction.Forward)
                    { _startPos.X = -windowSize; _endPos.X = _screenWidth; }
                    else
                    { _startPos.X = _screenWidth; _endPos.X = -windowSize; }
                    Duration = 5;
                    break;
                case Side.Top:
                    _startPos.Y = _endPos.Y = 0;
                    if (_currentDirection == Direction.Forward)
                    { _startPos.X = -windowSize; _endPos.X = _screenWidth; }
                    else
                    { _startPos.X = _screenWidth; _endPos.X = -windowSize; }
                    Duration = 5;
                    break;
                case Side.Left:
                    _startPos.X = _endPos.X = 0;
                    if (_currentDirection == Direction.Forward)
                    { _startPos.Y = -windowSize; _endPos.Y = _screenHeight; }
                    else
                    { _startPos.Y = _screenHeight; _endPos.Y = -windowSize; }
                    Duration = 3.5;
                    break;
                case Side.Right:
                    _startPos.X = _endPos.X = _screenWidth - windowSize;
                    if (_currentDirection == Direction.Forward)
                    { _startPos.Y = -windowSize; _endPos.Y = _screenHeight; }
                    else
                    { _startPos.Y = _screenHeight; _endPos.Y = -windowSize; }
                    Duration = 3.5;
                    break;
            }
        }

        private void UpdateAnimation(double elapsedSeconds)
        {
            double t = elapsedSeconds / Duration;
            Left = _startPos.X + (_endPos.X - _startPos.X) * t;
            Top = _startPos.Y + (_endPos.Y - _startPos.Y) * t;

            double sin = Math.Sin(2 * Math.PI * Frequency * elapsedSeconds);
            double targetScale = 1 + Amplitude * sin;
            double minScale = 1 - Amplitude;
            double maxScale = 1 + Amplitude;
            int index = (int)((targetScale - minScale) / (maxScale - minScale) * (_pulseImages.Length - 1) + 0.5);

            if (index != _lastIndex)
            {
                int width = (int)_maxHeight;
                int height = (int)_maxHeight;
                _writeableBitmap.WritePixels(new Int32Rect(0, 0, width, height), _frameBuffers[index], _stride, 0);
                _lastIndex = index;
            }
        }
    }
}