using System.IO;
using System.Threading;

namespace SystemPr.Course.Models {
    public class SettingModel {
        private readonly Mutex _mtx;
        public readonly string Path;

        public uint CopyCount { get; set; }
        public bool IsSync { get; set; }

        public SettingModel(string path) {
            const string mtxName = "setting_mutex:c79ef123-5330-42b0-b437-d7701ea089cf";
            _mtx = new Mutex(false, mtxName);
            Path = path;
        }

        public void Load() {
            bool isExist = false;
            try {
                _mtx.WaitOne();
                isExist = File.Exists(Path);
                if (isExist) {
                    using (var fs = new FileStream(Path, FileMode.Open)) {
                        using (var br = new BinaryReader(fs)) {
                            //MessageBox.Show("read!");
                            ReadM(br);
                        }
                    }
                }
            }
            catch {
                isExist = false;
            }
            finally {
                _mtx.ReleaseMutex();
                if (!isExist) {
                    CopyCount = 0;
                    IsSync = false;
                    Save();
                }
            }
        }
        public void Save() {
            try {
                _mtx.WaitOne();
                using (var fs = new FileStream(Path, FileMode.OpenOrCreate)) {
                    using (var bw = new BinaryWriter(fs)) {
                        WriteM(bw);
                    }
                }
            }
            catch {
                // it's bad
            }
            finally {
                _mtx.ReleaseMutex();
            }
        }
        public void SaveAndRelease() {
            Save();
            _mtx.Close();
        } 

        private void WriteM(BinaryWriter bw) {
            bw.Write(CopyCount);
            bw.Write(IsSync);
        }
        private void ReadM(BinaryReader br) {
            CopyCount = br.ReadUInt32();
            IsSync = br.ReadBoolean();
        }     
    }
}
