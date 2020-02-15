using System;
using System.Diagnostics.Contracts;
using System.IO;
using MathCore.WAV.Service;

// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable UnusedMember.Global

namespace MathCore.WAV
{
    public class WavFile : IDisposable//, IEnumerable<WAVFrame>
    {
        private Stream _DataStream;
        private readonly Header _Header;

        public long FullLength => _DataStream.Length;
        public long DataLength => FullLength - Header.Length;

        public int SamplingFrequency => _Header.SampleRate;

        public int ByteRate => _Header.SampleRate;

        public short FrameLength => _Header.BlockAlign;

        public long FramesCount => DataLength / FrameLength;

        public double dt => 1d / _Header.SampleRate;

        public double FileTimeLength => FramesCount * dt;

        protected long Position
        {
            get => _DataStream.Position;
            set => _DataStream.Seek(value - Header.Length, SeekOrigin.Begin);
        }

        public int ChannelsCount => _Header.NumChannels;

        public short SampleLength => (short)(((_Header.BitsPerSample - 1) >> 3) + 1);

        public Frame this[int i]
        {
            get
            {
                var sample_data = new byte[_Header.BlockAlign];
                _DataStream.Seek(i * SampleLength + Header.Length, SeekOrigin.Begin);
                _DataStream.Read(sample_data, 0, sample_data.Length);
                return new Frame(i / (double)_Header.SampleRate, _Header.NumChannels, sample_data);
            }
        }

        public WavFile(string FileName) : this(new FileInfo(FileName)) { }

        public WavFile(FileInfo File) : this(File.Open(FileMode.Open, FileAccess.Read, FileShare.Read)) { }

        protected WavFile(Stream DataStream)
        {
            _DataStream = DataStream;
            if (_DataStream.Position != 0) _DataStream.Seek(0L, SeekOrigin.Begin);
            _Header = _DataStream.FromStructure<Header>();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool Disposing)
        {
            if (!Disposing) return;
            if (_DataStream is null) return;
            _DataStream.Dispose();
            _DataStream = null;
        }

        //IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        //public IEnumerator<Frame> GetEnumerator()
        //{
        //    var length = SampleLength;
        //    var channel_count = ChannelsCount;
        //    while (_DataStream.Position < _DataStream.Length)
        //    {
        //        var data = new byte[_Header.BlockAlign];
        //        var time = _DataStream.Position / (double)_Header.SampleRate;
        //        yield return new Frame(time, channel_count, length, data);
        //    }
        //}
    }
}
