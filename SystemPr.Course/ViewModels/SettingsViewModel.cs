using System.Windows;
using SystemPr.Course.Code;
using SystemPr.Course.Models;

namespace SystemPr.Course.ViewModels {
    public class SettingsViewModel : BaseViewModel {
        private uint _res;

        private CountLautches _countLautch;
        private uint _amountLautch;
        private bool _isSync;
        private SettingModel _model;

        public CountLautches CountLautch {
            get { return _countLautch; }
            set {
                _countLautch = value;
                OnPropertyChanged("CountLautch");
            }
        }
        public uint AmountLautch {
            get { return _amountLautch; }
            set {
                _amountLautch = value;
                if (_amountLautch < 2)
                    _amountLautch = 2;
                OnPropertyChanged("AmountLautch");
            }
        }
        public bool IsSync {
            get { return _isSync; }
            set {
                _isSync = value;
                OnPropertyChanged("IsSync");
            }
        }
        public SettingModel Model {
            get { return _model; }
            set {
                _model = value;
                OnPropertyChanged("Model");
                InitViewModelByModel();
            }
        }

        private void InitViewModelByModel() {
            AmountLautch = 2;
            switch (_model.CopyCount) {
                case 0:
                    CountLautch = CountLautches.NoLimit;
                    break;
                case 1:
                    CountLautch = CountLautches.Single;
                    break;
                default:
                    if (_model.CopyCount > 0) {
                        AmountLautch = _model.CopyCount;
                        CountLautch = CountLautches.Limit;
                    }
                    break;
            }
            IsSync = _model.IsSync;
        }

        public bool Save() {
            if (!CheckChange())
                return false;

            _model.CopyCount = _res;
            _model.IsSync = _isSync;
            _model.Save();
            return true;
        }
        public bool CheckChange() {
            _res = 0;
            switch (_countLautch) {
                case CountLautches.Single:
                    _res = 1;
                    break;
                case CountLautches.Limit:
                    _res = _amountLautch;
                    break;
                case CountLautches.NoLimit:
                    _res = 0;
                    break;
            }
            if (_model.CopyCount != _res) {
                var countCopy = AppHelper.Count;
                if (_res > 0 && _res < countCopy) {
                    MessageBox.Show("You don't set Count Lautch app into " + _res
                         + " because lautched " + countCopy + " copyes");
                    return false;
                }
                return true;
            }
   
            return _model.IsSync != _isSync;
        }
        public void Update() {
            _model.Load();
            InitViewModelByModel();
        }
    }
}
