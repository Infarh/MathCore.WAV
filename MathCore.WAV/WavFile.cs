using System;
using System.Collections.Generic;
using System.IO;
using MathCore.WAV.Service;

// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable UnusedMember.Global

namespace MathCore.WAV
{
    public class WavFile : Wav//, IEnumerable<Frame>
    {
        public FileInfo File { get; }

        public long FullLength => File.Length;


        public WavFile(string FileName) : this(new FileInfo(FileName ?? throw new ArgumentNullException(nameof(FileName)))) { }

        public WavFile(FileInfo File) : base(File.OpenRead().Using(Header.Load)) => this.File = File;

        public override Stream GetDataStream()
        {
            var stream = File.OpenRead();
            stream.Seek(Header.Length, SeekOrigin.Begin);
            return stream;
        }

        public override IEnumerable<(double Time, long Value)> EnumerateSamples(int Channel)
        {
            using var data_stream = GetDataStream();

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

        public override IEnumerable<(double Time, IReadOnlyList<long> Values)> EnumerateSamples()
        {
            using var data_stream = GetDataStream();

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

        public override IEnumerable<(double Time, IReadOnlyList<long> Values)> EnumerateSamplesWithSingleArray()
        {
            using var data_stream = GetDataStream();

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
    }
}
