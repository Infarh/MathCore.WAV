using System;
using System.Collections.Generic;
using System.IO;

namespace MathCore.WAV
{
    public abstract class Wav
    {
        protected readonly Header _Header;

        /// <summary>полная длина файла в байтах включая заголовок</summary>
        public long DataLength => _Header.SubChunk2Size;

        public int SamplingFrequency => _Header.SampleRate;

        public int ByteRate => _Header.SampleRate;

        public short FrameLength => _Header.BlockAlign;

        public long FramesCount => _Header.FrameCount;

        public double dt => 1d / _Header.SampleRate;

        public double FileTimeLength => FramesCount * dt;

        public int ChannelsCount => _Header.ChannelsCount;

        public short SampleLength => (short)(((_Header.BitsPerSample - 1) >> 3) + 1);

        public virtual Frame this[int i]
        {
            get
            {
                using var data_stream = GetDataStream();
                var sample_length = _Header.BlockAlign;
                var data_offset = Header.Length + i * sample_length;
                if (i < 0 || data_offset >= data_stream.Length - sample_length)
                    throw new EndOfStreamException("Попытка чтения данных за пределами потока");

                var sample_data = new byte[sample_length];
                data_stream.Seek(data_offset, SeekOrigin.Begin);
                data_stream.Read(sample_data, 0, sample_length);
                return new Frame(i / (double)_Header.SampleRate, _Header.ChannelsCount, sample_data);
            }
        }

        protected Wav(Header Header) => _Header = Header;

        public abstract Stream GetDataStream();

        public long[] GetChannel(int Channel)
        {
            var channels_count = _Header.ChannelsCount;
            if (Channel >= channels_count)
                throw new ArgumentOutOfRangeException(nameof(Channel), Channel, $"В файле содержится {channels_count} каналов, а запрошен {Channel}");
            using var data_stream = GetDataStream();

            var sample_length = _Header.BlockAlign;
            var sample_data = new byte[sample_length];

            var data_length =_Header.FrameCount;
            var result = new long[data_length];

            var bytes_per_sample =_Header.BytesPerSample;
            var mask = (1 << _Header.BitsPerSample) - 1;
            for (var i = 0; i < data_length; i++)
            {
                data_stream.Seek(Header.Length + i * sample_length, SeekOrigin.Begin);
                data_stream.Read(sample_data, 0, sample_length);
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
            using var data_stream = GetDataStream();
            var channels_count = _Header.ChannelsCount;

            var sample_length = _Header.BlockAlign;
            var sample_data = new byte[sample_length];

            var data_length = _Header.FrameCount;
            var result = new long[channels_count][];
            for (var channel = 0; channel < channels_count; channel++)
                result[channel] = new long[data_length];

            var bytes_per_sample = _Header.BytesPerSample;
            var mask = (1 << _Header.BitsPerSample) - 1;
            for (var i = 0; i < data_length; i++)
            {
                data_stream.Seek(Header.Length + i * sample_length, SeekOrigin.Begin);
                data_stream.Read(sample_data, 0, sample_length);
                for (var channel = 0; channel < channels_count; channel++)
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

        public abstract IEnumerable<(double Time, long Value)> EnumerateSamples(int Channel);

        public abstract IEnumerable<(double Time, IReadOnlyList<long> Values)> EnumerateSamples();

        public abstract IEnumerable<(double Time, IReadOnlyList<long> Values)> EnumerateSamplesWithSingleArray();
    }
}
