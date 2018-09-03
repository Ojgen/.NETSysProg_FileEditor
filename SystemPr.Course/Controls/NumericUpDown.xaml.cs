using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SystemPr.Course.Controls {
    public partial class NumericUpDown : UserControl {
        public static readonly DependencyProperty ValueProperty = DependencyProperty
            .Register("Value", typeof(int?), typeof(NumericUpDown), new PropertyMetadata(1, null));
        public int? Value {
            get {
                return (int)GetValue(ValueProperty);
            }
            set {
                SetValue(ValueProperty, value > MaxValue ? MaxValue : value < MinValue ? MinValue : value);
                UpdateValue();
            }
        }
        public int? Iterator { get; set; }
        public int? MinValue { get; set; }
        public int? MaxValue { get; set; }


        public NumericUpDown() {
            InitializeComponent();
        }  
        private void UpdateValue() {
            value.Text = Value.ToString();
            IsValueChangedHandler();
        }
        private void ButtonClick(object sender, RoutedEventArgs e) {
            UpDown((sender as Button).Name);
        }
        private void ControlMouseWheel(object sender, MouseWheelEventArgs e) {
            UpDown(e.Delta > 0 ? "up" : "down");
        }
        private void ControlLoaded(object sender, RoutedEventArgs e) {
            if (Value == null) Value = 1;
            if (Iterator == null) Iterator = 1;
            if (MinValue == null) MinValue = 1;
            if (MaxValue == null) MaxValue = 100;
            value.Text = Value.ToString();
        }
        private void UpDown(string operation) {
            if (operation == "up")
                Value = Value + Iterator <= MaxValue ? Value + Iterator : MaxValue;
            else
                Value = Value - Iterator >= MinValue ? Value - Iterator : MinValue;
            UpdateValue();
        }
        
        public event EventHandler ValueChanged;
        private void IsValueChangedHandler() {
            if (ValueChanged != null)
                ValueChanged(this, EventArgs.Empty);
        }
    }
}
