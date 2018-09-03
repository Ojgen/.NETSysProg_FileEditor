using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using SystemPr.Course.Code;

namespace SystemPr.Course.ViewModels {
    public delegate void TabTask();

    public class TabViewModel : BaseViewModel, ITabViewModel {
        #region ViewModel properties

        private string _header;
        private string _path;
        private object _content;

        private bool _isSave;
        private string _dataTmpl;
        private bool _isSignal;
        private bool _isSelected;
        private bool _isSync;


        public string Header {
            get { return _header; }
            set {
                _header = value;
                OnPropertyChanged("Header");
            }
        }
        public string Path {
            get { return _path; }
            set {
                _path = value;
                OnPropertyChanged("Path");
                //-----
                Header = System.IO.Path.GetFileName(_path);
                if (IsRun) {
                    Open();
                }
            }
        }
        public object Content {
            get { return _content; }
            set {
                _content = value;
                OnPropertyChanged("Content");
                AddTask(CheckIsSaveTask);
            }
        }

        public bool IsSave {
            get { return _isSave; }
            set {
                _isSave = value;
                OnPropertyChanged("IsSave");
            }
        }
        public string DataTemplate {
            get { return _dataTmpl; }
            set {
                _dataTmpl = value;
                OnPropertyChanged("DataTemplate");
            }
        }
        public bool IsSignal {
            get { return _isSignal; }
            set {
                _isSignal = value;
                OnPropertyChanged("IsSignal");
            }
        }      
        public bool IsSelected {
            get { return _isSelected; }
            set {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
                Select();
            }
        }
        public bool IsSync {
            get { return _isSync; }
            set {
                _isSync = value;
                OnPropertyChanged("IsSync");
                SetEnableRaisingEvents();
            }
        }
        #endregion

        // file model
        private readonly FileSystemWatcher _fsw;
        private bool _isOwnerSaved;
        private DateTime _lastRead;
        private DateTime _lastWrite;
        private string _signalType;
        private string _newPath;
        
        // tabmodel 
        private readonly Thread _thr;
        private readonly EventWaitHandle _wh;
        private readonly Queue<TabTask> _tasks;

        public bool IsRun { get; private set; }
        public bool IsFirstOpen { get; private set; }


        public TabViewModel() {
            _fsw = new FileSystemWatcher();
            _fsw.Created += FswOnCreated;
            _fsw.Changed += FswOnChanged;
            _fsw.Deleted += FswOnDeleted;
            _fsw.Renamed += FswOnRenamed;

            // ----------------

            _wh = new AutoResetEvent(false);
            _tasks = new Queue<TabTask>();

            _thr = new Thread(Run) {
                Name = Guid.NewGuid().ToString().Substring(0,4), // delete
                IsBackground = true
            };
            IsRun = false;
            IsSignal = false;
            _lastRead = DateTime.MinValue;
            // ----
        }
        private void SetPathFileWatcher() {
            _fsw.Path = System.IO.Path.GetDirectoryName(Path);
            _fsw.Filter = System.IO.Path.GetFileName(Path);
            SetEnableRaisingEvents();
        }
        private void SetEnableRaisingEvents() {
            try {
                _fsw.EnableRaisingEvents = IsSync;
            }
            catch {
                // ignored
            }
        }

        public void Start() {
            if (IsRun) return;
            _thr.Start();
            IsRun = true;
            //AddTask(LoadContentTask);
            Open();
        }
        public void Stop() {
            if (!IsRun) return;
            _thr.Abort();
            IsRun = false;
        }
        private void Run() {
            while (true) {
                if (_tasks.Count > 0) {
                    var del = _tasks.Dequeue();
                    del.Invoke();
                }
                else {
                    _wh.WaitOne();
                }                   
            }
        }
        public void AddTask(TabTask del) {
            var isSet = _tasks.Count == 0;
            _tasks.Enqueue(del);
            if(isSet)
                _wh.Set();
        }

        public void Open() {
            AddTask(() => {
                IsFirstOpen = Content == null;
                LoadContentTask();
                IsFirstOpen = false;
            });
        }
        public void Save(string path = null) {
            AddTask(() => {
                SaveContentTask(path);
            });
        }
        public void Select() {
            AddTask(SelectTask);
        }

        // tasks
        protected virtual void LoadContentTask() {
            SetPathFileWatcher();
            _lastWrite = File.GetLastWriteTime(Path);
        }
        protected virtual void CheckIsSaveTask() {
            IsSave = File.Exists(Path);
            if(!IsSave) return;

            if(IsSync)
                IsSave = _lastWrite == _lastRead || _lastRead == DateTime.MinValue;
        }
        protected virtual void SaveContentTask(string path=null) {
            if (path != null) {
                Path = path;
                Header = System.IO.Path.GetFileName(path);
            }
            _isOwnerSaved = true;
        }
        protected virtual void SelectTask() {
            if(IsSelected)
                SignalMessage();
        }
        
        // events for file system watcher
        private void FswOnChanged(object sender, FileSystemEventArgs e) {
            AddTask(() => {
                // hack: perform only last event
                DateTime lastWriteTime = File.GetLastWriteTime(Path);
                if (lastWriteTime == _lastRead)
                    return;
                _lastRead = lastWriteTime;
                // ---------------
                SyncContentUpdate("update");
            });
        }
        private void FswOnCreated(object sender, FileSystemEventArgs e) {
            AddTask(() => {
                if (_isOwnerSaved)
                    return;
                SyncContentUpdate("update");
            });
        }
        private void FswOnDeleted(object sender, FileSystemEventArgs e) {
            AddTask(CheckIsSaveTask);
        }
        private void FswOnRenamed(object sender, FileSystemEventArgs e) {
            AddTask(() => {
                _newPath = e.FullPath;
                SyncContentUpdate("rename");
            });
        }
        private void SyncContentUpdate(string signalType) {
            // if a tab is a change owner 
            if (_isOwnerSaved) {
                _isOwnerSaved = false;
                CheckIsSaveTask();
            }
            else { // if a tab detected another's change

                _signalType = signalType; // 11

                if (IsSelected)
                    SignalMessage(true);
                else
                    IsSignal = true;
            }
        }
        private void SignalMessage(bool flag=false) {
            if (IsSignal || flag) {
                switch (_signalType) {
                    case "update":
                        if(MessageBoxThr.Show("Content updated. Is update tab?", "File updated",
                                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                            LoadContentTask();
                        break;
                    case "rename":
                        if (MessageBoxThr.Show("File renamed. Is update tab?", "File renamed",
                                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
                            Path = _newPath;
                        }
                        break;
                }
                CheckIsSaveTask();
                IsSignal = false;
            }
        }

    }
}
