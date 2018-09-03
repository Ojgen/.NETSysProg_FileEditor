using System;
using System.Threading;
using System.Windows;

namespace SystemPr.Course.Code {
    public class SystemSemaphoreHelper {
        private Semaphore _sem;
        public readonly string Name;
        public int Count { get; set; }

        public SystemSemaphoreHelper(string name) {
            _sem = null;
            Name = name;
        }
        public bool Create(uint count) {
            bool isNew;
            _sem = new Semaphore((int) count, (int) count, Name, out isNew);
            if(isNew)
                Count = (int)count;
            return isNew;
        }
        public bool Take() {
            bool res = false;
            if (_sem != null) {
                try {
                    res = _sem.WaitOne(0, false);
                }
                catch (Exception e) {
                    MessageBox.Show(e.Message, "id=" + AppHelper.ProcessId);
                }
            }
            return res;
        }
        public int Release() {
            var res = -1;
            if (_sem == null) 
                return res;
            try {
                res = _sem.Release();
            }
            catch (Exception e) {
                MessageBox.Show(e.Message, "id=" + AppHelper.ProcessId);
            }
            return res;
        }
        public void Close() {
            if (_sem != null) {
                _sem.Dispose();
                _sem.Close();
                _sem = null;
            }
        }
        public bool IsRefExist() {
            return _sem != null;
        }
        public bool IsExist() {
            bool res;
            try {
                var s = Semaphore.OpenExisting(Name);
                res = true;
                //MessageBox.Show(s.ToString(), "id=" + AppHelper.ProcessId);
            }
            catch(Exception e) { 
                res = false;
                //MessageBox.Show(e.Message, "id=" + AppHelper.ProcessId);
            }
            return res;
        }
    }
}
