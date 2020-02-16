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
        private Header _Header;

        private static Exception NotLoaded() => new InvalidOperationException("Не выполнена загрузка данных");

        private Stream DataStream => _DataStream ?? throw NotLoaded();

        public Header Header => _DataStream is null ? throw NotLoaded() : _Header;

        public FileInfo File => _DataStream is FileStream file_stream ? new FileInfo(file_stream.Name) : null;

        /// <summary>полная длина файла в байтах включая заголовок</summary>
        public long FullLength => DataStream.Length;

        public long DataLength => Header.SubChunk2Size;

        public int SamplingFrequency => Header.SampleRate;

        public int ByteRate => Header.SampleRate;

        public short FrameLength => Header.BlockAlign;

        public long FramesCount => Header.FrameCount;

        public double dt => 1d / Header.SampleRate;

        public double FileTimeLength => FramesCount * dt;

        protected long Position
        {
            get => DataStream.Position;
            set => DataStream.Seek(value - Header.Length, SeekOrigin.Begin);
        }

        public int ChannelsCount => Header.ChannelsCount;

        public short SampleLength => (short)(((Header.BitsPerSample - 1) >> 3) + 1);

        public Frame this[int i]
        {
            get
            {
                var sample_length = _Header.BlockAlign;
                var data_offset = Header.Length + i * sample_length;
                if (i < 0 || data_offset >= _DataStream.Length - sample_length)
                    throw new EndOfStreamException("Попытка чтения данных за пределами потока");

                var sample_data = new byte[sample_length];
                _DataStream.Seek(data_offset, SeekOrigin.Begin);
                _DataStream.Read(sample_data, 0, sample_length);
                return new Frame(i / (double)_Header.SampleRate, _Header.ChannelsCount, sample_data);
            }
        }

        public Header LoadFrom(string FileName) => LoadFrom(new FileInfo(FileName ?? throw new ArgumentNullException(nameof(FileName))));

        public Header LoadFrom(FileInfo DataFile) => LoadFrom((DataFile ?? throw new ArgumentNullException(nameof(DataFile))).OpenRead());

        public Header LoadFrom(Stream Stream)
        {
            if (_DataStream != null)
                throw new InvalidOperationException("Поток данных уже был загружен в данный экземпляр");
            _DataStream = Stream;
            return _Header = Header.Load(Stream);
        }

        public long[] GetChannel(int Channel)
        {
            DataStream.Seek(0, SeekOrigin.Begin);
            var channels_count = _Header.ChannelsCount;
            if (Channel >= channels_count)
                throw new ArgumentOutOfRangeException(nameof(Channel), Channel, $"В файле содержится {channels_count} каналов, а запрошен {Channel}");

            var sample_length = _Header.BlockAlign;
            var sample_data = new byte[sample_length];

            var data_length = _Header.FrameCount;
            var result = new long[data_length];

            var bytes_per_sample = _Header.BytesPerSample;
            for (var i = 0; i < data_length; i++)
            {
                _DataStream.Seek(Header.Length + i * sample_length, SeekOrigin.Begin);
                _DataStream.Read(sample_data, 0, sample_length);
                result[i] = bytes_per_sample switch
                {
                    1 => sample_data[Channel],
                    2 => BitConverter.ToInt16(sample_data, Channel * bytes_per_sample),
                    4 => BitConverter.ToInt32(sample_data, Channel * bytes_per_sample),
                    8 => BitConverter.ToInt64(sample_data, Channel * bytes_per_sample),
                    _ => throw new NotSupportedException($"Размерность отсчёта {bytes_per_sample} байт на канал не поддерживается")
                };
            }
            return result;
        }

        public long[][] GetChannels()
        {
            DataStream.Seek(0, SeekOrigin.Begin);
            var channels_count = _Header.ChannelsCount;

            var sample_length = _Header.BlockAlign;
            var sample_data = new byte[sample_length];

            var data_length = _Header.FrameCount;
            var result = new long[channels_count][];
            for(var channel = 0; channel < channels_count; channel++)
                result[channel] = new long[data_length];

            var bytes_per_sample = _Header.BytesPerSample;
            for (var i = 0; i < data_length; i++)
            {
                _DataStream.Seek(Header.Length + i * sample_length, SeekOrigin.Begin);
                _DataStream.Read(sample_data, 0, sample_length);
                for(var channel = 0; channel < channels_count; channel++)
                    result[channel][i] = bytes_per_sample switch
                    {
                        1 => sample_data[channel],
                        2 => BitConverter.ToInt16(sample_data, channel * bytes_per_sample),
                        4 => BitConverter.ToInt32(sample_data, channel * bytes_per_sample),
                        8 => BitConverter.ToInt64(sample_data, channel * bytes_per_sample),
                        _ => throw new NotSupportedException($"Размерность отсчёта {bytes_per_sample} байт на канал не поддерживается")
                    };
            }
            return result;
        }


        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool Disposing)
        {
            if (!Disposing) return;
            if (_DataStream is null) return;
            _DataStream?.Dispose();
            _DataStream = null;
        }

        #endregion

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
