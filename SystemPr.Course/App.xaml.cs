using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using SystemPr.Course.Code;
using SystemPr.Course.Models;

namespace SystemPr.Course {
    public partial class App {
        private static SystemSemaphoreHelper _sem;
        public static SettingModel Setting { get; set; }

        private static uint _countSem;
        private static string _msgTitle;

        [System.STAThreadAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public static void Main() {
            _sem = new SystemSemaphoreHelper("app_semaphor:c79ef123-5330-42b0-b437-d7701ea089cf");
            Setting = new SettingModel(Path.GetFullPath("setting.opt"));
            Setting.Load();

            _countSem = Setting.CopyCount - (uint)AppHelper.GetAllProcessesButThis().Length; // !
            _msgTitle = "id=" + AppHelper.ProcessId;
            
            if (Setting.CopyCount == 0) {
                RunApp();
            }
            else {
                _sem.Create(Setting.CopyCount);
                if (_sem.Take())
                    RunApp();
                else {
                    TakeSemaphorFailed();
                }
            }
        }
        private static void RunApp() {
            var app = new App();
            app.InitializeComponent();
            app.Exit += AppOnExit;
            app.Run();
        }
        private static void AppOnExit(object sender, ExitEventArgs args) {
            if (_sem.IsExist())
                _sem.Close();
        }
        
        private static void TakeSemaphorFailed() {
            var procs = AppHelper.GetAllProcessesButThis();
            var count = procs.Length;
            switch (count) {
                case 1:
                    MessageBox.Show("Application is already launched", "Info");
                    WinAPIHelper.ShowWindow(procs[0].MainWindowHandle, ShowWindowCommands.Normal);
                    break;
                default:
                    MessageBox.Show("Allowable limit of " + count + " copies exhausted", "Info");
                    break;
            }
        }
        public static void TakeSemaphore() {
            var copyCount = Setting.CopyCount;
            if (copyCount <= 0) 
                return;
            _sem.Create(copyCount);
            _sem.Take();
        }
        public static void ReleaseSemaphore() {
            if (!_sem.IsRefExist())
                return;
            _sem.Release();
            _sem.Close();
        }

        public static void ReleaseGlobalSemaphore() {
            foreach (var p in AppHelper.GetAllProcessesButThis())
                WinAPIHelper.SendStringMessage(p.MainWindowHandle, "ReleaseSemaphore");
            _sem.Close();
        }
        public static void TakeGlobalSemaphore() {
            TakeSemaphore();
            foreach (var p in AppHelper.GetAllProcessesButThis())
                WinAPIHelper.SendStringMessage(p.MainWindowHandle, "TakeSemaphore");
        }
        public static void UpdateGlobalSettings() {
            foreach (var p in AppHelper.GetAllProcessesButThis())
                WinAPIHelper.SendStringMessage(p.MainWindowHandle, "UpdateSettings");

            if (_sem.Count != Setting.CopyCount) {
                ReleaseGlobalSemaphore();
                TakeGlobalSemaphore();
            }
        }
    }
}
