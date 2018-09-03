using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemPr.Course.ViewModels {
    public interface ITabViewModel {
        string Header { get; set; }
        string Path { get; set; }
        object Content { get; set; }

        bool IsRun { get; }
        bool IsSave { get; set; }
        bool IsSync { get; set; }

        void Start();
        void Save(string path = null);
        void AddTask(TabTask task);
    }
}
