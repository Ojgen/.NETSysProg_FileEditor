using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SystemPr.Course.ViewModels;

namespace SystemPr.Course.Controls {
    public class TabControlEx : TabControl {
        public ICommand CloseTabCommand { get; set; }
        public ICommand ScrollTabsCommand { get; set; }

        public ObservableCollection<ITabViewModel> Tabs { get; set; }

        public TabControlEx() : base() {
            CloseTabCommand = new RoutedCommand("CloseTabCommand", typeof(TabControlEx));
            ScrollTabsCommand = new RoutedCommand("ScrollTabsCommand", typeof(TabControlEx));

            this.CommandBindings.Add(new CommandBinding(CloseTabCommand, CloseTabCommandHandler, CommandsCanExecute));
            this.CommandBindings.Add(new CommandBinding(ScrollTabsCommand, ScrollTabsCommandHandler, CommandsCanExecute));
        }

        private void CommandsCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }
        private void ScrollTabsCommandHandler(object sender, ExecutedRoutedEventArgs e) {
            var flag = (bool)e.Parameter;

            var lastInd = this.Items.Count - 1;
            var ind = this.SelectedIndex;
            ind += flag ? 1 : -1;
            ind = ind < 0 ? 0 : ind > lastInd ? lastInd : ind;



            var t = this.Template.FindName("sw", this) as ScrollViewer;

            if (!flag)
                t.PageLeft();
            else
                t.PageRight();

            //this.SelectedIndex = ind;
        }
        private void CloseTabCommandHandler(object sender, ExecutedRoutedEventArgs e) {
            var tab = e.Parameter as TabItem;
            if (tab == null)
                return;

            if (PreCloseTab(tab))
                CloseTab(tab);
        }
        private bool PreCloseTab(TabItem tab) {
            bool res = true;
            var tvm = tab.Content as TabViewModel;
            if (tvm == null || !Tabs.Contains(tvm))
                return false;


            if (!tvm.IsSave) {
                switch (MessageBox.Show("Is save change before close?", "Changes not save",
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Question)) {
                        case MessageBoxResult.Yes:
                            res = true;
                            // save change
                            break;
                        case MessageBoxResult.No:
                            res = true;
                            break;
                        case MessageBoxResult.Cancel:
                            res = false;
                            break;
                }
            }



            return res;
        }
        private void CloseTab(TabItem tab) {
            var tvm = tab.Content as TabViewModel;
            tvm.Stop();
            Tabs.Remove(tvm);
        }

    }
}
