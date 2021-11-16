using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MusicBox.Models;
using MusicBox.MVVModels;
using NAudio.MediaFoundation;
using Prism.Mvvm;

namespace MusicBox.ViewModels
{
    public class MainWindowVm :BindableBase
    {
        private MainWindowModel _model = new MainWindowModel();
        public Action<object, System.Windows.Input.KeyEventArgs> KeyDown => _model.KeyDown;
        public Action Close
        {
            get => _model.Close;
            set => _model.Close = value;
        }
        public void Closing(object sender, EventArgs e)
        {
            _model.Closing();
        }
        public ImageSource Result=>_model.Result;
        public ImageSource Square => _model.Square;
        public ImageSource BassPlaying => _model.BassPlaying;
        public ImageSource DrumsPlaying => _model.DrumsPlaying;
        public ImageSource MelodyPlaying => _model.MelodyPlaying;
        public ImageSource VocalPlaying => _model.VocalPlaying;
        public string BassPlayingString => _model.BassPlayingString;
        public string DrumsPlayingString => _model.DrumsPlayingString;
        public string MelodyPlayingString => _model.MelodyPlayingString;
        public string VocalPlayingString => _model.VocalPlayingString;
        public string BassEffectString => _model.BassEffectString;
        public string DrumsEffectString => _model.DrumsEffectString;
        public string MelodyEffectString => _model.MelodyEffectString;
        public string VocalEffectString => _model.VocalEffectString;
        public int BassVolume
        {
            get => _model.BassVolume;
            set => _model.BassVolume = value;
        }
        public int DrumsVolume
        {
            get => _model.DrumsVolume;
            set => _model.DrumsVolume = value;
        }
        public int MelodyVolume
        {
            get => _model.MelodyVolume;
            set => _model.MelodyVolume = value;
        }
        public int VocalVolume
        {
            get => _model.VocalVolume;
            set => _model.VocalVolume = value;
        }
        public MainWindowVm()
        {
            _model.PropertyChanged += (s, e) => RaisePropertyChanged(e.PropertyName);
        }
    }
}
