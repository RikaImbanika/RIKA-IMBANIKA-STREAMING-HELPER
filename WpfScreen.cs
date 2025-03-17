using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows;
using System.Runtime.InteropServices;
using System.Windows.Media;

namespace RIKA_TIMER
{
    public class WpfScreen
    {
        private readonly Rect _bounds;
        private readonly DpiScale _dpi;

/*        private WpfScreen(Rect bounds, DpiScale dpi)
        {
            _bounds = bounds;
            _dpi = dpi;
        }*/

        public Rect PhysicalBounds { get; } // Физические пиксели
        public DpiScale Dpi { get; }

        public WpfScreen(Rect bounds, DpiScale dpi)
        {
            PhysicalBounds = bounds;
            Dpi = dpi;
        }

        public static WpfScreen GetScreenFrom(Window window)
        {
            var hwnd = new WindowInteropHelper(window).EnsureHandle();
            var monitor = NativeMethods.MonitorFromWindow(hwnd, NativeMethods.MONITOR_DEFAULTTONEAREST);

            NativeMethods.MONITORINFOEX info = new();
            info.cbSize = Marshal.SizeOf(info);
            NativeMethods.GetMonitorInfo(monitor, ref info);

            return new WpfScreen(
                new Rect(
                    info.rcMonitor.Left,
                    info.rcMonitor.Top,
                    info.rcMonitor.Right - info.rcMonitor.Left,
                    info.rcMonitor.Bottom - info.rcMonitor.Top),
                VisualTreeHelper.GetDpi(window));
        }

        public Rect WpfBounds => new Rect(
            _bounds.X / _dpi.DpiScaleX,
            _bounds.Y / _dpi.DpiScaleY,
            _bounds.Width / _dpi.DpiScaleX,
            _bounds.Height / _dpi.DpiScaleY);

/*        public static WpfScreen GetScreenFrom(Window window)
        {
            try
            {
                var hwnd = new WindowInteropHelper(window).Handle;
                var monitor = NativeMethods.MonitorFromWindow(hwnd, NativeMethods.MONITOR_DEFAULTTONEAREST);

                NativeMethods.MONITORINFOEX info = new NativeMethods.MONITORINFOEX();
                info.cbSize = Marshal.SizeOf(info);

                if (!NativeMethods.GetMonitorInfo(monitor, ref info))
                {
                    MessageBox.Show("Ошибка GetMonitorInfo! Код: " + Marshal.GetLastWin32Error());
                    return null;
                }

                MessageBox.Show($"Монитор: {info.rcMonitor.Left}x{info.rcMonitor.Top} → " +
                              $"{info.rcMonitor.Right}x{info.rcMonitor.Bottom}");

                NativeMethods.GetMonitorInfo(monitor, ref info);

                var dpi = VisualTreeHelper.GetDpi(window);

                return new WpfScreen(
                    new Rect(
                        info.rcMonitor.Left,
                        info.rcMonitor.Top,
                        info.rcMonitor.Right - info.rcMonitor.Left,
                        info.rcMonitor.Bottom - info.rcMonitor.Top),
                    dpi);
            }
            catch (Exception ex)
            {
                MessageBox.Show("CRITICAL: " + ex);
                return null;
            }
        }*/

        /*        public static WpfScreen GetScreenFrom(Window window)
                {
                    var hwnd = new WindowInteropHelper(window).Handle;
                    var monitor = NativeMethods.MonitorFromWindow(hwnd, NativeMethods.MONITOR_DEFAULTTONEAREST);

                    NativeMethods.MONITORINFOEX info = new NativeMethods.MONITORINFOEX();
                    info.cbSize = Marshal.SizeOf(info); // ← ЭТОГО НЕ ХВАТАЛО
                    NativeMethods.GetMonitorInfo(monitor, ref info);

                    var dpi = VisualTreeHelper.GetDpi(window);

                    return new WpfScreen(
                        new Rect(
                            info.rcMonitor.Left,
                            info.rcMonitor.Top,
                            info.rcMonitor.Right - info.rcMonitor.Left,
                            info.rcMonitor.Bottom - info.rcMonitor.Top),
                        dpi);
                }*/

        private static class NativeMethods
        {
            public const int MONITOR_DEFAULTTONEAREST = 2;

            [DllImport("user32.dll")]
            public static extern IntPtr MonitorFromWindow(IntPtr hwnd, int flags);

            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            public struct MONITORINFOEX
            {
                public int cbSize;
                public RECT rcMonitor;
                public RECT rcWork;
                public uint dwFlags;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
                public string szDevice;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct RECT
            {
                public int Left;
                public int Top;
                public int Right;
                public int Bottom;
            }
        }
    }
}
