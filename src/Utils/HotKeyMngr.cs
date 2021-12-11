using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace ScreenCapture
{
    public class HotKeyMngr
    {
        // https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-registerhotkey
        // https://stackoverflow.com/a/27309185
        // Registers a hot key with Windows.
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        // Unregisters the hot key with Windows.
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        // The message type on which hotkey messages are received
        private const int WM_HOTKEY = 0x0312;

        private readonly Window _window;
        private readonly IntPtr _windowHandle;
        private readonly List<HotKeyEntry> _registeredHotKeys = new List<HotKeyEntry>();
        private int _currentID = 0;

        public HotKeyMngr(Window wd)
        {
            _window = wd;
            _windowHandle = new WindowInteropHelper(wd).Handle;
            HwndSource source = HwndSource.FromHwnd(_windowHandle);
            source.AddHook(new HwndSourceHook(WndProc));
        }

        ~HotKeyMngr()
        {
            CleanUpHotKeys();
        }

        public int EnableHotKey(Keys vk, ModifierKeys mk, Action performOnHotkeyPressed, bool setHandled = true)
        {
            System.Diagnostics.Debug.WriteLine($"Setting up Hot Key for {vk}, {mk}");

            HwndSource source = HwndSource.FromHwnd(_windowHandle);
            source.AddHook(new HwndSourceHook(WndProc));
            _window.Closing += (s, e) => CleanUpHotKeys();

            var v = (uint)mk;
            if (RegisterHotKey(_windowHandle, _currentID, (uint)mk, (uint)vk))
            {
                _registeredHotKeys.Add(new HotKeyEntry(_currentID, vk, mk, performOnHotkeyPressed, setHandled));
                _currentID++;
                return _currentID - 1;
            }

            return -1;
        }

        public bool DisableHotKey(int entry_id)
        {
            var entry = _registeredHotKeys.Where(e => e.ID == entry_id).FirstOrDefault();
            if (entry != default)
            {
                if (UnregisterHotKey(_windowHandle, entry_id))
                {
                    _registeredHotKeys.Remove(entry);
                    return true;
                }
            }

            return false;
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                // https://docs.microsoft.com/de-at/windows/win32/inputdev/wm-hotkey

                // The higher order of 'lParam' specify the keys, 
                // the lower order the modifiers
                var key = (Keys)(((int)lParam >> 16) & 0xFFFF);
                var modifier = (ModifierKeys)((int)lParam & 0xFFFF);

                bool anyHandled = false;
                foreach (var entry in _registeredHotKeys.Where(e => e.MK == modifier && e.VK == key))
                {
                    entry.ActionToPerform();
                    anyHandled |= entry.ShouldSetHandled;
                }

                if (anyHandled)
                    handled = true;
            }

            return IntPtr.Zero;
        }

        public void CleanUpHotKeys()
        {
            var i = 0;
            var maxLoops = _registeredHotKeys.Count * 3;

            // Try multiple times to unregister registered hotkeys
            // For loop is not possible as 'DisableHotKey' changes the items of '_registeredHotKeys'
            while (i < maxLoops && _registeredHotKeys.Count > 0)
                DisableHotKey(_registeredHotKeys[i % _registeredHotKeys.Count].ID);
        }

        [Flags]
        public enum ModifierKeys : uint
        {
            MOD_NONE = 0x0000,
            MOD_ALT = 0x0001,
            MOD_CONTROL = 0x0002,
            MOD_SHIFT = 0x0004,
            MOD_WIN = 0x0008,
            MOD_NOREPEAT = 0x4000,
        }

        private class HotKeyEntry
        {
            public int ID;
            public Keys VK;
            public ModifierKeys MK;
            public Action ActionToPerform;
            public bool ShouldSetHandled;

            public HotKeyEntry(int id, Keys vk, ModifierKeys mk, Action a, bool setHandled)
            {
                ID = id;
                VK = vk;

                // NOREPEAT won't be transmitted back therefore remove it
                MK = mk & ~ModifierKeys.MOD_NOREPEAT;
                ActionToPerform = a;
                ShouldSetHandled = setHandled;
            }
        }
    }
}
