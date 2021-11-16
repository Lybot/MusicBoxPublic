using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;
using AForge.Video.DirectShow;
using Emgu.CV;
using Emgu.CV.Aruco;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using MusicBox.Models;
using MusicBox.Properties;
using MusicBox.Views;
using Prism.Mvvm;

namespace MusicBox.MVVModels
{
    class MainWindowModel : BindableBase
    {
        readonly VideoCaptureDevice _capture = new VideoCaptureDevice(Settings.Default.SelectedCamera);
        private MusicList MusicList { get; }
        private ImageSource _result;
        private readonly ImageSource _greenLamp = Functions.BitmapToImageSource(Resources.GreenLamp, false);
        private readonly ImageSource _redLamp = Functions.BitmapToImageSource(Resources.RedLamp, false);
        private ImageSource _bassPlaying;
        private ImageSource _drumsPlaying;
        private ImageSource _melodyPlaying;
        private ImageSource _vocalPlaying;
        public Action Close { get; set; }
        private Bitmap _square;
        public ImageSource Square;
        public ImageSource BassPlaying
        {
            get => _bassPlaying;
            set
            {
                _bassPlaying = value;
                RaisePropertyChanged();
            }
        }
        public ImageSource DrumsPlaying
        {
            get => _drumsPlaying;
            set
            {
                _drumsPlaying = value;
                RaisePropertyChanged();
            }
        }
        public ImageSource MelodyPlaying
        {
            get => _melodyPlaying;
            set
            {
                _melodyPlaying = value;
                RaisePropertyChanged();
            }
        }
        public ImageSource VocalPlaying
        {
            get => _vocalPlaying;
            set
            {
                _vocalPlaying = value;
                RaisePropertyChanged();
            }
        }
        public ImageSource Result
        {
            get => _result;
            set
            {
                _result = value;
                RaisePropertyChanged();
            }
        }

        private string _bassPlayingString;
        private string _drumsPlayingString;
        private string _melodyPlayingString;
        private string _vocalPlayingString;
        private string _bassEffectString;
        private string _drumsEffectString;
        private string _melodyEffectString;
        private string _vocalEffectString;
        public string BassPlayingString
        {
            get => _bassPlayingString;
            set
            {
                _bassPlayingString = value;
                RaisePropertyChanged();
            }
        }
        public string DrumsPlayingString
        {
            get => _drumsPlayingString;
            set
            {
                _drumsPlayingString = value;
                RaisePropertyChanged();
            }
        }
        public string MelodyPlayingString
        {
            get => _melodyPlayingString;
            set
            {
                _melodyPlayingString = value;
                RaisePropertyChanged();
            }
        }
        public string VocalPlayingString
        {
            get => _vocalPlayingString;
            set
            {
                _vocalPlayingString = value;
                RaisePropertyChanged();
            }
        }
        public string BassEffectString
        {
            get => _bassEffectString;
            set
            {
                _bassEffectString = value;
                RaisePropertyChanged();
            }
        }
        public string DrumsEffectString
        {
            get => _drumsEffectString;
            set
            {
                _drumsEffectString = value;
                RaisePropertyChanged();
            }
        }
        public string MelodyEffectString
        {
            get => _melodyEffectString;
            set
            {
                _melodyEffectString = value;
                RaisePropertyChanged();
            }
        }
        public string VocalEffectString
        {
            get => _vocalEffectString;
            set
            {
                _vocalEffectString = value;
                RaisePropertyChanged();
            }
        }

        public int BassVolume
        {
            get => MusicList.BassVolume;
            set
            {
                MusicList.BassVolume = value;
                RaisePropertyChanged();
            }
        }
        public int DrumsVolume
        {
            get => MusicList.DrumsVolume;
            set
            {
                MusicList.DrumsVolume = value;
                RaisePropertyChanged();
            }
        }
        public int MelodyVolume
        {
            get => MusicList.MelodyVolume;
            set
            {
                MusicList.MelodyVolume = value;
                RaisePropertyChanged();
            }
        }
        public int VocalVolume
        {
            get => MusicList.VocalVolume;
            set
            {
                MusicList.VocalVolume = value;
                RaisePropertyChanged();
            }
        }
        public bool SetView(string type, bool isPlaying, string composition, string effect)
        {
            switch (type)
            {
                case "bass":
                    if (isPlaying && string.IsNullOrEmpty(BassPlayingString))
                    {
                        BassPlaying = _greenLamp;
                        BassPlayingString = composition;
                        BassEffectString = effect;
                    }

                    if (!isPlaying && !string.IsNullOrEmpty(BassPlayingString))
                    {
                        BassPlaying = _redLamp;
                        BassPlayingString = string.Empty;
                        BassEffectString = string.Empty;
                    }
                    break;
                case "melody":
                    if (isPlaying && string.IsNullOrEmpty(MelodyPlayingString))
                    {
                        MelodyPlaying = _greenLamp;
                        MelodyPlayingString = composition;
                        MelodyEffectString = effect;
                    }

                    if (!isPlaying && !string.IsNullOrEmpty(MelodyPlayingString))
                    {
                        MelodyPlaying = _redLamp;
                        MelodyPlayingString = string.Empty;
                        MelodyEffectString = string.Empty;
                    }
                    break;
                case "drums":
                    if (isPlaying && string.IsNullOrEmpty(DrumsPlayingString))
                    {
                        DrumsPlaying = _greenLamp;
                        DrumsPlayingString = composition;
                        DrumsEffectString = effect;
                    }

                    if (!isPlaying && !string.IsNullOrEmpty(DrumsPlayingString))
                    {
                        DrumsPlaying = _redLamp;
                        DrumsPlayingString = string.Empty;
                        DrumsEffectString = string.Empty;
                    }
                    break;
                case "vocal":
                    if (isPlaying && string.IsNullOrEmpty(VocalPlayingString))
                    {
                        VocalPlaying = _greenLamp;
                        VocalPlayingString = composition;
                        VocalEffectString = effect;
                    }

                    if (!isPlaying && !string.IsNullOrEmpty(VocalPlayingString))
                    {
                        VocalPlaying = _redLamp;
                        VocalPlayingString = string.Empty;
                        VocalEffectString = string.Empty;
                    }
                    break;
            }
            return true;
        }
        private readonly Dispatcher _dispatcher;
        private readonly Dictionary _dictionary = new Dictionary(Dictionary.PredefinedDictionaryName.Dict4X4_100);
        public void KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                new SettingsTab().Show();
                Close();
            }
        }
        public MainWindowModel()
        {
            //for (int i = 0; i < 40; i++)
            //{
            //    var image = new Image<Bgr, byte>(600, 600);
            //    ArucoInvoke.DrawMarker(_dictionary,i,600,image);
            //    image.Save($"Barcodes\\{i}.png");
            //    image.Dispose();
            //}
            MusicList = new MusicList(new Size(Settings.Default.WidthCrop, Settings.Default.HeightCrop), new Size(Settings.Default.WidthCenter, Settings.Default.HeightCenter), SetView);
            BassPlaying = _redLamp;
            DrumsPlaying = _redLamp;
            MelodyPlaying = _redLamp;
            VocalPlaying = _redLamp;
            VideoCapabilities capability = null;
            foreach (var cap in _capture.VideoCapabilities)
            {
                if (Functions.ParseCapabilityToString(cap) == Settings.Default.SelectedQuality)
                    capability = cap;
            }
            try
            {
                _capture.VideoResolution = capability;
                _capture.NewFrame += NewFrameArrived;
                _capture.Start();
            }
            catch
            {
                MessageBox.Show("Can't start camera", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            _square = new Bitmap(Settings.Default.WidthCrop, Settings.Default.HeightCrop);
            var gra = Graphics.FromImage(_square);
            gra.DrawRectangle(Pens.Black, new Rectangle(new Point(_square.Width / 2 - Settings.Default.WidthCenter / 2, _square.Height / 2 - Settings.Default.HeightCenter / 2), new Size(Settings.Default.WidthCenter, Settings.Default.HeightCenter)));
            gra.Save();
            Square = Functions.BitmapToImageSource(_square, true);
            RaisePropertyChanged($"Square");
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        private void NewFrameArrived(object sender, AForge.Video.NewFrameEventArgs e)
        {
            var image = e.Frame.Clone(new Rectangle(Settings.Default.LeftIndentCrop, Settings.Default.TopIndentCrop,
                Settings.Default.WidthCrop, Settings.Default.HeightCrop), e.Frame.PixelFormat);
            VectorOfInt kekw = new VectorOfInt();
            var points = new VectorOfVectorOfPointF();
            var parameter = DetectorParameters.GetDefault();
            parameter.CornerRefinementMethod = DetectorParameters.RefinementMethod.Subpix;
            ArucoInvoke.DetectMarkers(image.ToImage<Bgr, byte>(), _dictionary, points, kekw, parameter);
            var list = new List<ComingItem>();
            for (int i = 0; i < kekw.Size; i++)
            {
                list.Add(new ComingItem(kekw[i], points[i][0], points[i][2]));
            }
            //List<string> listUri = Check(kekw);
            MusicList.Update(list);
            _dispatcher.Invoke(delegate { Result = Functions.BitmapToImageSource(image, true); });
        }

        public void Closing()
        {
            MusicList.Dispose();
            _capture.NewFrame -= NewFrameArrived;
            _capture.SignalToStop();
        }
    }

    public class ComingItem
    {
        public int Number { get; set; }
        public PointF Point { get; set; }

        public ComingItem(int number, PointF firstPointF, PointF secondPointF)
        {
            Number = number;
            Point = Functions.FindCenter(firstPointF, secondPointF);
        }
    }
}
