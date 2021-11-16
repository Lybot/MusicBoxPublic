using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MusicBox.Models;
using MusicBox.MVVModels;
using MusicBox.Properties;
using Prism.Commands;
using Prism.Mvvm;

namespace MusicBox.ViewModels
{
    public class MusicSettingsVm : BindableBase
    {
        private readonly MusicSettingsModel _model = new MusicSettingsModel();
        public ObservableCollection<string> Presets => _model.Presets;
        public ObservableCollection<MusicItem> Basses => _model.Basses;
        public ObservableCollection<MusicItem> Melodies => _model.Melodies;
        public ObservableCollection<MusicItem> Vocals => _model.Vocals;
        public ObservableCollection<MusicItem> Drums => _model.Drums;
        public DelegateCommand AddPreset { get; set; }
        public DelegateCommand SaveChanges {get;set;}
        public string CurrentPreset { 
            get => _model.CurrentPreset;
            set => _model.CurrentPreset = value;
        }
        public MusicSettingsVm()
        {
            _model.PropertyChanged += (s, e) => RaisePropertyChanged(e.PropertyName);
            AddPreset = new DelegateCommand(_model.AddPreset);
            SaveChanges = new DelegateCommand(_model.SaveChanges);
        }
    }

    public class MusicItem : BindableBase
    {
        private int _number;

        public string Number
        {
            get => _number.ToString();
            set
            {
                if (int.TryParse(value, out var result))
                {
                    if (result > 0 && result < 11)
                    {
                        _number = result;
                        RaisePropertyChanged();
                        RaisePropertyChanged($"PublicNumber");
                    }
                }
            }
        }

        public string PublicNumber => (_number + 1).ToString();
        private string _name;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                RaisePropertyChanged();
                RaisePropertyChanged($"PublicName");
            }
        }

        public string PublicName
        {
            get => _name.Substring(2, _name.Length-6);
            set
            {
                _name = Number + " " + value;
                RaisePropertyChanged();
            }
        }

        private ImageSource _playStopImage;

        public ImageSource PlayStopImage
        {
            get => _playStopImage;
            set
            {
                _playStopImage = value;
                RaisePropertyChanged();
            }
        } /* = Functions.BitmapToImageSource(Resources.PlayImage, false);*/

        public ImageSource UpImage { get; set; }
        public ImageSource DownImage { get; set; }
        private bool _isPlay;
        public bool IsPlay
        {
            get => _isPlay;
            set
            {
                _isPlay = value;
                RaisePropertyChanged();
            }
        }
        public DelegateCommand UpNumber { get; set; }
        public DelegateCommand DownNumber { get; set; }
        public DelegateCommand PlayPause { get; set; }
        public DelegateCommand UpdatePath { get; set; }
    }
}
