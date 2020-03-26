using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace SleepyDisplay
{
    public enum DisplayStatus
    {
        PowerOn = -1,
        PowerOff = 2
    }

    public static class DisplaySleepHelper
    {
        private const int HWND_BROADCAST = 0xffff;
        private const uint WM_SYSCOMMAND = 0x112;
        private const uint SC_MONITORPOWER = 0xF170;
        private const int MOUSEEVENTF_MOVED = 0x0001;

        [DllImport("User32.dll")]
        private static extern int SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);


        [DllImport("user32.dll", EntryPoint = "GetDesktopWindow")]
        private static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        public static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public int mouseData;
            public int dwFlags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            public int type;
            public MOUSEINPUT mi;
        }




        /// <summary>
        /// ディスプレイの電源を切る
        /// </summary>
        public static void SleepDisplay(bool lockWorkstation = false)
        {
            //マウス移動を考慮し0.5秒待機する 

            Thread.Sleep(500);

            SendMessage((IntPtr)HWND_BROADCAST, WM_SYSCOMMAND, (IntPtr)SC_MONITORPOWER, (IntPtr)DisplayStatus.PowerOff);
        }

        /// <summary>
        /// ディスプレイの電源を入れる
        /// </summary>
        /// <param name="mouseInput">Win32APIパターンを用いるか原始的にマウスカーソル移動を用いるか</param>
        public static void WakeupDisplay(bool mouseInput = false)
        {
            if (mouseInput)
            {
                SendMessage((IntPtr)HWND_BROADCAST, WM_SYSCOMMAND, (IntPtr)SC_MONITORPOWER, (IntPtr)DisplayStatus.PowerOn);
            }
            else
            {
                var inputs = new INPUT[2];
                inputs[0].mi.dx = 5;
                inputs[0].mi.dwFlags = MOUSEEVENTF_MOVED;
                inputs[1].mi.dx = -5;
                inputs[1].mi.dwFlags = MOUSEEVENTF_MOVED;

                SendInput(2, inputs, Marshal.SizeOf(inputs[0]));
            }
        }
    }
}
