using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SystemPr.Course.ViewModels;

namespace SystemPr.Course.Controls {
    /// <summary>
    /// Interaction logic for ImageTabContent.xaml
    /// </summary>
    public partial class ImageTabContent : UserControl {
        //private TabImageViewModel data;
        //private double originWidth;
        //private double originHeight;
        //private bool isRotate;

        public ImageTabContent() {
            InitializeComponent();

            //DependencyPropertyDescriptor.FromProperty(Image.SourceProperty, typeof(Image))
            //    .AddValueChanged(Image, ImageSourceUpdated);
        }

        private void ButtonClick(object sender, RoutedEventArgs e) {
            var data = this.DataContext as TabImageViewModel;
            var btn = sender as Button;
            switch (btn.Name) {
                case "RotatePlus90Btn":
                    data.Transform.Rotate(true);
                    //data.Transform.Angle += 90;
                    break;
                case "RotateMinus90Btn":
                    data.Transform.Rotate(false);
                    //data.Transform.Angle -= 90;
                    break;
                case "MirrorXBtn":
                    data.Transform.Mirror("x");
                    //MirrorImage(isRotate ? "y" : "x");
                    break;
                case "MirrorYBtn":
                    data.Transform.Mirror("y");
                    //MirrorImage(isRotate ? "x" : "y");
                    break;


                case "infobtn":
                    
                    break;

                case "NewSizeBtn":
                    uint w, h;
                    //double scaleX, scaleY;
                    UInt32.TryParse(NewWidth.Text, out w);
                    UInt32.TryParse(NewHeight.Text, out h);

                    data.Transform.SetSize(w, h);

                    NewWidth.Text = NewHeight.Text = "";

                    //if (isRotate) {
                    //    uint t = w; w = h; h = t;
                    //}

                    //if (w > 0 && h > 0) {
                    //    scaleX = w/originWidth;
                    //    scaleY = h/originHeight;

                    //    data.Transform.ScaleY = data.Transform.ScaleY < 0 ? scaleY / -1 : scaleY;
                    //    data.Transform.ScaleX = data.Transform.ScaleX < 0 ? scaleX / -1 : scaleX;
                    //}
                    break;
                case "BackSizeBtn":
                    data.Transform.SetSize();
                    //data.Transform.ScaleY = data.Transform.ScaleY < 0 ? -1 : 1;
                    //data.Transform.ScaleX = data.Transform.ScaleX < 0 ? -1 : 1;

                    //Image.Width = originWidth;
                    //Image.Height = originHeight;
                    break;


            }

            //CheckRotate();
            //ShowResultSize();
        }
        //private void ImageMouseWheel(object sender, MouseWheelEventArgs e) {
        //    var inc = e.Delta < 0 ? (-.1) : .1;
        //    var y = data.Transform.ScaleY;
        //    var x = data.Transform.ScaleX;

        //    if ((x > -0.2 && x < 0.2) || (y > -0.2 && y < 0.2))
        //        return;

        //    data.Transform.ScaleY = y < 0 ? y - inc : y + inc;
        //    data.Transform.ScaleX = x < 0 ? x - inc : x + inc;

        //    //ShowResultSize();
        //}
        //private void MirrorImage(string coo) {
        //    switch (coo) {
        //        case "x":
        //            data.Transform.ScaleX /= (-1);
        //            break;
        //        case "y":
        //            data.Transform.ScaleY /= (-1);
        //            break;
        //    }
        //}
        //private void ShowResultSize() {
        //    var w = Math.Round(originWidth * data.Transform.ScaleX);
        //    var h = Math.Round(originHeight * data.Transform.ScaleY);

        //    if (w < 0)
        //        w /= -1;
        //    if (h < 0)
        //        h /= -1;

        //    //
        //    NewWidth.Text = ResWidth.Text = (isRotate ? h : w).ToString();
        //    NewHeight.Text = ResHeight.Text = (isRotate ? w : h).ToString();
        //    //ResScale.Text = "x" + scaleImg.ScaleX;
        //}
        //private void CheckRotate() {
        //    //var a = rotateImg.Angle;
        //    var a = data.Transform.Angle;
        //    isRotate = a == 90 || a == 270 || a == -90 || a == -270;  
        //}


        private void NewSizeTextChanged(object sender, TextChangedEventArgs e) {
            TextBox elemA = sender as TextBox, elemB = null;
            if (LockProp.IsChecked != true || !elemA.IsFocused)
                return;

            var tr = (this.DataContext as TabImageViewModel).Transform;
             

            double originA = 0, originB = 0;
            if (NewWidth.Equals(elemA)) {
                elemB = NewHeight;
                originA = tr.Is90Rotate ? tr.OriginHeight : tr.OriginWidth;
                originB = tr.Is90Rotate ? tr.OriginWidth : tr.OriginHeight;
            }
            else {
                elemB = NewWidth;
                originA = tr.Is90Rotate ? tr.OriginWidth : tr.OriginHeight;
                originB = tr.Is90Rotate ? tr.OriginHeight : tr.OriginWidth;
            }

            CalcNewSize(elemA, elemB, originA, originB);
        }
        private void CalcNewSize(TextBox elemA, TextBox elemB, double originA, double originB) {
            uint val = 0;
            UInt32.TryParse(elemA.Text, out val);

            if (val == 0 || originA == 0 || elemB == null)
                return;

            elemB.Text = Math.Round((originB * (val / originA))).ToString();
        }

        //private void ImageSourceUpdated(object sender, EventArgs e) {
        //    data = this.DataContext as TabImageViewModel;

        //    var bmp = Image.Source as BitmapImage;
        //    if (bmp == null)
        //        return;

        //    originWidth = bmp.PixelWidth;
        //    originHeight = bmp.PixelHeight;

        //    //CheckRotate();
        //    //ShowResultSize();

        //    //------
        //}
    }
}
