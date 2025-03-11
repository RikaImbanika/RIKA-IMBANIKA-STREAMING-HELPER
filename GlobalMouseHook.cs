using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;

namespace RIKA_TIMER
{
    public class GlobalMouseHook : IDisposable
    {
        private const int WH_MOUSE_LL = 14;
        private IntPtr _hookID = IntPtr.Zero;
        private readonly LowLevelMouseProc _proc;

        public event EventHandler<MouseButtonEventArgs> MouseDown;
        public event EventHandler<MouseWheelEventArgs> MouseWheel;

        public GlobalMouseHook() => _proc = HookCallback;

        public void Start()
        {
            using var module = Process.GetCurrentProcess().MainModule;
            _hookID = SetWindowsHookEx(WH_MOUSE_LL, _proc, GetModuleHandle(module.ModuleName), 0);
        }


        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                // Обработка кликов
                var button = GetButton(wParam);
                if (button.HasValue)
                {
                    Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        MouseDown?.Invoke(this, new MouseButtonEventArgs(
                            InputManager.Current.PrimaryMouseDevice,
                            0,
                            button.Value));
                    });
                }

                // Обработка колесика
                if ((int)wParam == 0x020A) // WM_MOUSEWHEEL
                {
                    var hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                    Debug.WriteLine($"Raw mouseData: 0x{hookStruct.mouseData:X8}");

                    // Извлекаем старшие 16 бит (HIWORD) и интерпретируем как знаковое число
                    short delta = (short)(hookStruct.mouseData >> 16);
                    int direction = Math.Sign(delta);

                    // Опционально: инвертировать направление (если необходимо)
                    // bool isReverseScroll = ...; // Проверка настроек системы
                    // if (isReverseScroll) direction *= -1;

                    Debug.WriteLine($"Delta: {delta} | Direction: {direction}");

                    Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        MouseWheel?.Invoke(this, new MouseWheelEventArgs(
                            InputManager.Current.PrimaryMouseDevice,
                            Environment.TickCount,
                            direction));
                    });
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        /* private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
         {
             if (nCode >= 0)
             {
                 // Обработка кликов
                 var button = GetButton(wParam);
                 if (button.HasValue)
                 {
                     Application.Current.Dispatcher.BeginInvoke(() =>
                     {
                         MouseDown?.Invoke(this, new MouseButtonEventArgs(
                             InputManager.Current.PrimaryMouseDevice,
                             0,
                             button.Value));
                     });
                 }

                 // Обработка колесика
                 // В классе GlobalMouseHook
                 // В классе GlobalMouseHook
                 // В классе GlobalMouseHook
                 // В классе GlobalMouseHook
                 // В классе GlobalMouseHook
                 if ((int)wParam == 0x020A) // WM_MOUSEWHEEL
                 {
                     var hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);

                     // Извлекаем младшие 16 бит (LOWORD) и интерпретируем как знаковое число
                     int rawLoword = (int)(hookStruct.mouseData & 0xFFFF);
                     int direction = (short)rawLoword; // Приводим к short для сохранения знака

                     // Учет обратной прокрутки (если нужно)
                     bool isReverseScroll = SystemParameters.SwapButtons; // Пример, замените на реальную проверку
                     if (isReverseScroll) direction *= -1;

                     // Нормализуем направление: 1 (Up), -1 (Down)
                     direction = Math.Sign(direction);

                     Debug.WriteLine($"Raw: 0x{hookStruct.mouseData:X8} | Direction: {direction}");

                     Application.Current.Dispatcher.BeginInvoke(() =>
                     {
                         MouseWheel?.Invoke(this, new MouseWheelEventArgs(
                             InputManager.Current.PrimaryMouseDevice,
                             Environment.TickCount,
                             direction));
                     });
                 }
             }
             return CallNextHookEx(_hookID, nCode, wParam, lParam);
         }*/

        private MouseButton? GetButton(IntPtr wParam)
        {
            switch ((int)wParam)
            {
                case 0x0201: return MouseButton.Left;
                case 0x0204: return MouseButton.Right;
                case 0x0207: return MouseButton.Middle;
                default: return null;
            }
        }

        public void Dispose() => UnhookWindowsHookEx(_hookID);

        #region WinAPI
        // Объявление структуры (важно!)
        // Структура MSLLHOOKSTRUCT
        [StructLayout(LayoutKind.Sequential)]
        public struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetModuleHandle(string lpModuleName);

        delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
        #endregion
    }
}