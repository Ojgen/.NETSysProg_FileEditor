using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SystemPr.Course.ViewModels {
    public class TabTextViewModel : TabViewModel {
        private int _contentHash;
        public TabTextViewModel() {
            DataTemplate = "TextContent";
            _contentHash = 0;
        }

        // tasks
        protected override void LoadContentTask() {
            base.LoadContentTask();
            _contentHash = 0;
            string str;
            using (StreamReader sr = new StreamReader(Path)) {
                str = sr.ReadToEnd();
            }
            Content = str;
        }
        protected override void CheckIsSaveTask() {
            if (_contentHash == 0)
                _contentHash = Content.GetHashCode();

            base.CheckIsSaveTask();
            if (!IsSave) return;
            IsSave = _contentHash.Equals(Content.GetHashCode());
        }
        protected override void SaveContentTask(string path = null) {
            base.SaveContentTask(path);
            try {
                SaveTextFile();
            }
            finally {
                LoadContentTask();
            }
        }

        private void SaveTextFile() {
            var text = Content.ToString();
            File.WriteAllText(Path, text);
        }
    }
}
