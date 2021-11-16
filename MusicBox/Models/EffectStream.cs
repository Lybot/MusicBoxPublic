using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using MusicBox.MVVModels;
using NAudio.CoreAudioApi;
using NAudio.Dsp;
using NAudio.Wave;

namespace MusicBox.Models
{
    public class EffectStream : WaveStream
    {
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            SourceStream.Dispose();
        }
        public WaveChannel32 SourceStream { get; set; }
        public string FileName { get; }
        public int Number { get; set; }
        public List<IEffect> Effects { get; set; }
        private bool _filterEnable;
        private bool _echoEnable;

        /// <summary>
        /// Looping stream with high/low pass filters and echo effect
        /// </summary>
        /// <param name="path">path to wav file</param>
        /// <param name="filterEnable">Use high/low pass filter</param>
        /// <param name="cutOffFreq">Frequency to the filter</param>
        /// <param name="highFilter">True - high pass, false - low pass</param>
        /// <param name="peakQ">Peak signal (often use 1)</param>
        /// <param name="echoEnable">True if use echo</param>
        /// <param name="lengthEcho">Time which echo will repeat</param>
        /// <param name="factorEcho">Echo volume (0 - silent; 1 - maximum as main sound)</param>
        /// <param name="number">Which number of Aruco this stream concerns</param>
        public EffectStream(string path, bool filterEnable, float cutOffFreq, bool highFilter, float peakQ, bool echoEnable, int lengthEcho, float factorEcho, int number)
        {
            var fileNameLength = path.LastIndexOf(".", StringComparison.Ordinal)-path.LastIndexOf("\\", StringComparison.Ordinal);
            FileName = path.Substring(path.LastIndexOf("\\", StringComparison.Ordinal)+1, fileNameLength-3);
            Number = number;
            var loopStream = new LoopStream(new WaveFileReader(path));
            var stream = new WaveChannel32(loopStream);
            SourceStream = stream;
            _filters = new BiQuadFilter[WaveFormat.Channels];
            EnableLooping = true;
            _echoEnable = echoEnable;
            _filterEnable = filterEnable;
            CreateFilters(highFilter, cutOffFreq, peakQ);
            CreateEffects(lengthEcho, factorEcho);
        }

        public void CreateEffects(int lengthEcho, float factorEcho)
        {
            if (Effects == null)
            {
                Effects = new List<IEffect>();
                for (int i = 0; i < WaveFormat.Channels; i++)
                {
                    Effects.Add(new Echo(lengthEcho, factorEcho));
                }
            }
            else
                for (int i = 0; i < WaveFormat.Channels; i++)
                {
                    Effects[i].SetLengthEcho(lengthEcho,factorEcho);
                }
        }
        public void UpdateEffect(EffectValue effect)
        {
            _filterEnable = effect.FilterEnable;
            _echoEnable = effect.EchoEnable;
            if (effect.FilterEnable)
            {
                CreateFilters(effect.FilterEffect, effect.Frequency, effect.PeakQ);
            }
            if (effect.EchoEnable)
            {
                CreateEffects(effect.EchoLength, effect.EchoVolume);
            }
        }
        private readonly BiQuadFilter[] _filters;
        private void CreateFilters(bool highFilter, float cutOffFreq, float peakQ)
        {
            if (highFilter)
            {
                for (int n = 0; n < WaveFormat.Channels; n++)
                    if (_filters[n] == null)
                        _filters[n] = BiQuadFilter.HighPassFilter(WaveFormat.SampleRate, cutOffFreq, peakQ);
                    else
                        _filters[n].SetHighPassFilter(WaveFormat.SampleRate, cutOffFreq, peakQ);
            }
            else
            {
                for (int n = 0; n < WaveFormat.Channels; n++)
                    if (_filters[n] == null)
                        _filters[n] = BiQuadFilter.LowPassFilter(WaveFormat.SampleRate, cutOffFreq, peakQ);
                    else
                        _filters[n].SetLowPassFilter(WaveFormat.SampleRate, cutOffFreq, peakQ);
            }

        }
        public override long Length => SourceStream.Length;
        public bool EnableLooping { get; set; }
        public override long Position
        {
            get => SourceStream.Position;
            set => SourceStream.Position = value;
        }

        public sealed override WaveFormat WaveFormat => SourceStream.WaveFormat;

        private int _channel;

        public override int Read(byte[] buffer, int offset, int count)
        {
            // Console.WriteLine("DirectSoundOut requested {0} bytes", count);
            int totalBytesRead = 0;
            while (totalBytesRead < count)
            {
                int read = SourceStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
                if (read == 0)
                {
                    if (Position == 0 || !EnableLooping)
                    {
                        // something wrong with the source stream
                        break;
                    }
                    Position = 0;
                }
                totalBytesRead += read;
                for (int i = 0; i < read / 4; i++)
                {
                    float sample = BitConverter.ToSingle(buffer, i * 4);
                    if (Effects.Count == WaveFormat.Channels)
                    {
                        if (_filterEnable)
                            sample = _filters[_channel].Transform(sample);
                        if (_echoEnable)
                            sample = Effects[_channel].ApplyEffect(sample);
                        _channel = (_channel + 1) % WaveFormat.Channels;
                    }
                    byte[] bytes = BitConverter.GetBytes(sample);
                    buffer[i * 4 + 0] = bytes[0];
                    buffer[i * 4 + 1] = bytes[1];
                    buffer[i * 4 + 2] = bytes[2];
                    buffer[i * 4 + 3] = bytes[3];
                }
            }

            return totalBytesRead;
        }
    }
    public interface IEffect
    {
        float ApplyEffect(float sample);
        void SetLengthEcho(int length, float factor);
    }
    public class Echo : IEffect
    {
        public int EchoLength { get; set; }

        public float EchoFactor { get; set; }

        private readonly Queue<float> _samples;

        public Echo(int length = 20000, float factor = 0.5f)
        {
            EchoLength = length;
            EchoFactor = factor;
            _samples = new Queue<float>();
            for (int i = 0; i < length; i++) _samples.Enqueue(0f);
        }

        public float ApplyEffect(float sample)
        {
            _samples.Enqueue(sample);
            return Math.Min(1, Math.Max(-1, sample + EchoFactor * _samples.Dequeue()));
        }

        public void SetLengthEcho(int length, float factor)
        {
            EchoFactor = factor;
            EchoLength = length;
        }
    }
    public class LoopStream : WaveStream
    {
        readonly WaveStream _sourceStream;

        /// <summary>
        /// Creates a new Loop stream
        /// </summary>
        /// <param name="sourceStream">The stream to read from. Note: the Read method of this stream should return 0 when it reaches the end
        /// or else we will not loop to the start again.</param>
        public LoopStream(WaveStream sourceStream)
        {
            _sourceStream = sourceStream;
            EnableLooping = true;
        }

        /// <summary>
        /// Use this to turn looping on or off
        /// </summary>
        public bool EnableLooping { get; set; }
        /// <summary>
        /// Return source stream's wave format
        /// </summary>
        public override WaveFormat WaveFormat => _sourceStream.WaveFormat;

        /// <summary>
        /// LoopStream simply returns
        /// </summary>
        public override long Length => _sourceStream.Length;

        /// <summary>
        /// LoopStream simply passes on positioning to source stream
        /// </summary>
        public override long Position
        {
            get => _sourceStream.Position;
            set => _sourceStream.Position = value;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int totalBytesRead = 0;

            while (totalBytesRead < count)
            {
                int bytesRead = _sourceStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
                if (bytesRead == 0)
                {
                    if (_sourceStream.Position == 0 || !EnableLooping)
                    {
                        // something wrong with the source stream
                        break;
                    }
                    // loop
                    _sourceStream.Position = 0;
                }
                totalBytesRead += bytesRead;
            }
            return totalBytesRead;
        }
    }
}
