using System;
using System.Collections.Generic;
using System.IO;

namespace MathCore.WAV
{
    public class WavStream : Wav, IDisposable
    {
        private readonly Stream _DataStream;
        private readonly bool _LeaveOpen;

        public override Frame this[int i]
        {
            get
            {
                if (!_DataStream.CanSeek)
                    throw new NotSupportedException("Поток данных не поддерживает возможностей поиска положения в нём");

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

        public WavStream(Stream DataStream, bool LeaveOpen = false)
            : base(Header.Load(DataStream ?? throw new ArgumentNullException(nameof(DataStream))))
        {
            _DataStream = DataStream;
            _LeaveOpen = LeaveOpen;
        }

        public override Stream GetDataStream()
        {
            if (_DataStream is FileStream file)
                return new FileStream(file.Name, FileMode.Open);
            if (_DataStream.CanSeek)
                _DataStream.Seek(Header.Length, SeekOrigin.Begin);
            return _DataStream;
        }

        public override IEnumerable<(double Time, long Value)> EnumerateSamples(int Channel)
        {
            Stream data_stream = null;
            try
            {
                data_stream = GetDataStream();

                var channels_count = _Header.ChannelsCount;
                if (Channel >= channels_count)
                    throw new ArgumentOutOfRangeException(nameof(Channel), Channel, $"В файле содержится {channels_count} каналов, а запрошен {Channel}");

                var sample_length = _Header.BlockAlign;
                var sample_data = new byte[sample_length];

                var data_length = _Header.FrameCount;

                var bytes_per_sample = _Header.BytesPerSample;
                var mask = (1 << _Header.BitsPerSample) - 1;
                for (var i = 0; i < data_length; i++)
                {
                    data_stream.Seek(Header.Length + i * sample_length, SeekOrigin.Begin);
                    data_stream.Read(sample_data, 0, sample_length);
                    var value = bytes_per_sample switch
                    {
                        1 => sample_data[Channel],
                        2 => BitConverter.ToInt16(sample_data, Channel * bytes_per_sample),
                        4 => BitConverter.ToInt32(sample_data, Channel * bytes_per_sample),
                        8 => BitConverter.ToInt64(sample_data, Channel * bytes_per_sample),
                        _ => throw new NotSupportedException($"Размерность отсчёта {bytes_per_sample} байт на канал не поддерживается")
                    };
                    yield return (i / (double)_Header.SampleRate, value);
                }
            }
            finally
            {
                if (!ReferenceEquals(data_stream, _DataStream))
                    data_stream?.Dispose();
            }
        }

        public override IEnumerable<(double Time, IReadOnlyList<long> Values)> EnumerateSamples()
        {
            Stream data_stream = null;
            try
            {
                data_stream = GetDataStream();

                var channels_count = _Header.ChannelsCount;

                var sample_length = _Header.BlockAlign;
                var sample_data = new byte[sample_length];

                var data_length = _Header.FrameCount;

                var bytes_per_sample = _Header.BytesPerSample;
                var mask = (1 << _Header.BitsPerSample) - 1;
                for (var i = 0; i < data_length; i++)
                {
                    var result = new long[channels_count];
                    data_stream.Seek(Header.Length + i * sample_length, SeekOrigin.Begin);
                    data_stream.Read(sample_data, 0, sample_length);
                    for (var channel = 0; channel < channels_count; channel++)
                        result[channel] = bytes_per_sample switch
                        {
                            1 => sample_data[channel],
                            2 => BitConverter.ToInt16(sample_data, channel * bytes_per_sample),
                            4 => BitConverter.ToInt32(sample_data, channel * bytes_per_sample),
                            8 => BitConverter.ToInt64(sample_data, channel * bytes_per_sample),
                            _ => throw new NotSupportedException($"Размерность отсчёта {bytes_per_sample} байт на канал не поддерживается")
                        };
                    yield return (i / (double)_Header.SampleRate, result);
                }
            }
            finally
            {
                if (!ReferenceEquals(data_stream, _DataStream))
                    data_stream?.Dispose();
            }
        }

        public override IEnumerable<(double Time, IReadOnlyList<long> Values)> EnumerateSamplesWithSingleArray()
        {
            Stream data_stream = null;
            try
            {
                data_stream = GetDataStream();

                var channels_count = _Header.ChannelsCount;

                var sample_length = _Header.BlockAlign;
                var sample_data = new byte[sample_length];

                var data_length = _Header.FrameCount;

                var bytes_per_sample = _Header.BytesPerSample;
                var result = new long[channels_count];
                var mask = (1 << _Header.BitsPerSample) - 1;
                for (var i = 0; i < data_length; i++)
                {
                    data_stream.Seek(Header.Length + i * sample_length, SeekOrigin.Begin);
                    data_stream.Read(sample_data, 0, sample_length);
                    for (var channel = 0; channel < channels_count; channel++)
                        result[channel] = bytes_per_sample switch
                        {
                            1 => sample_data[channel],
                            2 => BitConverter.ToInt16(sample_data, channel * bytes_per_sample),
                            4 => BitConverter.ToInt32(sample_data, channel * bytes_per_sample),
                            8 => BitConverter.ToInt64(sample_data, channel * bytes_per_sample),
                            _ => throw new NotSupportedException($"Размерность отсчёта {bytes_per_sample} байт на канал не поддерживается")
                        };
                    yield return (i / (double)_Header.SampleRate, result);
                }
            }
            finally
            {
                if (!ReferenceEquals(data_stream, _DataStream))
                    data_stream?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _Disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (_Disposed || !disposing) return;
            _Disposed = true;
            if (!_LeaveOpen)
                _DataStream.Dispose();
        }
    }
}