using System;
using System.Collections.Generic;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Media;
using System.Windows.Threading;
using AForge.Video;
using AForge.Video.DirectShow;
using MusicBox.Models;
using MusicBox.Properties;
using MusicBox.ViewModels;
using Newtonsoft.Json;
using Color = System.Drawing.Color;

namespace MusicBox.MVVModels
{
    public class CameraSettingsModel:BindableBase
    {
        private readonly Dispatcher _disp;
        private VideoCaptureDevice _capture;
        private ImageSource _result;
        public static ObservableCollection<string> ListEffects = new ObservableCollection<string>(){"Echo effect","High-pass filter", "Low-pass filter"};
        public EffectSettingVm TopEffect { get; set; }
        public EffectSettingVm LeftEffect { get; set; } 
        public EffectSettingVm RightEffect { get; set; } 
        public EffectSettingVm BottomEffect { get; set; }
        public VideoCaptureDevice Capture
        {
            get => _capture;
            set
            {
                if (_capture!=null)
                    _capture.NewFrame -= FrameArrived;
                _capture = value;
                _capture.NewFrame += FrameArrived;
            }
        }
        private void FrameArrived(object sender, NewFrameEventArgs e)
        {
            try
            {
                var graph = Graphics.FromImage(e.Frame);
                graph.DrawRectangle(new System.Drawing.Pen(Color.Red,5), LeftIndent,TopIndent, Width,Height);
                graph.DrawRectangle(new System.Drawing.Pen(Color.Green, 5), LeftIndent+Width/2- WidthCenter/2, TopIndent +Height/2 - HeightCenter/2, WidthCenter, HeightCenter);
                graph.Save();
                graph.Dispose();
                _disp.Invoke(delegate { Result = Functions.BitmapToImageSource(e.Frame, true); });
            }
            catch
            {
                Capture.SignalToStop();
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
        private VideoDevice _selectedCamera;
        public VideoDevice SelectedCamera
        {
            get => _selectedCamera;
            set
            {
                _selectedCamera = value;
                RaisePropertyChanged();
                Capture = new VideoCaptureDevice(_selectedCamera.Moniker);
                foreach (var capability in Capture.VideoCapabilities)
                {
                    VideoSettings.Add(capability);   
                }
                RaisePropertyChanged($"VideoSettings");
                if (Capture.IsRunning)
                    Capture.Stop();
                Settings.Default.SelectedCamera = value.Moniker;
                Settings.Default.Save();
            }
        }
        private ObservableCollection<VideoDevice> _videoDevices = new ObservableCollection<VideoDevice>();
        public ObservableCollection<VideoDevice> VideoDevices
        {
            get => _videoDevices;
            set
            {
                _videoDevices = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<VideoCapabilities> _videoSettings = new ObservableCollection<VideoCapabilities>();
        public ObservableCollection<VideoCapabilities> VideoSettings
        {
            get => _videoSettings;
            set
            {
                _videoSettings = value;
                RaisePropertyChanged();
            }
        }
        private VideoCapabilities _selectedSettings;
        public VideoCapabilities SelectedSettings
        {
            get => _selectedSettings;
            set
            {
                _selectedSettings = value;
                RaisePropertyChanged();
                Capture.VideoResolution = _selectedSettings;
                if (!Capture.IsRunning)
                    Capture.Start();
                else
                {
                    Capture.SignalToStop();
                    new Thread(() =>
                    {
                        Capture.WaitForStop();
                        Capture.Start();
                    }).Start();
                }
                Settings.Default.SelectedQuality = Functions.ParseCapabilityToString(value);
                Settings.Default.Save();
            }
        }

        public int LeftIndent
        {
            get => Settings.Default.LeftIndentCrop;
            set
            {
                Settings.Default.LeftIndentCrop = value;
                Settings.Default.Save();
                RaisePropertyChanged();
            }
        }
        public int TopIndent
        {
            get => Settings.Default.TopIndentCrop;
            set
            {
                Settings.Default.TopIndentCrop = value;
                Settings.Default.Save();
                RaisePropertyChanged();
            }
        }
        public int Width
        {
            get => Settings.Default.WidthCrop;
            set
            {
                Settings.Default.WidthCrop = value;
                Settings.Default.Save();
                RaisePropertyChanged();
            }
        }

        public int Height
        {
            get => Settings.Default.HeightCrop;
            set
            {
                Settings.Default.HeightCrop = value;
                Settings.Default.Save();
                RaisePropertyChanged();
            }
        }

        public int HeightCenter
        {
            get => Settings.Default.HeightCenter;
            set
            {
                Settings.Default.HeightCenter = value;
                Settings.Default.Save();
                RaisePropertyChanged();
            }
        }

        public int WidthCenter
        {
            get => Settings.Default.WidthCenter;
            set
            {
                Settings.Default.WidthCenter = value;
                Settings.Default.Save();
                RaisePropertyChanged();
            }
        }

        public int ErrorFrames
        {
            get => Settings.Default.ErrorFrames;
            set
            {
                Settings.Default.ErrorFrames = value;
                Settings.Default.Save();
                RaisePropertyChanged();
            }
        }
        public bool StartSamplesOver
        {
            get => Settings.Default.StartSamplesOver;
            set
            {
                Settings.Default.StartSamplesOver = value;
                Settings.Default.Save();
                RaisePropertyChanged();
            }
        }
        public void Start()
        {
            // ReSharper disable once CollectionNeverUpdated.Local
            var devices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            for (var i = 0; i < devices.Count; i++)
            {
                VideoDevices.Add(new VideoDevice(){Moniker = devices[i].MonikerString, PublicName = devices[i].Name});
            }

            if (!string.IsNullOrEmpty(Settings.Default.SelectedCamera))
            {
                try
                {
                    SelectedCamera = VideoDevices.FirstOrDefault(dvc => dvc.Moniker == Settings.Default.SelectedCamera);
                    if (!string.IsNullOrEmpty(Settings.Default.SelectedQuality))
                    {
                        SelectedSettings = VideoSettings.FirstOrDefault(stg =>
                            Functions.ParseCapabilityToString(stg) == Settings.Default.SelectedQuality);
                    }
                }
                catch
                {
                    //ignored
                }
            }
        }
        public CameraSettingsModel()
        {
            _disp = Dispatcher.CurrentDispatcher;
            //EffectSettingVm.SettingChanged += SaveEffectSettings;
            LoadEffects();
            SettingsTabVm.CloseSettings += Closing;
            Start();
        }

        public void DefaultEffects()
        {
            TopEffect = new EffectSettingVm(ListEffects, "High-pass filter", 1000,15000,1, -1, "Top");
            BottomEffect = new EffectSettingVm(ListEffects, "Low-pass filter", 1000,10000, 1, -1, "Bottom");
            LeftEffect = new EffectSettingVm(ListEffects, "Echo effect", 1000, 10000, 0.5f, 0.5f, "Left");
            RightEffect = new EffectSettingVm(ListEffects, "Echo effect", 10000, 10000, 0.3f,0.9f, "Right");
        }
        public void LoadEffects()
        {
            try
            { 
                var jsonString = File.ReadAllText("EffectsSettings.json");
                var listEffects = JsonConvert.DeserializeObject<List<EffectSettingJson>>(jsonString);
                var topEffect = listEffects.FindLast(side => side.SideEffect == "Top");
                var leftEffect = listEffects.FindLast(side => side.SideEffect == "Left");
                var bottomEffect = listEffects.FindLast(side => side.SideEffect == "Bottom");
                var rightEffect = listEffects.FindLast(side => side.SideEffect == "Right");
                TopEffect = EffectSettingVm.ConvertFromJson(topEffect);
                LeftEffect = EffectSettingVm.ConvertFromJson(leftEffect);
                RightEffect = EffectSettingVm.ConvertFromJson(rightEffect);
                BottomEffect = EffectSettingVm.ConvertFromJson(bottomEffect);
            }
            catch
            {
                DefaultEffects();
            }
        }
        public void SaveEffectSettings()
        {
            var listEffect = new List<EffectSettingJson>
            {
                TopEffect.ConvertToJson("Top"),
                LeftEffect.ConvertToJson("Left"),
                RightEffect.ConvertToJson("Right"),
                BottomEffect.ConvertToJson("Bottom")
            };
            File.WriteAllText("EffectsSettings.json",JsonConvert.SerializeObject(listEffect));
        }
        private void Closing(object sender, string e)
        {
            SaveEffectSettings();
            if (_capture != null) {
                _capture.NewFrame -= FrameArrived;
                _capture.SignalToStop();
            }
        }
    }

    public struct VideoDevice
    {
        public string Moniker { get; set; }
        public string PublicName { get; set; }
    }
}
