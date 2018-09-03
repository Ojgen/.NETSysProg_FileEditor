using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using SystemPr.Course.Code;
using System.ComponentModel;
using System.Windows.Input;
using SystemPr.Course.ViewModels;
using Hooks;

namespace SystemPr.Course {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string[] imgExts, textExts;
        private SettingsViewModel _svw;
        private KeyboardHook _keyboardHook;

        public MainWindow() {
            InitializeComponent();
            Thread.CurrentThread.Name = "mainThread"; // delete

            imgExts = new[] { ".jpg", ".jpeg", ".png", ".gif", ".tiff" };
            textExts = new[] { ".txt", ".rtf", ".xml", ".svg", ".htm", ".html", ".json", ".log" };

            TabControl.Tabs = new ObservableCollection<ITabViewModel>();
            TabControl.ItemsSource = TabControl.Tabs;

            Title += " id=" + AppHelper.ProcessId;
            SourceInitialized += OnSourceInitialized;
            Closing += OnClosing;


            _keyboardHook = new KeyboardHook();
            _keyboardHook.SetHook();
            _keyboardHook.KeyDown += OnHotKeys;

            AddHotKeys(ItemSave.InputGestureText);
            AddHotKeys(ItemOpenFile.InputGestureText);
            AddHotKeys(ItemSaveAs.InputGestureText);

            //
            AddHotKeys("Alt+A+Z");
            AddHotKeys("Ctrl+Alt+Delete");
            
        }

        
        private void OpenFiles(bool isMult=false) {
            var filter = imgExts.Aggregate("Image Files|", (c, el) => c + ("*" + el + ";"));
            filter = textExts.Aggregate(filter + "|Text Files|", (c, el) => c + ("*" + el + ";"));
            filter += "|All Files|*.*";

            FileDialog fd = new OpenFileDialog {
                Multiselect = isMult,
                Filter = filter,
                FilterIndex = 3
            };

            if (fd.ShowDialog() == null) 
                return;

            var fms = fd.FileNames;
            ITabViewModel lastTab = null;
            for (int i = 0, count = fms.Length; i < count; i++) {
                ITabViewModel tab = null;
                switch (GetFileTypeByPath(fms[i])) {
                    case FileTypes.Image:
                        tab = new TabImageViewModel();
                        break;
                    case FileTypes.Text:
                        tab = new TabTextViewModel();
                        break;
                }

                if (tab != null) {
                    tab.Path = fms[i];
                    tab.IsSync = App.Setting.IsSync;
                    TabControl.Tabs.Add(tab);
                    lastTab = tab;
                }
            }
            foreach (var tab in TabControl.Tabs.Where(tab => !tab.IsRun)) {
                tab.Start();
                if (tab.Equals(lastTab)) {
                    TabControl.SelectedItem = lastTab;
                }
            }
        }
        private void SaveFile(bool isSaveAs=false) {
            var ind = TabControl.SelectedIndex;

            if (ind < 0 && ind > TabControl.Tabs.Count - 1)
                return;

            var tab = TabControl.Tabs[ind];
            string path = null;

            if (isSaveAs) {
                var sfd = new SaveFileDialog() {
                    FileName = tab.Path
                };
                if (sfd.ShowDialog() == null)
                    return;
                path = sfd.FileName;
                if (path == "")
                    return;
            }

            tab.Save(path);
        }
        private FileTypes GetFileTypeByPath(string path) {
            var e = System.IO.Path.GetExtension(path);
            return imgExts.Contains(e) ? FileTypes.Image : textExts.Contains(e) ? FileTypes.Text : FileTypes.Other;
        }

        
        private void MenuItemClick(object sender, RoutedEventArgs e) {
            var item = sender as MenuItem;
            if (item == null)
                return;

            var tabInd = TabControl.SelectedIndex;
            switch (item.Name) {
                case "ItemOpenFile":
                    OpenFiles();
                    break;
                case "ItemOpenFiles":
                    OpenFiles(true);
                    break;
                case "ItemSave":
                    if (tabInd < 0)
                        return;
                    SaveFile();
                    break;
                case "ItemSaveAs":
                    if (tabInd < 0)
                        return;
                    SaveFile(true);
                    break;
                case "ItemSettings":
                    OpenSetting();
                    break;
                case "ItemAbout":
                    Debug.Print(Thread.CurrentThread.Name + " cur thr");
                    break;
            }
        }
        private void OnClosing(object sender, CancelEventArgs e) {
            if (TabControl.Tabs.Count(t => !t.IsSave) == 0)
                    return;

            if (MessageBox.Show("Don't save tabs are open. Is close Application?", "Warning", 
                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) {
                    e.Cancel = true;
            }
        }
        private void OnSourceInitialized(object sender, EventArgs e) {
            AddLocalHook(this, WndProc);
        }

        public void OpenSetting() {
            _svw = new SettingsViewModel {
                Model = App.Setting
            };

            var settWnd = new SettingsWindow {
                DataContext = _svw
            };
            settWnd.SourceInitialized += (a,b) => {
                AddLocalHook(settWnd, WndProc);
            };
            settWnd.SettingsChanged += (s, e) => {
                SyncOnOff(App.Setting.IsSync);
                App.UpdateGlobalSettings();
            };
            settWnd.ShowDialog();
        }

        public void AddLocalHook(Visual visual, HwndSourceHook hook) {
            HwndSource source = PresentationSource.FromVisual(visual) as HwndSource;
            if (source != null) 
                source.AddHook(hook);
        }
        public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            //var subCode = (WindowMessageParameter)wParam.ToInt32();
            var wndMsg = (WinAPIHelper.WndMsgs) msg;
            switch (wndMsg) {
                case WinAPIHelper.WndMsgs.WM_COPYDATA:
                    WndMessageCopyData(lParam);
                    break;
            }
            return IntPtr.Zero;
        }
        private void WndMessageCopyData(IntPtr lParam) {
            var data = (WinAPIHelper.COPYDATASTRUCT)Marshal.PtrToStructure(lParam, typeof(WinAPIHelper.COPYDATASTRUCT));
            var text = Marshal.PtrToStringAnsi(data.LpData);
            if (text == null)
                return;
            switch ((SendCommands)Enum.Parse(typeof(SendCommands), text)) {  
                case SendCommands.ReleaseSemaphore:
                    App.ReleaseSemaphore();
                    break;
                case SendCommands.TakeSemaphore:
                    App.TakeSemaphore();
                    break;
                case SendCommands.UpdateSettings:
                    if(_svw != null)
                        _svw.Update();
                    else
                        App.Setting.Load();

                    // test set isSync
                    SyncOnOff(App.Setting.IsSync);
                    // -----------
                    break;
            }
        }

        public void SyncOnOff(bool flag) {
            foreach (var tab in TabControl.Tabs)
                tab.IsSync = flag;
        }


        // hot keys hook
        /// <summary>
        /// priority:
        /// 1 - ctrl
        /// 2 - shift
        /// 3 - alt
        /// 4 - other vk;
        /// separator: +
        /// </summary>
        /// <param name="hotKeys"></param>
        public void AddHotKeys(string hotKeys) {
            _keyboardHook.AddHotKey(hotKeys);
        } 
        private void OnHotKeys(object sender, KeysEventArgs e) {
            var hotKeys = "";
            if (e.Ctrl)
                hotKeys += "Ctrl";
            if (e.Shift)
                hotKeys += "Shift";
            if (e.Alt)
                hotKeys += "Alt";

            hotKeys = e.Keys.Aggregate(hotKeys, (current, key) => current + ("+" + key));
            Debug.Print((e.IsDown ? "down: " : e.IsUp ? "up: " : "") + hotKeys);

            switch (hotKeys) {
                case "Ctrl+S": // save file
                    SaveFile();
                    break;
                case "Ctrl+O": // open file
                    OpenFiles();
                    break;
                case "Ctrl+Alt+S": // save file as
                    SaveFile(true);
                    break;

                case "Alt+A+Z":
                    var callNext = _keyboardHook.IsCallNextHookIfHotKey;
                    _keyboardHook.IsCallNextHookIfHotKey = !callNext;

                    callNext = _keyboardHook.IsCallNextHook;
                    _keyboardHook.IsCallNextHook = !callNext;
                    break;
                case "Ctrl+Alt+Delete":
                    break;
            }
        }

    }
}
