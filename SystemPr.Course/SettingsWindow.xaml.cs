using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using SystemPr.Course.Code;
using SystemPr.Course.ViewModels;

namespace SystemPr.Course {
    public partial class SettingsWindow {
        public event EventHandler SettingsChanged;
        protected virtual void OnSettingsChanged() {
            var handler = SettingsChanged;
            if (handler != null) 
                handler(this, EventArgs.Empty);
            Debug.Print(handler + "");
        }

        public SettingsWindow(){
            InitializeComponent();
            Title += " id=" + AppHelper.ProcessId;
        }

        private void ButtonClick(object sender, RoutedEventArgs e) {
            var btn = sender as Button;
            if (btn == null)
                return;
            switch (btn.Name) {
                case "SaveBtn":
                    SaveSettings();
                    break;
                case "OkBtn":
                    SaveSettings();
                    Exit(true);
                    break;
                case "CancelBtn":
                    Exit(false);
                    break;
            }
        }
        private void SaveSettings() {
            var svm = DataContext as SettingsViewModel;
            if (svm != null && svm.Save()) {
                OnSettingsChanged();
            }
        }
        private void Exit(bool dialogRes) {
            DialogResult = dialogRes;
            Close();
        }
    }
}
