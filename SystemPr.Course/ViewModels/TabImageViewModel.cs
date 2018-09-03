using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SystemPr.Course.ViewModels {
    public class TabImageViewModel : TabViewModel {
        private ImageTransformViewModel _transform;
        private bool? _isShowRealSize;
        private bool? _lockProportion;

        public ImageTransformViewModel Transform {
            get {return _transform;}
            set {
                _transform = value;
                OnPropertyChanged("Transform");
            }
        }      
        public bool? IsShowRealSize {
            get { return _isShowRealSize; }
            set {
                _isShowRealSize = value;
                OnPropertyChanged("IsShowRealSize");
            }
        }       
        public bool? LockProportion {
            get { return _lockProportion; }
            set {
                _lockProportion = value;
                OnPropertyChanged("LockProportion");
            }
        }
        
        public TabImageViewModel() : base() {
            DataTemplate = "LoadContent";
            IsShowRealSize = false;
            LockProportion = true;

            TransformInit();
        }
        private void TransformInit() {
            Transform = new ImageTransformViewModel();
            TransformDefault();
            Transform.PropertyChanged += (s, e) => {
                CheckIsSaveTask();
            };
        }
        private void TransformDefault() {
            Transform.Angle = 0;
            Transform.ScaleX = 1;
            Transform.ScaleY = 1;
        }


        // tasks 
        protected override void LoadContentTask() {
            base.LoadContentTask();

            byte[] buffer = File.ReadAllBytes(Path);
            var memoryStream = new MemoryStream(buffer);
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.StreamSource = memoryStream;
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.EndInit();
            bmp.Freeze();

            // default transform
            TransformDefault();
            Content = bmp;
            Transform.SetBitmap(bmp);
            //Transform.OriginWidth = bmp.PixelWidth;
            //Transform.OriginHeight = bmp.PixelHeight;
            //Transform.Width = bmp.PixelWidth;
            //Transform.Height = bmp.PixelHeight;
            // ---
            if (IsFirstOpen)
                DataTemplate = "ImageContent";
        }
        protected override void CheckIsSaveTask() {
            base.CheckIsSaveTask();
            if (!IsSave)
                return;
            IsSave = !_transform.IsChange;
        }
        protected override void SaveContentTask(string path=null) {
            //AddTask(() => { // change?
                base.SaveContentTask(path);

                try {
                    //SaveImage(GetBitmapSource(), Path);
                    SaveImage(Transform.GeTransformedBitmap(Content as BitmapSource), Path);
                }
                catch (InvalidOperationException) {
                    MessageBox.Show("Image don't transform");
                }
                catch (Exception e) {
                    MessageBox.Show(e.Message);
                }

                LoadContentTask();
            //});
        }

        private BitmapEncoder GetBmpEncoder(string format) {
            switch (format) {
                case ".jpg":
                case ".jpeg":
                    return new JpegBitmapEncoder();
                case ".png":
                    return new PngBitmapEncoder();
                case ".gif":
                    return new GifBitmapEncoder();
                case ".tiff":
                    return new TiffBitmapEncoder();
                default:
                    return new BmpBitmapEncoder();
            } 
        }
        private void SaveImage(TransformedBitmap tbmp, string path) {
            var ext = System.IO.Path.GetExtension(path);

            BitmapEncoder enc = GetBmpEncoder(ext);
            enc.Frames.Add(BitmapFrame.Create(tbmp));
            using (var streem = new FileStream(path, FileMode.Create)) {
                enc.Save(streem);
            }
        }
    }
}
