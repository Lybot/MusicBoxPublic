using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using MusicBox.Models;
using MusicBox.MVVModels;
using Prism.Mvvm;

namespace MusicBox.ViewModels
{
    class CameraSettingsVm:BindableBase
    {
        private readonly CameraSettingsModel _model = new CameraSettingsModel();
        public ImageSource Result => _model.Result;
        public ObservableCollection<string> VideoDevices
        {
            get
            {
                var collection = new ObservableCollection<string>();
                foreach (var device in _model.VideoDevices)
                {
                    collection.Add(device.PublicName);
                }
                return collection;
            }
        }
        public string SelectedCamera
        {
            get => _model.SelectedCamera.PublicName;
            set
            {
                var device= _model.VideoDevices.FirstOrDefault(pbc => pbc.PublicName == value);
                _model.SelectedCamera = device;
            }
        }
        public ObservableCollection<string> VideoSettings
        {
            get
            {
                var collection = new ObservableCollection<string>();
                foreach (var setting in _model.VideoSettings)
                {
                    var data = Functions.ParseCapabilityToString(setting);
                    collection.Add(data);
                }
                return collection;
            }
        }

        public EffectSettingVm TopEffect
        {
            get => _model.TopEffect;
            set => _model.TopEffect= value;
        }
        public EffectSettingVm LeftEffect
        {
            get => _model.LeftEffect;
            set => _model.LeftEffect = value;
        }
        public EffectSettingVm BottomEffect
        {
            get => _model.BottomEffect;
            set => _model.BottomEffect = value;
        }
        public EffectSettingVm RightEffect
        {
            get => _model.RightEffect;
            set => _model.RightEffect = value;
        }
        public string SelectedSettings
        {
            get
            {
                if (_model.SelectedSettings == null)
                    return "";
                return Functions.ParseCapabilityToString(_model.SelectedSettings);
            }
            set
            {
                var capability =
                    _model.VideoSettings.FirstOrDefault(sett => Functions.ParseCapabilityToString(sett) == value);
                _model.SelectedSettings = capability;
            }
        }

        public string LeftIndent
        {
            get => _model.LeftIndent.ToString();
            set
            {
                if (int.TryParse(value, out var result))
                {
                    _model.LeftIndent = result;
                }
            }
        }
        public string TopIndent
        {
            get => _model.TopIndent.ToString();
            set
            {
                if (int.TryParse(value, out var result))
                {
                    _model.TopIndent = result;
                }
            }
        }
        public string Width
        {
            get => _model.Width.ToString();
            set
            {
                if (int.TryParse(value, out var result))
                {
                    _model.Width= result;
                }
            }
        }
        public string Height
        {
            get => _model.Height.ToString();
            set
            {
                if (int.TryParse(value, out var result))
                {
                    _model.Height = result;
                }
            }
        }
        public string WidthCenter
        {
            get => _model.WidthCenter.ToString();
            set
            {
                if (int.TryParse(value, out var result))
                {
                    _model.WidthCenter = result;
                }
            }
        }
        public string HeightCenter
        {
            get => _model.HeightCenter.ToString();
            set
            {
                if (int.TryParse(value, out var result))
                {
                    _model.HeightCenter = result;
                }
            }
        }
        public string ErrorFrames
        {
            get => _model.ErrorFrames.ToString();
            set
            {
                if (int.TryParse(value, out var result))
                {
                    _model.ErrorFrames = result;
                }
            }
        }
        public bool StartSamplesOver
        {
            get => _model.StartSamplesOver;
            set => _model.StartSamplesOver = value;
        }
        public CameraSettingsVm()
        {
            _model.PropertyChanged += (s, e) => RaisePropertyChanged(e.PropertyName);
        }
    }
}
