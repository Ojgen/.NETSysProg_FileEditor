using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemPr.Course {
    public enum ShowWindowCommands {
        Hide, Normal, ShowMinimized, ShowMaximized, ShowNoActivate, Show, Minimize, ShowMinNoActive, ShowNA, Restore, ShowDefault, ForceMinimize
    }
    public enum SendCommands {
        ReleaseSemaphore, TakeSemaphore, UpdateSettings
    }
    public enum FileTypes { Image, Text, Other };
    public enum CountLautches { NoLimit, Single, Limit }
}
