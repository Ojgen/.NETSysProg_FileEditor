using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Hooks {
    public delegate IntPtr HookCallbackDelegate(int nCode, IntPtr wParam, IntPtr lParam);
    public class HookBase {
        [DllImport("user32.dll", SetLastError = true)]
        protected static extern IntPtr SetWindowsHookEx(int idHook, HookCallbackDelegate lpfn, IntPtr hMod, int dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        protected static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("Kernel32.dll", SetLastError = true)]
        protected static extern IntPtr GetModuleHandle(IntPtr lpModuleName);

        [DllImport("user32.dll", SetLastError = true)]
        protected static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);


        protected IntPtr HHook;
        protected HookCallbackDelegate Callback;

        ~HookBase() {
            UnHook();
        }

        //public IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam) {
        //    if(nCode < 0)
        //        return CallNextHookEx(_hHook, nCode, wParam, lParam);
        //    return new IntPtr(1);
        //}

        public virtual void SetHook() { }
        public virtual void UnHook() {
            UnhookWindowsHookEx(HHook);
        }
        public bool IsCallNextHook { get; set; }

    }
}
