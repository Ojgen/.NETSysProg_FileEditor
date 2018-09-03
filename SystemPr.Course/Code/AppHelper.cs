using System.Diagnostics;
using System.Linq;

namespace SystemPr.Course.Code {
    public class AppHelper {
        static AppHelper() {
            CurentProcess = Process.GetCurrentProcess();
            ProcessName = CurentProcess.ProcessName;
            ProcessId = CurentProcess.Id;
        }

        public static string ProcessName { get; private set; }
        public static int ProcessId { get; private set; }
        public static Process CurentProcess { get; private set; }

        public static int Count {
            get { return GetAllProcesses().Length; }
        }

        public static Process[] GetAllProcesses() {
            return Process.GetProcessesByName(ProcessName).ToArray();
        }
        public static Process[] GetAllProcessesButThis() {
            return Process.GetProcessesByName(ProcessName).Where(p => p.Id != ProcessId).ToArray();
        } 
    }
}
