using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace SnapDrop
{
    /// <summary>Converts between "Ctrl+Shift+S" text and Win32 modifier/virtual-key values.</summary>
    static class Hotkey
    {
        public struct Combo { public uint Mods; public uint Vk; }

        public static bool TryParse(string text, out Combo combo)
        {
            combo = new Combo();
            if (string.IsNullOrWhiteSpace(text)) return false;
            uint mods = 0;
            Key key = Key.None;
            foreach (var raw in text.Split('+'))
            {
                string t = raw.Trim();
                switch (t.ToLowerInvariant())
                {
                    case "ctrl": case "control": mods |= 0x2; break;
                    case "shift": mods |= 0x4; break;
                    case "alt": mods |= 0x1; break;
                    case "win": mods |= 0x8; break;
                    default:
                        if (t.Length == 1 && char.IsDigit(t[0])) key = (Key)((int)Key.D0 + (t[0] - '0'));
                        else if (!Enum.TryParse(t, true, out key)) return false;
                        break;
                }
            }
            if (key == Key.None) return false;
            int vk = KeyInterop.VirtualKeyFromKey(key);
            if (vk == 0) return false;
            combo.Mods = mods;
            combo.Vk = (uint)vk;
            return true;
        }

        public static string Format(ModifierKeys mods, Key key)
        {
            var parts = new System.Collections.Generic.List<string>();
            if ((mods & ModifierKeys.Control) != 0) parts.Add("Ctrl");
            if ((mods & ModifierKeys.Shift) != 0) parts.Add("Shift");
            if ((mods & ModifierKeys.Alt) != 0) parts.Add("Alt");
            if ((mods & ModifierKeys.Windows) != 0) parts.Add("Win");
            parts.Add(KeyName(key));
            return string.Join("+", parts);
        }

        static string KeyName(Key k)
        {
            if (k >= Key.D0 && k <= Key.D9) return ((int)k - (int)Key.D0).ToString();
            return k.ToString();
        }

        /// <summary>A single key on its own is only allowed for function keys and Print Screen.</summary>
        public static bool NeedsModifier(Key k)
        {
            return !((k >= Key.F1 && k <= Key.F24) || k == Key.PrintScreen || k == Key.Snapshot);
        }
    }

    /// <summary>Turns a read-only TextBox into a "press the keys you want" box.</summary>
    static class HotkeyBox
    {
        public static void Attach(TextBox box, Action changed)
        {
            box.IsReadOnly = true;
            box.IsReadOnlyCaretVisible = false;
            box.Cursor = Cursors.Hand;
            box.PreviewKeyDown += (s, e) =>
            {
                e.Handled = true;
                Key key = e.Key == Key.System ? e.SystemKey : e.Key;
                if (key == Key.LeftCtrl || key == Key.RightCtrl || key == Key.LeftShift || key == Key.RightShift ||
                    key == Key.LeftAlt || key == Key.RightAlt || key == Key.LWin || key == Key.RWin ||
                    key == Key.ImeProcessed || key == Key.DeadCharProcessed) return;

                var mods = Keyboard.Modifiers;
                if (mods == ModifierKeys.None && (key == Key.Back || key == Key.Delete)) { box.Text = ""; changed(); return; }
                if (key == Key.Tab || key == Key.Escape) { Keyboard.ClearFocus(); return; }
                if (mods == ModifierKeys.None && Hotkey.NeedsModifier(key)) return;   // ignore plain letters

                box.Text = Hotkey.Format(mods, key == Key.Snapshot ? Key.PrintScreen : key);
                changed();
            };
            box.GotKeyboardFocus += (s, e) => box.SelectAll();
        }
    }
}
