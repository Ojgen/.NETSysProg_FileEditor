using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Hooks {
    public class KeyboardHook : HookBase {
        private class HotKeys {
            public HotKeys() {
                Ctrl = Alt = Shift = false;
                VisualKeys = new HashSet<Key>();
            }

            public HashSet<Key> VisualKeys { get; set; }
            public bool Ctrl;
            public bool Alt;
            public bool Shift;

            public override bool Equals(object obj) {
                var o = obj as HotKeys;
                if (o == null)
                    return false;
                var res = CompareHotKeys(o.VisualKeys);
                if (o.Ctrl != Ctrl || o.Alt != Alt || o.Shift != Shift)
                    res = false;
                return res;
            }
            private bool CompareHotKeys(HashSet<Key> obj) {
                if (VisualKeys == obj) 
                    return true;
                if ((VisualKeys == null) || (obj == null) || (VisualKeys.Count != obj.Count)) 
                    return false;
                HashSet<Key>.Enumerator e1 = VisualKeys.GetEnumerator(), e2 = obj.GetEnumerator();
                while (e1.MoveNext() && e2.MoveNext())
                    if (e1.Current != e2.Current)
                        return false;
                return true;
            }
        }

        // https://msdn.microsoft.com/en-us/library/windows/desktop/ms644967%28v=vs.85%29.aspx
        [StructLayout(LayoutKind.Sequential)]
        private struct KeyboardHookStruct {
            public readonly int vkCode;
            public readonly int scanCode;
            public readonly int flags;
            public readonly int time;
            public readonly IntPtr dwExtraInfo;
        }
        
        const int WH_KEYBOARD_LL = 13;
        const int WM_KEYDOWN = 0x100;
        const int WM_KEYUP = 0x101;
        const int WM_SYSKEYDOWN = 0x104;
        const int WM_SYSKEYUP = 0x105;


        public event KeysEventHandler KeyDown;
        public event KeysEventHandler KeyUp;

        public bool IsCallNextHookIfHotKey { get; set; }
        private bool _isUp;
        private HotKeys _keysDown;
        private HotKeys _keys = new HotKeys();
        private List<HotKeys> _hookedKeys = new List<HotKeys>();

        // constructor
        public KeyboardHook() {
            IsCallNextHookIfHotKey = true;
            IsCallNextHook = true;
        }

        public override void SetHook() {
            base.SetHook();
            Callback = HookCallBack;
            HHook = SetWindowsHookEx(WH_KEYBOARD_LL, Callback, GetModuleHandle(IntPtr.Zero), 0);
        }
        public override void UnHook() {
            base.UnHook();
            UnhookWindowsHookEx(HHook);
        }

        public void AddHotKey(string hotKeys) {
            var hk = CreateHotKey(hotKeys);
            if(hk != null)
                _hookedKeys.Add(hk);
        }
        public void DelHotKey(string hotKeys) {
            var hk = CreateHotKey(hotKeys);
            var delHk = _hookedKeys.SingleOrDefault(h => h.Equals(hk));
            if (delHk != null)
                _hookedKeys.Remove(delHk);
        }

        private static HotKeys CreateHotKey(string hotKeys) {
            var keys = hotKeys.Split('+').ToList();
            for (var i = 0; i < keys.Count; i++) {
                keys[i] = keys[i].Trim().ToLower();

                try {
                    var num = Int32.Parse(keys[i]);
                    if (num >= 0 && num <= 9)
                        keys[i] = "d" + num;
                    else
                        return null;
                }
                catch { }
                
            }

            var hk = new HotKeys {
                Ctrl = keys.Contains("ctrl"),
                Alt = keys.Contains("alt"),
                Shift = keys.Contains("shift"),
                VisualKeys = new HashSet<Key>()
            };

            if (hk.Ctrl) keys.Remove("ctrl");
            if (hk.Alt) keys.Remove("alt");
            if (hk.Shift) keys.Remove("shift");

            foreach (var key in keys) {
                try {
                    hk.VisualKeys.Add((Key) Enum.Parse(typeof (Key), key, true));
                }
                catch {
                    return null;
                }
            }

            return hk;
        }

        // https://msdn.microsoft.com/en-us/library/windows/desktop/ms644985%28v=vs.85%29.aspx
        // https://msdn.microsoft.com/en-us/library/windows/desktop/ms644984%28v=vs.85%29.aspx
        public IntPtr HookCallBack(int nCode, IntPtr wParam, IntPtr lParam) {
            if(nCode < 0)
                return IsCallNextHook ? CallNextHookEx(HHook, nCode, wParam, lParam) : new IntPtr(1);

            var khs = (KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct));
            var key = KeyInterop.KeyFromVirtualKey(khs.vkCode);

            switch (wParam.ToInt32()) {
                case WM_KEYDOWN:
                case WM_SYSKEYDOWN:
                    ActionKey(true, key);
                    if (CheckDownHotKey()) {
                        if(KeyDown != null)
                            SendEventHotKey(true, KeyDown);
                    }
                    if(IsCallNextHookIfHotKey)
                        break;
                    return new IntPtr(1);

                case WM_KEYUP:
                case WM_SYSKEYUP:
                    ActionKey(false, key);
                    if (CheckUpHotKey()) {
                        if(KeyUp != null)
                            SendEventHotKey(false, KeyUp);
                        _isUp = false;
                        _keysDown = null;
                    }
                    if(IsCallNextHookIfHotKey)
                        break;
                    return new IntPtr(1);
            }

            return IsCallNextHook ? CallNextHookEx(HHook, nCode, wParam, lParam) : new IntPtr(1);
        }

        public void ActionKey(bool flag, Key key) {
            switch (key) {
                case Key.LeftCtrl:
                case Key.RightCtrl:
                    _keys.Ctrl = flag;
                    return;
                case Key.LeftAlt:
                case Key.RightAlt:
                    _keys.Alt = flag;
                    return;
                case Key.LeftShift:
                case Key.RightShift:
                    _keys.Shift = flag;
                    return;
            }
            if (flag)
                _keys.VisualKeys.Add(key);
            else
                _keys.VisualKeys.Remove(key);
        }
        public bool CheckDownHotKey() {
            _keysDown = null;
            foreach (var hk in _hookedKeys) {
                if (!hk.Equals(_keys)) {
                    continue;
                }
                _keysDown = hk;
            }

            return _keysDown != null;
        }
        public bool CheckUpHotKey() {
            return _isUp = !_isUp && (_keysDown != null && !_keysDown.Equals(_keys));
        }
        public void SendEventHotKey(bool flag, KeysEventHandler h) {
            var kea = new KeysEventArgs(_keysDown.Ctrl, _keysDown.Alt, _keysDown.Shift, _keysDown.VisualKeys, flag);
            h.Invoke(this, kea);
        }
    }
}
