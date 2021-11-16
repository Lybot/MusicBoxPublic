using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using MusicBox.MVVModels;
using MusicBox.Properties;
using MusicBox.ViewModels;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace MusicBox.Models
{
    public class MusicList : IDisposable
    {
        private readonly string _musicPath = Environment.CurrentDirectory + "\\Music\\";
        private readonly List<EffectStream> _streams = new List<EffectStream>(40);
        private DirectSoundOut BassSound { get; set; } = new DirectSoundOut();
        private DirectSoundOut DrumsSound { get; set; } = new DirectSoundOut();
        private DirectSoundOut MelodySound { get; set; } = new DirectSoundOut();
        private DirectSoundOut VocalSound { get; set; } = new DirectSoundOut();
        private int _bassPlayingNumber = 55;
        private int _drumsPlayingNumber = 55;
        private int _melodyPlayingNumber = 55;
        private int _vocalPlayingNumber = 55;
        private Size WorkSize { get; }
        private Size CenterSize { get; }
        private EffectSettingVm LeftEffect { get; }
        private EffectSettingVm TopEffect { get; }
        private EffectSettingVm RightEffect { get; }
        private EffectSettingVm BottomEffect { get; }
        private Func<string, bool, string, string, bool> SetView { get; }
        private float _bassVolume=1f;
        private float _drumsVolume=1f;
        private float _melodyVolume = 1f;
        private float _vocalVolume = 1f;
        private bool StartSamplesOver => Settings.Default.StartSamplesOver;
        public int BassVolume
        {
            get => (int) (_bassVolume * 100);
            set => _bassVolume = value / 100f;
        }
        public int DrumsVolume
        {
            get => (int)(_drumsVolume * 100);
            set => _drumsVolume = value / 100f;
        }
        public int MelodyVolume
        {
            get => (int)(_melodyVolume * 100);
            set => _melodyVolume = value / 100f;
        }
        public int VocalVolume
        {
            get => (int)(_vocalVolume * 100);
            set => _vocalVolume = value / 100f;
        }

        public MusicList(Size fieldSize, Size centerSize, Func<string, bool,string, string,bool> setView)
        {
            WorkSize = new Size(fieldSize.Width / 2 - centerSize.Width / 2, fieldSize.Height / 2 - centerSize.Height / 2);
            CenterSize = centerSize;
            SetView = setView;
            int number = 0;
            var stringJson = File.ReadAllText("EffectsSettings.json");
            var effects = JsonConvert.DeserializeObject<List<EffectSettingJson>>(stringJson);
            TopEffect = EffectSettingVm.ConvertFromJson(effects.Find(find=> find.SideEffect=="Top"));
            LeftEffect = EffectSettingVm.ConvertFromJson(effects.Find(find => find.SideEffect == "Left"));
            RightEffect = EffectSettingVm.ConvertFromJson(effects.Find(find => find.SideEffect == "Right"));
            BottomEffect = EffectSettingVm.ConvertFromJson(effects.Find(find => find.SideEffect == "Bottom"));
            var dir = new DirectoryInfo(_musicPath);
            foreach (var musicDirs in dir.GetDirectories())
            {
                foreach (var sound in musicDirs.GetFiles())
                {
                    _streams.Add(new EffectStream(sound.FullName, false, 10000, true, 1, false, 10000, 0.5f, number));
                    number++;
                }
            }
        }
        bool _bassPlaying = false;
        bool _drumsPlaying = false;
        bool _melodyPlaying = false;
        bool _vocalPlaying = false;
        private LastFramesList _lastFrames = new LastFramesList(Settings.Default.ErrorFrames);

        public void ApplyTopEffect(ref EffectValue effect, PointF point)
        {
            if (TopEffect.CurrentEffect == CameraSettingsModel.ListEffects[0])
            {
                //echo
                var currentWorkHeight = point.Y;
                effect.EchoEnable = true;
                var maxEchoLength = TopEffect.SecondSettingValue;
                var minEchoLength = TopEffect.FirstSettingValue;
                var maxEchoVolume = TopEffect.ForthSettingValue;
                var minEchoVolume = TopEffect.ThirdSettingValue;
                effect.EchoLength = maxEchoLength-
                                   (int)(currentWorkHeight / WorkSize.Height * (maxEchoLength - minEchoLength));
                effect.EchoVolume = maxEchoVolume -
                                    (int)(currentWorkHeight / WorkSize.Height * (maxEchoVolume - minEchoVolume));
                effect.EchoLengthRelation = (effect.EchoLength * 100 / maxEchoLength);
                effect.EchoVolumeRelation = (int)(effect.EchoVolume * 100 / maxEchoVolume);
            }
            else
            {
                //low-pass and high-pass filters
                var currentWorkHeight = point.Y;
                effect.FilterEnable = true;
                var maxFilterFrequency = TopEffect.SecondSettingValue;
                var minFilterFrequency = TopEffect.FirstSettingValue;
                effect.Frequency = maxFilterFrequency -
                                   (int)(currentWorkHeight / WorkSize.Height * (maxFilterFrequency - minFilterFrequency));
                effect.FrequencyRelation = effect.Frequency * 100 / maxFilterFrequency;
                effect.PeakQ = TopEffect.ThirdSettingValue;
                //check low-pass or high-pass filter
                effect.FilterEffect = TopEffect.CurrentEffect != CameraSettingsModel.ListEffects[1];
            }
        }
        public void ApplyBottomEffect(ref EffectValue effect, PointF point)
        {
            if (BottomEffect.CurrentEffect == CameraSettingsModel.ListEffects[0])
            {
                //echo
                var currentWorkHeight = point.Y- CenterSize.Height - WorkSize.Height;
                effect.EchoEnable = true;
                var maxEchoLength = BottomEffect.SecondSettingValue;
                var minEchoLength = BottomEffect.FirstSettingValue;
                var maxEchoVolume = BottomEffect.ForthSettingValue;
                var minEchoVolume = BottomEffect.ThirdSettingValue;
                effect.EchoLength = minEchoLength +
                                    (int)(currentWorkHeight / WorkSize.Height * (maxEchoLength - minEchoLength));
                effect.EchoVolume = minEchoVolume +
                                    (int)(currentWorkHeight / WorkSize.Height * (maxEchoVolume - minEchoVolume));
                effect.EchoLengthRelation = (effect.EchoLength * 100 / maxEchoLength);
                effect.EchoVolumeRelation = (int)(effect.EchoVolume * 100 / maxEchoVolume);
            }
            else
            {
                //low-pass and high-pass filters
                var currentWorkHeight = point.Y - CenterSize.Height - WorkSize.Height;
                effect.FilterEnable = true;
                var maxFilterFrequency = BottomEffect.SecondSettingValue;
                var minFilterFrequency = BottomEffect.FirstSettingValue;
                effect.Frequency = minFilterFrequency +
                                   (int)(currentWorkHeight / WorkSize.Height * (maxFilterFrequency - minFilterFrequency));
                effect.FrequencyRelation = effect.Frequency * 100 / maxFilterFrequency;
                effect.PeakQ = BottomEffect.ThirdSettingValue;
                //check low-pass or high-pass filter
                effect.FilterEffect = BottomEffect.CurrentEffect != CameraSettingsModel.ListEffects[1];
            }
        }
        public void ApplyLeftEffect(ref EffectValue effect, PointF point)
        {
            if (LeftEffect.CurrentEffect == CameraSettingsModel.ListEffects[0])
            {
                //echo
                var currentWorkWidth = point.X;
                effect.EchoEnable = true;
                var maxEchoLength = LeftEffect.SecondSettingValue;
                var minEchoLength = LeftEffect.FirstSettingValue;
                var maxEchoVolume = LeftEffect.ForthSettingValue;
                var minEchoVolume = LeftEffect.ThirdSettingValue;
                effect.EchoLength = maxEchoLength -
                                    (int)(currentWorkWidth / WorkSize.Width * (maxEchoLength - minEchoLength));
                effect.EchoVolume = maxEchoVolume -
                                    (int)(currentWorkWidth / WorkSize.Width * (maxEchoVolume - minEchoVolume));
                effect.EchoLengthRelation = (effect.EchoLength *100 / maxEchoLength);
                effect.EchoVolumeRelation = (int) (effect.EchoVolume * 100 / maxEchoVolume);
            }
            else
            {
                //low-pass and high-pass filters
                var currentWorkWidth = point.X;
                effect.FilterEnable = true;
                var maxFilterFrequency = LeftEffect.SecondSettingValue;
                var minFilterFrequency = LeftEffect.FirstSettingValue;
                effect.Frequency = maxFilterFrequency -
                                   (int)(currentWorkWidth / WorkSize.Width * (maxFilterFrequency - minFilterFrequency));
                effect.FrequencyRelation = effect.Frequency * 100 / maxFilterFrequency;
                effect.PeakQ = LeftEffect.ThirdSettingValue;
                //check low-pass or high-pass filter
                effect.FilterEffect = LeftEffect.CurrentEffect != CameraSettingsModel.ListEffects[1];
            }
        }
        public void ApplyRightEffect(ref EffectValue effect, PointF point)
        {
            if (RightEffect.CurrentEffect == CameraSettingsModel.ListEffects[0])
            {
                //echo
                var currentWorkWidth = point.X - CenterSize.Width - WorkSize.Width;
                effect.EchoEnable = true;
                var maxEchoLength = RightEffect.SecondSettingValue;
                var minEchoLength = RightEffect.FirstSettingValue;
                var maxEchoVolume = RightEffect.ForthSettingValue;
                var minEchoVolume = RightEffect.ThirdSettingValue;
                effect.EchoLength = minEchoLength +
                                    (int)(currentWorkWidth / WorkSize.Width * (maxEchoLength - minEchoLength));
                effect.EchoVolume = minEchoVolume +
                                    (int)(currentWorkWidth / WorkSize.Width * (maxEchoVolume - minEchoVolume));
                effect.EchoLengthRelation = (effect.EchoLength * 100 / maxEchoLength);
                effect.EchoVolumeRelation = (int)(effect.EchoVolume * 100 / maxEchoVolume);
            }
            else
            {
                //low-pass and high-pass filters
                var currentWorkWidth = point.X - CenterSize.Width - WorkSize.Width;
                effect.FilterEnable = true;
                var maxFilterFrequency = RightEffect.SecondSettingValue;
                var minFilterFrequency = RightEffect.FirstSettingValue;
                effect.Frequency = minFilterFrequency +
                                   (int)(currentWorkWidth / WorkSize.Width * (maxFilterFrequency - minFilterFrequency));
                effect.FrequencyRelation = effect.Frequency * 100 / maxFilterFrequency;
                effect.PeakQ = RightEffect.ThirdSettingValue;
                //check low-pass or high-pass filter
                effect.FilterEffect = RightEffect.CurrentEffect != CameraSettingsModel.ListEffects[1];
            }
        }
        public void Update(List<ComingItem> list)
        {
            //if (list.Count == 0)
            //{
            //    BassSound.Stop();
            //    MelodySound.Stop();
            //    VocalSound.Stop();
            //    DrumsSound.Stop();
            //    return;
            //}
            Stop(list);
            // this booleans for current frame, this is to prevent 10 concurrently starting samples
            bool bassStarted = false;
            bool drumsStarted = false;
            bool melodyStarted = false;
            bool vocalStarted = false;
            foreach (var item in list)
            {
                if (item.Number>40) return;
                var effect = new EffectValue();
                if (item.Point.X<=WorkSize.Width)
                    ApplyLeftEffect(ref effect,item.Point);
                if (item.Point.X>=WorkSize.Width+CenterSize.Width)
                    ApplyRightEffect(ref effect, item.Point);
                if (item.Point.Y<=WorkSize.Height)
                    ApplyTopEffect(ref effect, item.Point);
                if (item.Point.Y>=WorkSize.Height+CenterSize.Height)
                    ApplyBottomEffect(ref effect, item.Point);
                _streams[item.Number].UpdateEffect(effect);
                StringBuilder effectString = new StringBuilder();
                if (effect.EchoEnable)
                {
                    effectString.Append("Echo effect\n");
                    effectString.Append($"Echo length: {effect.EchoLength} ({effect.EchoLengthRelation}%)\n");
                    effectString.Append($"Echo volume: {effect.EchoVolume} ({effect.EchoVolumeRelation}%)");
                }
                else
                {
                    if (effect.FilterEnable)
                    {
                        if (effect.FilterEffect)
                        {
                            effectString.Append("High-pass filter \n");
                        }
                        else
                        {
                            effectString.Append("Low-pass filter \n");
                        }
                        effectString.Append($"Frequency: {effect.Frequency} ({effect.FrequencyRelation}%)");
                    }
                    else
                    {
                        effectString.Append("No effects");
                    }
                }
                
                switch (item.Number / 10)
                {
                    case 0:
                        _streams[item.Number].SourceStream.Volume = _bassVolume;
                        if (bassStarted)
                            continue;
                        if (_lastFrames.ContainNumber(item.Number) && _bassPlaying)
                        {
                            SetView("bass", true, "prev", effectString.ToString());
                            continue;
                        }
                        if (item.Number != _bassPlayingNumber)
                        {
                            _bassPlayingNumber = item.Number;
                            BassSound.Stop();
                            BassSound.Dispose();
                            BassSound = new DirectSoundOut();
                            if (StartSamplesOver)
                            {
                                _streams[item.Number].Position = 0;
                            }
                            BassSound.Init(_streams[item.Number]);
                            BassSound.Play();
                            SetView("bass", true, _streams[item.Number].FileName, effectString.ToString());
                            _bassPlaying = true;
                            bassStarted = true;
                        }

                        break;
                    case 1:
                        _streams[item.Number].SourceStream.Volume = _drumsVolume;
                        if (drumsStarted)
                            continue;
                        if (_lastFrames.ContainNumber(item.Number) && _drumsPlaying)
                            continue;
                        if (item.Number != _drumsPlayingNumber)
                        {
                            _drumsPlayingNumber = item.Number;
                            DrumsSound.Stop();
                            DrumsSound.Dispose();
                            DrumsSound = new DirectSoundOut();
                            if (StartSamplesOver)
                            {
                                _streams[item.Number].Position = 0;
                            }
                            DrumsSound.Init(_streams[item.Number]);
                            DrumsSound.Play();
                            SetView("drums", true, _streams[item.Number].FileName, effectString.ToString());
                            _drumsPlaying = true;
                            drumsStarted = true;
                        }

                        break;
                    case 2:
                        _streams[item.Number].SourceStream.Volume = _melodyVolume;
                        if (melodyStarted)
                            continue;
                        if (_lastFrames.ContainNumber(item.Number) && _melodyPlaying)
                            continue;
                        if (item.Number != _melodyPlayingNumber)
                        {
                            _melodyPlayingNumber = item.Number;
                            MelodySound.Stop();
                            MelodySound.Dispose();
                            MelodySound = new DirectSoundOut();
                            if (StartSamplesOver)
                            {
                                _streams[item.Number].Position = 0;
                            }
                            MelodySound.Init(_streams[item.Number]);
                            MelodySound.Play();
                            SetView("melody", true, _streams[item.Number].FileName, effectString.ToString());
                            _melodyPlaying = true;
                            melodyStarted = true;
                        }

                        break;
                    case 3:
                        _streams[item.Number].SourceStream.Volume = _vocalVolume;
                        if (vocalStarted)
                            continue;
                        if (_lastFrames.ContainNumber(item.Number) && _vocalPlaying)
                            continue;
                        if (item.Number != _vocalPlayingNumber)
                        {
                            _vocalPlayingNumber = item.Number;
                            VocalSound.Stop();
                            VocalSound.Dispose();
                            VocalSound = new DirectSoundOut();
                            if (StartSamplesOver)
                            {
                                _streams[item.Number].Position = 0;
                            }
                            VocalSound.Init(_streams[item.Number]);
                            VocalSound.Play();
                            SetView("vocal", true, _streams[item.Number].FileName, effectString.ToString());
                            _vocalPlaying = true;
                            vocalStarted = true;
                        }
                        break;
                }
            }
            _lastFrames.Add(list);
        }
        private int _framesToStop => Settings.Default.ErrorFrames;
        private int _bassStop;
        private int _drumsStop;
        private int _melodyStop;
        private int _vocalStop;
        public void Stop(List<ComingItem> list)
        {
            if (list.FirstOrDefault(test => test.Number / 10 == 0) == null)
            {
                _bassStop++;
                if (_bassStop == _framesToStop)
                {
                    _bassStop = 0;
                    _bassPlayingNumber = 55;
                    BassSound.Stop();
                    BassSound.Dispose();
                    SetView("bass", false, "", "");
                    _bassPlaying = false;
                }
            }
            else
            {
                _bassStop = 0;
            }

            if (list.FirstOrDefault(test => test.Number / 10 == 1) == null)
            {
                _drumsStop++;
                if (_drumsStop >= _framesToStop)
                {
                    _drumsStop = 0;
                    _drumsPlayingNumber = 55;
                    DrumsSound.Stop();
                    DrumsSound.Dispose();
                    SetView("drums", false, "", "");
                    _drumsPlaying = false;
                }
            }
            else
            {
                _drumsStop = 0;
            }
            if (list.FirstOrDefault(test => test.Number / 10 == 2) == null)
            {
                _melodyStop++;
                if (_melodyStop >= _framesToStop)
                {
                    _melodyStop = 0;
                    _melodyPlayingNumber = 55;
                    MelodySound.Stop();
                    MelodySound.Dispose();
                    SetView("melody", false, "", "");
                    _melodyPlaying = false;
                }
            }
            else
            {
                _melodyStop = 0;
            }
            if (list.FirstOrDefault(test => test.Number / 10 == 3) == null)
            {
                _vocalStop++;
                if (_vocalStop >= _framesToStop)
                {
                    _vocalStop = 0;
                    _vocalPlayingNumber = 55;
                    VocalSound.Stop();
                    VocalSound.Dispose();
                    SetView("vocal", false, "", "");
                    _vocalPlaying = false;
                }
            }
            else
            {
                _vocalStop = 0;
            }
        }

        public void Dispose()
        {
            BassSound?.Dispose();
            MelodySound?.Dispose();
            DrumsSound?.Dispose();
            VocalSound?.Dispose();
            foreach (var item in _streams)
            {
                item?.Dispose();
            }
        }
    }

    public struct EffectValue
    {
        /// <summary>
        /// True is high-pass, false - low-pass
        /// </summary>
        public bool FilterEffect { get; set; }
        public bool FilterEnable { get; set; }
        public int Frequency { get; set; }
        public int FrequencyRelation { get; set; }
        public float PeakQ { get; set; }
        /// <summary>
        /// True is enable, false - disable
        /// </summary>
        public bool EchoEnable { get; set; }
        public int EchoLength { get; set; }
        public int EchoLengthRelation { get; set; }
        public float EchoVolume { get; set; }
        public int EchoVolumeRelation { get; set; }
    }
}
