using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Controls;
using MusicBox.MVVModels;
using Prism.Mvvm;

namespace MusicBox.ViewModels
{
    public class EffectSettingVm:BindableBase
    {
        private readonly Dictionary<string, List<string>> _descriptions = new Dictionary<string, List <string>>()
        {
            {"Filter", new List<string>(){"Lower limit of filter", "Higher limit of filter", "Peak signal", ""}},
            {"Echo", new List<string>(){"Minimum echo length", "Maximum echo length", "Minimum echo volume", "Maximum echo volume"}}
        };

        private void ChangeDescriptions(List<string> list)
        {
            FirstSettingDescription = list[0];
            SecondSettingDescription = list[1];
            ThirdSettingDescription = list[2];
            ForthSettingDescription = list[3];
            RaisePropertyChanged($"FirstSettingDescription");
            RaisePropertyChanged($"SecondSettingDescription");
            RaisePropertyChanged($"ThirdSettingDescription");
            RaisePropertyChanged($"ForthSettingDescription");
        }
        public static EffectSettingVm ConvertFromJson(EffectSettingJson setting)
        {
            var effectSetting = new EffectSettingVm(CameraSettingsModel.ListEffects, setting.NameEffect, setting.FirstSetting, setting.SecondSetting, setting.ThirdSetting, setting.ForthSetting, setting.SideEffect);
            return effectSetting;
        }
        public static event EventHandler SettingChanged;
        public EffectSettingJson ConvertToJson(string side)
        {
            var fileSetting = new EffectSettingJson()
            {
                FirstSetting = FirstSettingValue,
                SecondSetting = SecondSettingValue,
                ThirdSetting = ThirdSettingValue,
                ForthSetting = ForthSettingValue,
                NameEffect = CurrentEffect,
                SideEffect = side
            };
            return fileSetting;
        }
        public EffectSettingVm(ObservableCollection<string> effectVariants, string currentEffect, int firstSettings, int secondSetting, float thirdSetting, float forthSetting, string side)
        {
            EffectVariants = effectVariants;
            CurrentEffect = currentEffect;
            FirstSettingValue = firstSettings;
            SecondSettingValue = secondSetting;
            ThirdSettingValue = thirdSetting;
            ForthSettingValue = forthSetting;
            if (side == "Top" || side == "Bottom")
                Orientation = Orientation.Horizontal;
            else
            {
                Orientation = Orientation.Vertical;
            }

            if (currentEffect == effectVariants[1] || currentEffect == effectVariants[2])
            {
                ChangeDescriptions(_descriptions["Filter"]);
            }
            else
            {
                ChangeDescriptions(_descriptions["Echo"]);
            }
            RaisePropertyChanged($"FirstSetting");
            RaisePropertyChanged($"SecondSetting");
            RaisePropertyChanged($"ThirdSetting");
            RaisePropertyChanged($"ForthSetting");
        }
        private ObservableCollection<string> _effectVariants;
        public ObservableCollection<string> EffectVariants
        {
            get => _effectVariants;
            set
            {
                _effectVariants = value;
                RaisePropertyChanged();
            }
        }
        private string _currentEffect;
        public string CurrentEffect
        {
            get => _currentEffect;
            set
            {
                _currentEffect = value;
                RaisePropertyChanged();
                SettingChanged?.Invoke(null, null);
                if (value == EffectVariants[1] || value == EffectVariants[2])
                {
                    ChangeDescriptions(_descriptions["Filter"]);
                }
                else
                {
                    ChangeDescriptions(_descriptions["Echo"]);
                }
            }
        }

        private Orientation _orientation;
        public Orientation Orientation
        {
            get => _orientation;
            set
            {
                _orientation = value;
                RaisePropertyChanged();
                RaisePropertyChanged($"OppositeOrientation");
            }
        }

        public Orientation OppositeOrientation
        {
            get
            {
                if (Orientation == Orientation.Horizontal)
                    return Orientation.Vertical;
                return Orientation.Horizontal;
            }
        }
        public string FirstSettingDescription { get; set; }
        public string SecondSettingDescription { get; set; }
        public string ThirdSettingDescription { get; set; }
        public string ForthSettingDescription { get; set; }
        /// <summary>
        /// For echo - min echo-length;
        /// For filters - min filter frequency
        /// </summary>
        public int FirstSettingValue;
        /// <summary>
        /// For echo - max echo-length;
        /// For filters - max filter frequency
        /// </summary>
        public int SecondSettingValue;
        /// <summary>
        /// For echo - min echo-volume;
        /// For filters - peakQ
        /// </summary>
        public float ThirdSettingValue;
        /// <summary>
        /// For echo - max echo-volume;
        /// For filters - null or empty
        /// </summary>
        public float ForthSettingValue;
        public string FirstSetting
        {
            get => FirstSettingValue.ToString(CultureInfo.InvariantCulture);
            set
            {
                if (int.TryParse(value, out var result))
                {
                    FirstSettingValue = result;
                    RaisePropertyChanged();
                    SettingChanged?.Invoke(null, null);
                }
            }
        }
        public string SecondSetting
        {
            get => SecondSettingValue.ToString(CultureInfo.InvariantCulture);
            set
            {
                if (int.TryParse(value, out var result))
                {
                    SecondSettingValue = result;
                    RaisePropertyChanged();
                    SettingChanged?.Invoke(null, null);
                }
            }
        }
        public string ThirdSetting
        {
            get => ThirdSettingValue.ToString(CultureInfo.InvariantCulture);
            set
            {
                if (float.TryParse(value, out var result))
                {
                    ThirdSettingValue = result;
                    RaisePropertyChanged();
                    SettingChanged?.Invoke(null, null);
                }
            }
        }
        public string ForthSetting
        {
            get => ForthSettingValue.ToString(CultureInfo.InvariantCulture);
            set
            {
                if (float.TryParse(value, out var result))
                {
                    ForthSettingValue = result;
                    RaisePropertyChanged();
                    SettingChanged?.Invoke(null, null);
                }
            }
        }
    }

    public struct EffectSettingJson
    {
        public string SideEffect { get; set; }
        public string NameEffect { get; set; }
        public int FirstSetting { get; set; }
        public int SecondSetting { get; set; }
        public float ThirdSetting { get; set; }
        public float ForthSetting { get; set; }
    }
}
