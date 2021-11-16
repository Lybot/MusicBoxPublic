using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using MusicBox.Models;
using MusicBox.Properties;
using MusicBox.ViewModels;
using NAudio.Wave;
using Prism.Commands;
using Prism.Mvvm;

namespace MusicBox.MVVModels
{
    public class MusicSettingsModel : BindableBase
    {
        private ObservableCollection<MusicItem> _basses = new ObservableCollection<MusicItem>();
        private ObservableCollection<MusicItem> _melodies = new ObservableCollection<MusicItem>();
        private ObservableCollection<MusicItem> _vocals = new ObservableCollection<MusicItem>();
        private ObservableCollection<MusicItem> _drums = new ObservableCollection<MusicItem>();
        private ObservableCollection<string> _presets;
        private readonly string _musicPath = Environment.CurrentDirectory + "\\Music\\";
        private readonly ImageSource _downImage = Functions.BitmapToImageSource(Resources.Down, false);
        private readonly ImageSource _upImage = Functions.BitmapToImageSource(Resources.Up, false);
        private readonly ImageSource _playImage = Functions.BitmapToImageSource(Resources.PlayImage, false);
        private ImageSource _pauseImage = Functions.BitmapToImageSource(Resources.PauseImage, false);
        private string _currentPreset;
        private bool _hasChanged;
        public string CurrentPreset
        {
            get => _currentPreset;
            set
            {
                var previousPreset = Environment.CurrentDirectory + "\\Presets\\" + _currentPreset;
                if (File.Exists(previousPreset)&&_hasChanged)
                {
                    File.Delete(previousPreset);
                    ZipFile.CreateFromDirectory(Environment.CurrentDirectory + "\\Music", previousPreset);
                }
                _currentPreset = value;
                var newPreset = Environment.CurrentDirectory + "\\Presets\\" + _currentPreset;
                bool shouldExtract = false;
                if (Directory.Exists(Environment.CurrentDirectory + "\\Music"))
                {
                    using (ZipArchive zipArchive = ZipFile.OpenRead(newPreset))
                    {
                        var currentMusic = new List<string>();
                        currentMusic.AddRange(Directory.GetFiles(Environment.CurrentDirectory + "\\Music\\Basses").Select(Path.GetFileName));
                        currentMusic.AddRange(Directory.GetFiles(Environment.CurrentDirectory + "\\Music\\Drums").Select(Path.GetFileName));
                        currentMusic.AddRange(Directory.GetFiles(Environment.CurrentDirectory + "\\Music\\Melodies").Select(Path.GetFileName));
                        currentMusic.AddRange(Directory.GetFiles(Environment.CurrentDirectory + "\\Music\\Vocals").Select(Path.GetFileName));
                        foreach (ZipArchiveEntry entry in zipArchive.Entries)
                        {
                            var wav = entry.FullName.Substring(entry.FullName.IndexOf('/') + 1);
                            if (wav.Length > 0 && !currentMusic.Contains(wav))
                            {
                                shouldExtract = true;
                            }
                            //if (entry.FullName.Substring(entry.FullName.IndexOf('/'))
                            //{
                            //    var wav = entry.FullName.Substring(entry.FullName.IndexOf('/'));
                            //    if (!currentMusic.Contains(wav))
                            //        shouldExtract = true;
                            //}
                        }
                    }
                }
                if (File.Exists(newPreset) && shouldExtract)
                {
                    Directory.Delete(Environment.CurrentDirectory + "\\Music", true);
                    ZipFile.ExtractToDirectory(newPreset, Environment.CurrentDirectory + "\\Music");
                }
                Load();
                Settings.Default.LastPreset = _currentPreset;
                Settings.Default.Save();
                RaisePropertyChanged();
                _hasChanged = false;
            }
        }
        public ObservableCollection<string> Presets
        {
            get => _presets;
            set
            {
                _presets = value;
                RaisePropertyChanged();
            }
        }
        public ObservableCollection<MusicItem> Basses
        {
            get => _basses;
            set
            {
                _basses = value;
                RaisePropertyChanged();
            }
        }
        public ObservableCollection<MusicItem> Melodies
        {
            get => _melodies;
            set
            {
                _melodies = value;
                RaisePropertyChanged();
            }
        }
        public ObservableCollection<MusicItem> Vocals
        {
            get => _vocals;
            set
            {
                _vocals = value;
                RaisePropertyChanged();
            }
        }
        public ObservableCollection<MusicItem> Drums
        {
            get => _drums;
            set
            {
                _drums = value;
                RaisePropertyChanged();
            }
        }

        private MusicListPlayer _bassPlayer;
        private MusicListPlayer _drumsPlayer;
        private MusicListPlayer _melodyPlayer;
        private MusicListPlayer _vocalPlayer;
        private void LoadPresets()
        {
            var dir = new DirectoryInfo(Environment.CurrentDirectory + "\\Presets");
            if (Presets == null)
                Presets = new ObservableCollection<string>();
            foreach(var preset in dir.GetFiles())
            {
                Presets.Add(preset.Name);
                if (preset.Name == Settings.Default.LastPreset)
                {
                    CurrentPreset = preset.Name;
                }
            }
            if (string.IsNullOrEmpty(CurrentPreset))
            {
                CurrentPreset = Presets[0];
            }
        }
        private void PlayBass(MusicItem item, string pathToList)
        {
            if (item.IsPlay)
            {
                _bassPlayer.Stop();
                _bassPlayer.PlaybackStopped -= PlayerStopped;
                _bassPlayer.Dispose();
                item.PlayStopImage = _playImage;
                item.IsPlay = false;
                return;
            }
            foreach (var bassItem in Basses)
            {
                if (bassItem.IsPlay)
                {
                    bassItem.IsPlay = false;
                    bassItem.PlayStopImage = _playImage;
                }
            }
            item.IsPlay = true;
            item.PlayStopImage = _pauseImage;
            if (_bassPlayer != null)
            {
                _bassPlayer.Stop();
                _bassPlayer.Dispose();
            }

            _bassPlayer = new MusicListPlayer {MusicItem = item};
            _bassPlayer.PlaybackStopped += PlayerStopped;
            var path = pathToList + "\\" + item.Name;
            var reader = new WaveFileReader(path);
            var stream = new LoopStream(reader)
            {
                EnableLooping = false
            };
            _bassPlayer.Init(stream);
            _bassPlayer.Play();
        }
        private void PlayerStopped(object sender, StoppedEventArgs e)
        {
            if (sender is MusicListPlayer player)
            {
                player.MusicItem.IsPlay = false;
                player.MusicItem.PlayStopImage = _playImage;
            }
        }

        private void PlayDrum(MusicItem item, string pathToList)
        {
            if (item.IsPlay)
            {
                _drumsPlayer.Stop();
                _drumsPlayer.PlaybackStopped -= PlayerStopped;
                _drumsPlayer.Dispose();
                item.PlayStopImage = _playImage;
                item.IsPlay = false;
                return;
            }
            foreach (var drumItem in Drums)
            {
                if (drumItem.IsPlay)
                {
                    drumItem.IsPlay = false;
                    drumItem.PlayStopImage = _playImage;
                }
            }
            item.IsPlay = true;
            item.PlayStopImage = _pauseImage;
            if (_drumsPlayer != null)
            {
                _drumsPlayer.Stop();
                _drumsPlayer.Dispose();
            }

            _drumsPlayer = new MusicListPlayer { MusicItem = item };
            _drumsPlayer.PlaybackStopped += PlayerStopped;
            var path = pathToList + "\\" + item.Name;
            var reader = new WaveFileReader(path);
            var stream = new LoopStream(reader)
            {
                EnableLooping = false
            };
            _drumsPlayer.Init(stream);
            _drumsPlayer.Play();
        }

        private void ChooseSample(MusicItem item, string pathToList)
        {
            var dialog = new OpenFileDialog
            {
                Filter = @"Wav files (*.wav)|*.wav",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)
            };
            if (dialog.ShowDialog() != DialogResult.OK) return;
            var newName = item.Number + " " + dialog.SafeFileName;
            File.Delete(pathToList + "\\" + item.Name);
            item.Name = newName;
            File.Copy(dialog.FileName, pathToList+"\\"+newName);
            _hasChanged = true;
        }
        private void PlayMelody(MusicItem item, string pathToList)
        {
            if (item.IsPlay)
            {
                _melodyPlayer.Stop();
                _melodyPlayer.PlaybackStopped -= PlayerStopped;
                _melodyPlayer.Dispose();
                item.PlayStopImage = _playImage;
                item.IsPlay = false;
                return;
            }
            foreach (var melodyItem in Melodies)
            {
                if (melodyItem.IsPlay)
                {
                    melodyItem.IsPlay = false;
                    melodyItem.PlayStopImage = _playImage;
                }
            }
            item.IsPlay = true;
            item.PlayStopImage = _pauseImage;
            if (_melodyPlayer != null)
            {
                _melodyPlayer.Stop();
                _melodyPlayer.Dispose();
            }

            _melodyPlayer = new MusicListPlayer { MusicItem = item };
            _melodyPlayer.PlaybackStopped += PlayerStopped;
            var path = pathToList + "\\" + item.Name;
            var reader = new WaveFileReader(path);
            var stream = new LoopStream(reader)
            {
                EnableLooping = false
            };
            _melodyPlayer.Init(stream);
            _melodyPlayer.Play();
        }
        private void PlayVocal(MusicItem item, string pathToList)
        {
            if (item.IsPlay)
            {
                _vocalPlayer.Stop();
                _vocalPlayer.PlaybackStopped -= PlayerStopped;
                _vocalPlayer.Dispose();
                item.PlayStopImage = _playImage;
                item.IsPlay = false;
                return;
            }
            foreach (var vocalItem in Vocals)
            {
                if (vocalItem.IsPlay)
                {
                    vocalItem.IsPlay = false;
                    vocalItem.PlayStopImage = _playImage;
                }
            }
            item.IsPlay = true;
            item.PlayStopImage = _pauseImage;
            if (_vocalPlayer != null)
            {
                _vocalPlayer.Stop();
                _vocalPlayer.Dispose();
            }
            _vocalPlayer = new MusicListPlayer { MusicItem = item };
            _vocalPlayer.PlaybackStopped += PlayerStopped;
            var path = pathToList + "\\" + item.Name;
            var reader = new WaveFileReader(path);
            var stream = new LoopStream(reader)
            {
                EnableLooping = false
            };
            _vocalPlayer.Init(stream);
            _vocalPlayer.Play();
        }
        private void Load()
        {
            var dir = new DirectoryInfo(_musicPath);
            Basses = new ObservableCollection<MusicItem>();
            Drums = new ObservableCollection<MusicItem>();
            Melodies = new ObservableCollection<MusicItem>();
            Vocals = new ObservableCollection<MusicItem>();
            foreach (var musicDirs in dir.GetDirectories())
            {
                foreach (var sound in musicDirs.GetFiles())
                {
                    var listItem = new MusicItem()
                    {
                        DownImage = _downImage,
                        UpImage = _upImage,
                        IsPlay = false,
                        Name = sound.Name,
                        Number = sound.Name.Substring(0, 2),
                        PlayStopImage = _playImage,
                    };
                    if (musicDirs.Name == "Basses")
                    {
                        listItem.DownNumber = new DelegateCommand(delegate
                        {
                            Down(Basses, listItem, musicDirs.FullName);
                        });
                        listItem.UpNumber = new DelegateCommand(delegate
                        {
                            Up(Basses, listItem, musicDirs.FullName);
                        });
                        listItem.PlayPause = new DelegateCommand(delegate
                        {
                            PlayBass(listItem, musicDirs.FullName);
                        });
                        listItem.UpdatePath = new DelegateCommand(delegate
                        {
                            ChooseSample(listItem, musicDirs.FullName);
                        });
                        Basses.Add(listItem);
                    }
                    if (musicDirs.Name == "Vocals")
                    {
                        listItem.DownNumber = new DelegateCommand(delegate
                        {
                            Down(Vocals, listItem, musicDirs.FullName);
                        });
                        listItem.UpNumber = new DelegateCommand(delegate
                        {
                            Up(Vocals, listItem, musicDirs.FullName);
                        });
                        listItem.PlayPause = new DelegateCommand(delegate
                        {
                            PlayVocal(listItem, musicDirs.FullName);
                        });
                        listItem.UpdatePath = new DelegateCommand(delegate
                        {
                            ChooseSample(listItem, musicDirs.FullName);
                        });
                        Vocals.Add(listItem);
                    }
                    if (musicDirs.Name == "Melodies")
                    {
                        listItem.DownNumber = new DelegateCommand(delegate
                        {
                            Down(Melodies, listItem, musicDirs.FullName);
                        });
                        listItem.UpNumber = new DelegateCommand(delegate
                        {
                            Up(Melodies, listItem, musicDirs.FullName);
                        });
                        listItem.PlayPause = new DelegateCommand(delegate
                        {
                            PlayMelody(listItem, musicDirs.FullName);
                        });
                        listItem.UpdatePath = new DelegateCommand(delegate
                        {
                            ChooseSample(listItem, musicDirs.FullName);
                        });
                        Melodies.Add(listItem);
                    }
                    if (musicDirs.Name == "Drums")
                    {
                        listItem.DownNumber = new DelegateCommand(delegate
                        {
                            Down(Drums, listItem, musicDirs.FullName);
                        });
                        listItem.UpNumber = new DelegateCommand(delegate
                        {
                            Up(Drums, listItem, musicDirs.FullName);
                        });
                        listItem.PlayPause = new DelegateCommand(delegate
                        {
                            PlayDrum(listItem, musicDirs.FullName);
                        });
                        listItem.UpdatePath = new DelegateCommand(delegate
                        {
                            ChooseSample(listItem, musicDirs.FullName);
                        });
                        Drums.Add(listItem);
                    }
                }
            }
        }
        private void Replace(ObservableCollection<MusicItem> list, MusicItem firstItem, MusicItem secondItem, string pathToList)
        {
            var firstNewPath = secondItem.Number + " " + firstItem.Name.Substring(2);
            var secondNewPath = firstItem.Number + " " + secondItem.Name.Substring(2);
            File.Move(pathToList + "\\" + firstItem.Name, pathToList + "\\" + firstNewPath);
            File.Move(pathToList + "\\" + secondItem.Name, pathToList + "\\" + secondNewPath);
            while (!Functions.IsFileReady(pathToList + "\\" + firstNewPath) || !Functions.IsFileReady(pathToList + "\\" + secondNewPath))
                Thread.Sleep(50);
            var tempName = firstNewPath;
            var tempNumber = firstItem.Number;
            var indexOfFirst = list.IndexOf(firstItem);
            var indexOfSecond = list.IndexOf(secondItem);
            list[indexOfFirst].Name = secondNewPath;
            list[indexOfSecond].Name = tempName;
            _hasChanged = true;
            //list[indexOfFirst].Number = secondItem.Number;
            //list[indexOfSecond].Number = tempNumber;

        }
        private void Down(ObservableCollection<MusicItem> list, MusicItem item, string pathToList)
        {
            var index = list.IndexOf(item);
            if (index >= 9)
                return;
            var downItem = list[index + 1];
            Replace(list, item, downItem, pathToList);
        }

        private void Up(ObservableCollection<MusicItem> list, MusicItem item, string pathToList)
        {
            var index = list.IndexOf(item);
            if (index <= 0)
                return;
            var upItem = list[index - 1];
            Replace(list, item, upItem, pathToList);
        }
        public void AddPreset()
        {
            var presetPath = Environment.CurrentDirectory + "\\Presets";
            File.Copy(presetPath + "\\Default", presetPath + "\\Default " + Presets.Count.ToString());
            Presets.Add("Default " + Presets.Count.ToString());
        }
        public void SaveChanges()
        {
            var previousPreset = Environment.CurrentDirectory + "\\Presets\\" + _currentPreset;
            ZipFile.CreateFromDirectory(Environment.CurrentDirectory + "\\Music", previousPreset);
        }
        public MusicSettingsModel()
        {
            LoadPresets();
        }
    }
    public class MusicListPlayer : WaveOut {
        public MusicItem MusicItem { get; set; }
    }
}
