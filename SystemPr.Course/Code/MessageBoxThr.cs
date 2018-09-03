using System.Windows;

namespace SystemPr.Course.Code {
    public class MessageBoxThr {
        private static MessageBoxResult _msgRes;
        public static MessageBoxResult Show(string msg, string caption, MessageBoxButton mbb, MessageBoxImage mbi) {
            Application.Current.Dispatcher.Invoke(() => {
                _msgRes = MessageBox.Show(Application.Current.MainWindow, msg, caption, mbb, mbi);
            });
            return _msgRes;
        }
    }
}
