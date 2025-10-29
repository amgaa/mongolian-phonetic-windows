using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MongolianPhonetic
{
    // Simple test class to verify SendInput works
    public static class InputTester
    {
        [DllImport("user32.dll")]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        private static extern IntPtr GetMessageExtraInfo();

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public int type;
            public InputUnion u;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)]
            public KEYBDINPUT ki;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        public static void SendUnicodeString(string text)
        {
            foreach (char c in text)
            {
                SendUnicodeChar(c);
            }
        }

        private static void SendUnicodeChar(char c)
        {
            INPUT[] inputs = new INPUT[2];

            inputs[0] = new INPUT
            {
                type = 1, // INPUT_KEYBOARD
                u = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = 0,
                        wScan = c,
                        dwFlags = 0x0004, // KEYEVENTF_UNICODE
                        time = 0,
                        dwExtraInfo = GetMessageExtraInfo()
                    }
                }
            };

            inputs[1] = new INPUT
            {
                type = 1,
                u = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = 0,
                        wScan = c,
                        dwFlags = 0x0004 | 0x0002, // KEYEVENTF_UNICODE | KEYEVENTF_KEYUP
                        time = 0,
                        dwExtraInfo = GetMessageExtraInfo()
                    }
                }
            };

            uint result = SendInput(2, inputs, Marshal.SizeOf(typeof(INPUT)));

            if (result == 0)
            {
                MessageBox.Show($"SendInput failed for char '{c}'. Error: {Marshal.GetLastWin32Error()}");
            }
        }
    }
}
