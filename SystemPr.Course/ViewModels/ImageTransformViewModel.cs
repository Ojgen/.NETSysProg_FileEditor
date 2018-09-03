using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SystemPr.Course.ViewModels {
        public class ImageTransformViewModel : BaseViewModel {
        private double _scaleY = 1;
        private double _scaleX = 1;
        private double _angle;
        private bool _is90Rotate;
        private double _originWidth;
        private double _originHeight;
        private double _width;
        private double _height;


        public double ScaleY {
            get { return _scaleY; }
            set {
                _scaleY = value;
                OnPropertyChanged("ScaleY");
            }
        }
        public double ScaleX {
            get { return _scaleX; }
            set {
                _scaleX = value;
                OnPropertyChanged("ScaleX");
            }
        }
        public double Angle {
            get { return _angle; }
            set {
                _angle = value;

                while (_angle >= 360)
                    _angle -= 360;
                while (_angle <= -360)
                    _angle += 360;

                OnPropertyChanged("Angle");
                SetIs90Rotate();
            }
        }
        public bool Is90Rotate {
            get { return _is90Rotate; }
            private set {
                _is90Rotate = value;
                OnPropertyChanged("Is90Rotate");
            }
        }
        public double OriginWidth {
            get { return _originWidth; }
            set {
                _originWidth = value;
                OnPropertyChanged("OriginWidth");
            }
        }
        public double OriginHeight {
            get { return _originHeight; }
            set {
                _originHeight = value;
                OnPropertyChanged("OriginHeight");
            }
        }
        public double Width {
            get { return _width; }
            set {
                _width = value;
                OnPropertyChanged("Width");
            }
        }
        public double Height {
            get { return _height; }
            set {
                _height = value;
                OnPropertyChanged("Height");
            }
        }

        public bool IsChange {
            get {
                var res = true;
                double[,] q = {
                    {0,1,1},
                    {180,-1,-1},
                    {-180,-1,-1}
                };

                for(int i=0, l = 3; i < l; i++) {
                    if (_angle == q[i, 0] && _scaleX == q[i, 1] && _scaleY == q[i, 2])
                        res = false;
                }
                return res;
            }
        }


        public void Rotate(bool flag) {
            Angle += flag ? 90 : (-90);
            CheckSize();
        }
        public void Mirror(string coo) {
            if (_is90Rotate)
                coo = coo == "x" ? "y" : "x";

            switch (coo) {
                case "x":
                    ScaleX /= (-1);
                    break;
                case "y":
                    ScaleY /= (-1);
                    break;
            }
        }
        public void SetSize() {
            ScaleY = _scaleY < 0 ? -1 : 1;
            ScaleX = _scaleX < 0 ? -1 : 1;

            CheckSize();
        }
        public void SetSize(double w, double h) {
            double scaleX, scaleY;
            if (_is90Rotate) {
                double t = w; w = h; h = t;
            }

            if (w > 0 && h > 0) {
                scaleX = w / _originWidth;
                scaleY = h / _originHeight;

                ScaleY = _scaleY < 0 ? scaleY / -1 : scaleY;
                ScaleX = _scaleX < 0 ? scaleX / -1 : scaleX;

                CheckSize();
            }
        }

        public void SetBitmap(BitmapImage bmp) {
            Width = OriginWidth = bmp.PixelWidth;
            Height = OriginHeight = bmp.PixelHeight;
        }
        public TransformedBitmap GeTransformedBitmap(BitmapSource bmp) {
            double x = _scaleX, y = _scaleY;

            var tBmp = new TransformedBitmap();
            tBmp.BeginInit();
            tBmp.Source = bmp;

            var tg = new TransformGroup();
            tg.Children.Add(new RotateTransform(_angle));
            tg.Children.Add(new ScaleTransform(_is90Rotate ? y : x, _is90Rotate ? x : y));

            tBmp.Transform = tg;
            tBmp.EndInit();

            return tBmp;
        }

        private void CheckSize() {
            var w = Math.Round(_originWidth * _scaleX);
            var h = Math.Round(_originHeight * _scaleY);

            if (w < 0)
                w /= -1;
            if (h < 0)
                h /= -1;

            Width = (_is90Rotate ? h : w);
            Height = (_is90Rotate ? w : h);
        }
        private void SetIs90Rotate() {
            Is90Rotate = _angle == 90 || _angle == 270 || _angle == -90 || _angle == -270;
        }
    }
}
