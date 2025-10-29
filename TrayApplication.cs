using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

namespace MongolianPhonetic
{
    public class TrayApplication : ApplicationContext
    {
        private NotifyIcon _trayIcon;
        private KeyboardHook _hook;
        private ContextMenuStrip _contextMenu;
        private ToolStripMenuItem _toggleItem;
        private ToolStripMenuItem _autoStartItem;
        private HotkeyForm _hotkeyForm;

        private const string APP_NAME = "MongolianPhonetic";
        private const string STARTUP_KEY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const int HOTKEY_ID = 1;
        private const int MOD_CONTROL = 0x0002;
        private const int MOD_SHIFT = 0x0004;
        private const int VK_M = 0x4D; // 'M' key

        public TrayApplication()
        {
            // Initialize keyboard hook
            _hook = new KeyboardHook();
            _hook.EnabledChanged += OnHookEnabledChanged;
            _hook.KeyProcessed += OnKeyProcessed;

            // Create a hidden form for hotkey registration
            _hotkeyForm = new HotkeyForm(this);
            _hotkeyForm.RegisterHotkey(HOTKEY_ID, MOD_CONTROL | MOD_SHIFT, VK_M);

            // Create context menu
            _contextMenu = new ContextMenuStrip();

            _toggleItem = new ToolStripMenuItem("Enabled", null, OnToggle);
            _toggleItem.Checked = _hook.IsEnabled;
            _contextMenu.Items.Add(_toggleItem);

            _contextMenu.Items.Add(new ToolStripSeparator());

            _autoStartItem = new ToolStripMenuItem("Start with Windows", null, OnToggleAutoStart);
            _autoStartItem.Checked = IsAutoStartEnabled();
            _contextMenu.Items.Add(_autoStartItem);

            _contextMenu.Items.Add(new ToolStripSeparator());

            var hotkeyItem = new ToolStripMenuItem("Hotkey: Ctrl+Shift+M");
            hotkeyItem.Enabled = false;
            _contextMenu.Items.Add(hotkeyItem);

            _contextMenu.Items.Add(new ToolStripSeparator());

            _contextMenu.Items.Add("About...", null, OnAbout);
            _contextMenu.Items.Add("Exit", null, OnExit);

            // Create tray icon
            _trayIcon = new NotifyIcon
            {
                Icon = CreateIcon(),
                ContextMenuStrip = _contextMenu,
                Visible = true,
                Text = "Mongolian Phonetic Keyboard"
            };

            _trayIcon.DoubleClick += OnToggle;

            UpdateIcon();

            // Show startup balloon tip only on first run
            #if !DEBUG
            _trayIcon.ShowBalloonTip(3000, "Mongolian Phonetic Keyboard",
                "Press Ctrl+Shift+M to toggle between Mongolian/English\nDouble-click icon to toggle",
                ToolTipIcon.Info);
            #endif
        }

        private Icon CreateIcon()
        {
            // Create a simple icon with "Мон" text
            Bitmap bitmap = new Bitmap(32, 32);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);

                // Draw a rounded rectangle background
                using (var brush = new SolidBrush(Color.FromArgb(0, 120, 212)))
                {
                    g.FillEllipse(brush, 2, 2, 28, 28);
                }

                // Draw "М" letter
                using (var font = new Font("Arial", 16, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.White))
                {
                    var format = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    g.DrawString("М", font, brush, new RectangleF(0, 0, 32, 32), format);
                }
            }

            IntPtr hIcon = bitmap.GetHicon();
            Icon icon = Icon.FromHandle(hIcon);
            return icon;
        }

        private void UpdateIcon()
        {
            if (_hook.IsEnabled)
            {
                _trayIcon.Text = "Mongolian Keyboard - ON (Ctrl+Shift+M to toggle)";
            }
            else
            {
                _trayIcon.Text = "Mongolian Keyboard - OFF (Ctrl+Shift+M to toggle)";
            }
        }

        private void OnToggle(object sender, EventArgs e)
        {
            _hook.IsEnabled = !_hook.IsEnabled;
        }

        public void ToggleKeyboard()
        {
            _hook.IsEnabled = !_hook.IsEnabled;
        }

        private void OnHookEnabledChanged(object sender, bool isEnabled)
        {
            _toggleItem.Checked = isEnabled;
            UpdateIcon();

            // Show balloon tip
            if (isEnabled)
            {
                _trayIcon.ShowBalloonTip(800, "Mongolian Keyboard",
                    "✓ Mongolian - Type in Cyrillic", ToolTipIcon.Info);
            }
            else
            {
                _trayIcon.ShowBalloonTip(800, "Mongolian Keyboard",
                    "✓ English - Type normally", ToolTipIcon.Info);
            }
        }

        private void OnKeyProcessed(object sender, string info)
        {
            // Debug: Show what keys are being processed
            System.Diagnostics.Debug.WriteLine($"Key processed: {info}");

            #if DEBUG
            // Show balloon tip only in debug mode
            _trayIcon.ShowBalloonTip(500, "Key Processed", info, ToolTipIcon.Info);
            #endif
        }

        private void OnToggleAutoStart(object sender, EventArgs e)
        {
            if (IsAutoStartEnabled())
            {
                DisableAutoStart();
            }
            else
            {
                EnableAutoStart();
            }
            _autoStartItem.Checked = IsAutoStartEnabled();
        }

        private bool IsAutoStartEnabled()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(STARTUP_KEY, false))
                {
                    if (key != null)
                    {
                        object value = key.GetValue(APP_NAME);
                        return value != null;
                    }
                }
            }
            catch { }
            return false;
        }

        private void EnableAutoStart()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(STARTUP_KEY, true))
                {
                    if (key != null)
                    {
                        string exePath = Application.ExecutablePath;
                        key.SetValue(APP_NAME, $"\"{exePath}\"");
                        MessageBox.Show("Auto-start enabled. The application will start automatically when Windows starts.",
                            "Auto-start Enabled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to enable auto-start: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisableAutoStart()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(STARTUP_KEY, true))
                {
                    if (key != null)
                    {
                        key.DeleteValue(APP_NAME, false);
                        MessageBox.Show("Auto-start disabled.",
                            "Auto-start Disabled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to disable auto-start: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnAbout(object sender, EventArgs e)
        {
            MessageBox.Show(
                "Mongolian Phonetic Keyboard v1.0\n\n" +
                "Created by Amgaa G.\n" +
                "License: LGPL\n\n" +
                "This application provides phonetic Mongolian keyboard layout for Windows.\n\n" +
                "Usage:\n" +
                "- Double-click the tray icon to enable/disable\n" +
                "- Right-click for options\n" +
                "- Type phonetically: 'sain baina uu' → 'сайн байна уу'\n\n" +
                "Multi-character combinations:\n" +
                "ye→е, yo→ё, ts→ц, ch→ч, sh→ш, yu→ю, ya→я\n" +
                "ai→ай, ei→эй, oi→ой, ui→уй, qi→өй, wi→үй, ii→ий",
                "About Mongolian Phonetic Keyboard",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void OnExit(object sender, EventArgs e)
        {
            _trayIcon.Visible = false;
            _hook.Dispose();
            Application.Exit();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _hotkeyForm?.Dispose();
                _trayIcon?.Dispose();
                _hook?.Dispose();
            }
            base.Dispose(disposing);
        }

        // Hidden form for hotkey registration
        private class HotkeyForm : Form
        {
            private TrayApplication _parent;

            [DllImport("user32.dll")]
            private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

            [DllImport("user32.dll")]
            private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

            private const int WM_HOTKEY = 0x0312;

            public HotkeyForm(TrayApplication parent)
            {
                _parent = parent;
                // Make form invisible
                this.FormBorderStyle = FormBorderStyle.None;
                this.ShowInTaskbar = false;
                this.WindowState = FormWindowState.Minimized;
                this.Opacity = 0;
                this.Width = 0;
                this.Height = 0;
            }

            public void RegisterHotkey(int id, int modifiers, int key)
            {
                if (!RegisterHotKey(this.Handle, id, modifiers, key))
                {
                    MessageBox.Show("Failed to register hotkey Ctrl+Shift+M. It may be in use by another application.",
                        "Hotkey Registration Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_HOTKEY)
                {
                    _parent.ToggleKeyboard();
                }
                base.WndProc(ref m);
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    UnregisterHotKey(this.Handle, HOTKEY_ID);
                }
                base.Dispose(disposing);
            }
        }
    }
}
