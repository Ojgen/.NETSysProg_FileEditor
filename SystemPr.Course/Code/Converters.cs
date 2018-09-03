using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;


namespace SystemPr.Course.Code {
    class RevertValueConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return 0;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }


    public class RevertBooleanConvertor : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return !(bool) value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return !(bool) value;
        }
    }
    class BooleanToVisibility : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var revert = (bool)(parameter ?? false);
            var res = (bool) value;
            return (revert ? !res : res) ? Visibility.Visible : Visibility.Hidden;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var revert = (bool)(parameter ?? false);
            return !revert && (Visibility)value == Visibility.Visible;
        }
    }

    class NumberFromPercentConverter : IValueConverter {
        public double Percent { get; set; }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var n = parameter.ToString();
            var d = double.Parse(n);
            //return ((double)value * (Percent / 100));
            return ((double)value * (double.Parse(parameter.ToString()) / 100));
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return null;
        }
    }



    // -----------------------
    class RadioButtonCheckedConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return value.Equals(parameter);
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return value.Equals(true) ? parameter : Binding.DoNothing;
        }
    }
    class StringToResourseConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var obj = parameter as FrameworkElement;
            if(obj != null)
                return obj.TryFindResource(value);
            return Application.Current.Resources[value];
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var obj = parameter as FrameworkElement;

            if(obj != null) {
                do {
                    var res = GetKeyByValue(value, obj.Resources);
                    if (res != null)
                        return res;
                    obj = obj.Parent as FrameworkElement;
                } while (obj != null);
            }

            return GetKeyByValue(value, Application.Current.Resources);
        }

        private object GetKeyByValue(object value, ResourceDictionary rd) {
            int i = 0;
            foreach (var val in rd.Values) {
                if (val.Equals(value))
                    return rd.Keys.Cast<object>().ToArray()[i];
                i++;
            }

            //foreach (var md in rd.MergedDictionaries) {
            //    var res = GetKeyByValue(value, md);
            //    if (res != null)
            //        return res;
            //}
            //return null;

            return rd.MergedDictionaries.Select(md => GetKeyByValue(value, md)).FirstOrDefault(res => res != null);
        }
    }

    class BoolToObjectConverter : IValueConverter {
        public object TrueObject { get; set; }
        public object FalseObject { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return (bool) value ? TrueObject : FalseObject;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return TrueObject.Equals(value);
        }
    }

}
