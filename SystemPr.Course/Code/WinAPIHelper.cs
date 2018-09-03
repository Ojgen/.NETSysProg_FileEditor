using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SystemPr.Course.Code {
    class WinAPIHelper {

        public enum WndMsgs : uint {
            WM_COPYDATA = 0x4A
        }

        //https://msdn.microsoft.com/en-us/library/windows/desktop/ms649010%28v=vs.85%29.aspx
        public struct COPYDATASTRUCT {
            public IntPtr DwData;
            public int CbData;
            public IntPtr LpData;
        }

        // http://pinvoke.net/default.aspx/user32/ShowWindow.html
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, ShowWindowCommands nCmdShow);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, WndMsgs msg, IntPtr wParam, ref COPYDATASTRUCT lParam);


        //[DllImport("kernel32.dll", SetLastError = true)]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //public static extern bool CloseHandle(IntPtr hObject);


        public static int SendStringMessage(IntPtr hWnd, string msg) {

            if (hWnd != IntPtr.Zero) {
                var copyDataStruct = new COPYDATASTRUCT {
                    DwData = IntPtr.Zero,
                    LpData = Marshal.StringToHGlobalAnsi(msg),
                    CbData = Marshal.SystemDefaultCharSize * msg.Length
                };

                SendMessage(hWnd, WndMsgs.WM_COPYDATA, IntPtr.Zero, ref copyDataStruct);

                Marshal.FreeHGlobal(copyDataStruct.LpData);
            }

            return 0;
        }

    }
}
