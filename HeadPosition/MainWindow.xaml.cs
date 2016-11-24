using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using SharpSenses;
using SharpSenses.Gestures;
using SharpSenses.RealSense.Capabilities;

namespace HeadPosition {
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private ICamera _camera;
        private Dictionary<Direction, Image> _arrows = new Dictionary<Direction, Image>();

        public MainWindow() {
            InitializeComponent();
            _arrows[Direction.Up] = ArrowUp;
            _arrows[Direction.Down] = ArrowDown;
            _arrows[Direction.Left] = ArrowLeft;
            _arrows[Direction.Right] = ArrowRight;
            Task.Run(() => Start());
        }

        public void Start() {
            _camera = Camera.Create(Capability.FaceTracking, Capability.ImageStreamTracking);
            _camera.Face.Mouth.Moved += MouthOnMoved;
            _camera.ImageStream.NewImageAvailable += ImageStream_NewImageAvailable;
            _camera.Start();
            Debug.WriteLine("Camera Started");
        }

        private void ImageStream_NewImageAvailable(object sender, ImageEventArgs e) {
            var image = new BitmapImage();
            using (var mem = new MemoryStream(e.BitmapImage)) {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();

            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                FaceFinder.Source = image;
            }));
        }

        private void MouthOnMoved(object sender, PositionEventArgs positionEventArgs) {
            var x = positionEventArgs.NewPosition.Image.X;
            var y = positionEventArgs.NewPosition.Image.Y;
            var middleX = 300;
            var middleY = 270;
            var limit = 50;

            ShowArrow(Direction.None);
            if (x > middleX + limit) {
                ShowArrow(Direction.Right);
            }
            if (x < middleX - limit) {
                ShowArrow(Direction.Left);
            }
            if (y > middleY + limit) {
                ShowArrow(Direction.Down);
            }
            if (y < middleX - limit) {
                ShowArrow(Direction.Up);
            }
        }

        private void ShowArrow(Direction direction) {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                _arrows.Values.ToList().ForEach(a => a.Visibility = Visibility.Collapsed);
                if (direction != Direction.None) {
                    _arrows[direction].Visibility = Visibility.Visible;
                }
            }));
        }
    }
}