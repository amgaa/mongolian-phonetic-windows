using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace MongolianPhonetic
{
    public class KeyboardHook : IDisposable
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_CHAR = 0x0102;
        private const int EM_REPLACESEL = 0x00C2;

        private LowLevelKeyboardProc _proc;
        private IntPtr _hookID = IntPtr.Zero;
        private MongolianPhoneticMapper _mapper;
        private bool _isEnabled = true;
        private volatile bool _isSending = false;
        private System.Windows.Forms.Timer _sendTimer;
        private string _pendingText;
        private int _pendingBackspaces;
        private static System.IO.StreamWriter _logFile;

        public event EventHandler<bool> EnabledChanged;
        public event EventHandler<string> KeyProcessed;

        private static void Log(string message)
        {
            #if DEBUG
            try
            {
                if (_logFile == null)
                {
                    string logPath = System.IO.Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                        "MongolianKeyboard_Debug.txt");
                    _logFile = new System.IO.StreamWriter(logPath, true);
                    _logFile.AutoFlush = true;
                    _logFile.WriteLine($"\n=== Session started at {DateTime.Now} ===");
                }
                _logFile.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
                System.Diagnostics.Debug.WriteLine(message);
            }
            catch { }
            #endif
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    if (!_isEnabled)
                    {
                        _mapper.Reset();
                    }
                    EnabledChanged?.Invoke(this, _isEnabled);
                }
            }
        }

        public KeyboardHook()
        {
            _mapper = new MongolianPhoneticMapper();
            _proc = HookCallback;
            _hookID = SetHook(_proc);

            // Timer for sending input outside of hook callback
            _sendTimer = new System.Windows.Forms.Timer();
            _sendTimer.Interval = 1;
            _sendTimer.Tick += SendTimer_Tick;
        }

        private void SendTimer_Tick(object sender, EventArgs e)
        {
            _sendTimer.Stop();
            _isSending = true;

            try
            {
                // Send backspaces if needed
                for (int i = 0; i < _pendingBackspaces; i++)
                {
                    SendBackspace();
                }

                // Send the pending text
                if (!string.IsNullOrEmpty(_pendingText))
                {
                    try
                    {
                        SendText(_pendingText);
                    }
                    catch
                    {
                        // Try direct unicode input as fallback
                        foreach (char c in _pendingText)
                        {
                            SendUnicodeChar(c);
                        }
                    }
                }
            }
            catch { }
            finally
            {
                _pendingText = null;
                _pendingBackspaces = 0;
                _isSending = false;
            }
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && _isEnabled && !_isSending && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Keys key = (Keys)vkCode;

                // Get modifier states
                bool shift = (Control.ModifierKeys & Keys.Shift) != 0;
                bool ctrl = (Control.ModifierKeys & Keys.Control) != 0;
                bool alt = (Control.ModifierKeys & Keys.Alt) != 0;

                // Don't intercept if Ctrl or Alt is pressed (allow shortcuts)
                if (ctrl || alt)
                {
                    _mapper.Reset();
                    return CallNextHookEx(_hookID, nCode, wParam, lParam);
                }

                // Process the key
                var result = _mapper.ProcessKey(key, shift);

                if (result.ShouldSuppress)
                {
                    // Suppress the original key and queue output for sending
                    if (result.OutputText != null)
                    {
                        // Queue the text to be sent via timer (outside hook callback)
                        _pendingText = result.OutputText;
                        _pendingBackspaces = result.BackspacesToSend;
                        _sendTimer.Start();
                    }
                    return (IntPtr)1; // Suppress the key
                }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private void SendBackspace()
        {
            // Use keybd_event for backspace (simpler and more reliable)
            keybd_event(0x08, 0, 0, UIntPtr.Zero);
            Thread.Sleep(5);
            keybd_event(0x08, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        private void SendText(string text)
        {
            try
            {
                // Get the foreground window
                IntPtr foregroundWindow = GetForegroundWindow();
                if (foregroundWindow == IntPtr.Zero) return;

                // Get the thread IDs
                uint foregroundThreadId = GetWindowThreadProcessId(foregroundWindow, IntPtr.Zero);
                uint currentThreadId = GetCurrentThreadId();

                // Attach to the foreground window's thread to get its focus
                IntPtr focusedWindow = IntPtr.Zero;
                bool attached = false;

                if (foregroundThreadId != currentThreadId)
                {
                    attached = AttachThreadInput(currentThreadId, foregroundThreadId, true);
                }

                focusedWindow = GetFocus();

                if (attached)
                {
                    AttachThreadInput(currentThreadId, foregroundThreadId, false);
                }

                // Use the focused window if we got it, otherwise use foreground window
                IntPtr targetWindow = focusedWindow != IntPtr.Zero ? focusedWindow : foregroundWindow;

                // Get the window class to determine the control type
                System.Text.StringBuilder className = new System.Text.StringBuilder(256);
                GetClassName(targetWindow, className, className.Capacity);
                string windowClass = className.ToString();

                // Check if it's a Rich Edit control (Notepad, etc.)
                bool isRichEdit = windowClass.StartsWith("RichEdit", StringComparison.OrdinalIgnoreCase) ||
                                  windowClass.StartsWith("RICHEDIT", StringComparison.OrdinalIgnoreCase) ||
                                  windowClass.Equals("Edit", StringComparison.OrdinalIgnoreCase);

                if (isRichEdit)
                {
                    // Use EM_REPLACESEL for Rich Edit controls
                    IntPtr textPtr = Marshal.StringToHGlobalUni(text);
                    try
                    {
                        SendMessage(targetWindow, EM_REPLACESEL, (IntPtr)1, textPtr);
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(textPtr);
                    }
                }
                else
                {
                    // Use WM_CHAR for other controls
                    foreach (char c in text)
                    {
                        SendMessage(targetWindow, WM_CHAR, (IntPtr)c, IntPtr.Zero);
                    }
                }
            }
            catch { }
        }

        private void SendUnicodeViaKeybdEvent(char c)
        {
            // Send unicode character using keybd_event with KEYEVENTF_UNICODE flag
            const uint KEYEVENTF_UNICODE = 0x0004;

            // Key down
            keybd_event(0, (byte)(c & 0xFF), KEYEVENTF_UNICODE, UIntPtr.Zero);
            keybd_event(0, (byte)((c >> 8) & 0xFF), KEYEVENTF_UNICODE, UIntPtr.Zero);

            // Key up
            keybd_event(0, (byte)(c & 0xFF), KEYEVENTF_UNICODE | KEYEVENTF_KEYUP, UIntPtr.Zero);
            keybd_event(0, (byte)((c >> 8) & 0xFF), KEYEVENTF_UNICODE | KEYEVENTF_KEYUP, UIntPtr.Zero);

            Log($"Sent unicode char via keybd_event: '{c}' (U+{((int)c):X4})");
        }

        private void SendCtrlV()
        {
            // Use keybd_event instead of SendInput (more compatible)
            const byte VK_CONTROL = 0x11;
            const byte VK_V = 0x56;

            Log("Sending Ctrl+V");

            keybd_event(VK_CONTROL, 0, 0, UIntPtr.Zero);
            Thread.Sleep(5);

            keybd_event(VK_V, 0, 0, UIntPtr.Zero);
            Thread.Sleep(5);

            keybd_event(VK_V, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            Thread.Sleep(5);

            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);

            Log("Ctrl+V complete");
        }

        private void SendUnicodeChar(char c)
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

            SendInput(2, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        private void SendUnicodeKey(ushort keyCode, bool keyDown)
        {
            INPUT[] inputs = new INPUT[1];

            inputs[0] = new INPUT
            {
                type = 1,
                u = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = keyCode,
                        wScan = 0,
                        dwFlags = (uint)(keyDown ? 0 : 0x0002), // KEYEVENTF_KEYUP
                        time = 0,
                        dwExtraInfo = GetMessageExtraInfo()
                    }
                }
            };

            SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        public void Dispose()
        {
            if (_sendTimer != null)
            {
                _sendTimer.Stop();
                _sendTimer.Dispose();
                _sendTimer = null;
            }

            if (_hookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookID);
                _hookID = IntPtr.Zero;
            }

            #if DEBUG
            if (_logFile != null)
            {
                _logFile.WriteLine("=== Session ended ===");
                _logFile.Close();
                _logFile = null;
            }
            #endif
        }

        #region Win32 API

        private const uint KEYEVENTF_KEYUP = 0x0002;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        private static extern IntPtr GetMessageExtraInfo();

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool PostMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr GetFocus();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr processId);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, System.Text.StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

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

        #endregion
    }

    public class KeyProcessResult
    {
        public bool ShouldSuppress { get; set; }
        public string OutputText { get; set; }
        public int BackspacesToSend { get; set; }
    }
}
