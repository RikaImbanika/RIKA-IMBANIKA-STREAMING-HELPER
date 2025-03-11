using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace RIKA_TIMER
{
    public partial class DesktopTextWindow : Window
    {
        public DesktopTextWindow(string text)
        {
            ConfigureWindow();
            CreateVisuals(text);
            SetupAnimation();
        }

        private void ConfigureWindow()
        {
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            Topmost = true;
            ShowInTaskbar = false;
            ResizeMode = ResizeMode.NoResize;

            Width = SystemParameters.WorkArea.Width;
            Height = SystemParameters.WorkArea.Height;
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = SystemParameters.WorkArea.Left;
            Top = SystemParameters.WorkArea.Top;
        }

        private void CreateVisuals(string text)
        {
            var mainContainer = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(20, 0, 0, 40),
                ClipToBounds = false
            };

            // Создаем цепочку вложенных Grid
            var shadowHost = new Grid();
            mainContainer.Children.Add(shadowHost);

            Grid currentLayer = shadowHost;
            for (int i = 0; i < 4; i++)
            {
                var newLayer = new Grid
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Effect = new DropShadowEffect
                    {
                        Color = Colors.Black,
                        ShadowDepth = 0,
                        BlurRadius = 20,
                        Opacity = 1,
                        RenderingBias = RenderingBias.Quality
                    }
                };

                // Добавляем невидимый элемент для задания размеров
                newLayer.Children.Add(new TextBlock
                {
                    Text = text,
                    Foreground = Brushes.Transparent,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Bottom
                });

                currentLayer.Children.Add(newLayer);
                currentLayer = newLayer;
            }

            var textBlock = new TextBlock
            {
                Text = text,
                Foreground = Brushes.White,
                TextWrapping = TextWrapping.NoWrap,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom
            };
            currentLayer.Children.Add(textBlock);

            SizeChanged += (s, e) =>
            {
                textBlock.MaxWidth = ActualWidth / 3;
                textBlock.FontSize = ActualWidth / text.Length * 0.6;
            };

            Content = mainContainer;
        }

        private void SetupAnimation()
        {
            var storyboard = new Storyboard();

            // Оптимизация рендеринга
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.LowQuality);
            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);

            // Настройки времени (можно регулировать)
            double appearTime = 1;
            double pauseTime = 3;
            double disappearTime = 5;

            // Анимация появления
            var appearAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(appearTime),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn } // Более легкая функция
            };
            Storyboard.SetTargetProperty(appearAnimation, new PropertyPath(OpacityProperty));

            // Анимация исчезновения
            var disappearAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(disappearTime),
                BeginTime = TimeSpan.FromSeconds(appearTime + pauseTime),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTargetProperty(disappearAnimation, new PropertyPath(OpacityProperty));

            storyboard.Children.Add(appearAnimation);
            storyboard.Children.Add(disappearAnimation);

            // Оптимизация завершения
            storyboard.Completed += (s, e) =>
            {
                storyboard.Remove(this);
                Close();
            };

            storyboard.Begin(this, HandoffBehavior.SnapshotAndReplace);
        }
    }
}