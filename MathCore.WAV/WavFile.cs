using System.Runtime.CompilerServices;

// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable UnusedMember.Global
namespace MathCore.WAV;

/// <summary>Объект для чтения wav-файла в формате PCM</summary>
public class WavFile : Wav
{
    /* ------------------------------------------------------------------------------------- */

    /// <summary>Читаемый файл данных</summary>
    public FileInfo File { get; }

    /// <summary>Длина файла</summary>
    public long FullLength => File.Length;

    /* ------------------------------------------------------------------------------------- */

    /// <summary>Инициализация нового экземпляра <see cref="WavFile"/></summary>
    /// <param name="FileName">Имя файла с данными для чтения</param>
    public WavFile(string FileName) : this(new FileInfo(FileName ?? throw new ArgumentNullException(nameof(FileName)))) { }

    /// <summary>Инициализация нового экземпляра <see cref="WavFile"/></summary>
    /// <param name="File">файл данных для чтения</param>
    public WavFile(FileInfo File) : base(File.OpenRead().Using(Header.Load)) => this.File = File;

    /// <summary>Открывает файловый поток и переходит к 44 байту (началу блока данных)</summary>
    /// <returns>Файловый поток для чтения данных</returns>
    protected override Stream GetDataStream()
    {
        var       stream = File.OpenRead();
        const int length = Header.Length;
        if (stream.Seek(length, SeekOrigin.Begin) != length)
            throw new InvalidOperationException($"Не удалось перейти в файле к смещению {length}");
        return stream;
    }

    /// <inheritdoc />
    public override IEnumerable<(double Time, long Value)> EnumerateSamples(int Channel)
    {
        using var data_stream = GetDataStream();

        var channels_count = _Header.ChannelsCount;
        if (Channel >= channels_count)
            throw new ArgumentOutOfRangeException(nameof(Channel), Channel, $"В файле содержится {channels_count} каналов, а запрошен {Channel}");

        var sample_length = _Header.BlockAlign;
        var sample_data   = new byte[sample_length];

        var data_length = _Header.FrameCount;

        var    bytes_per_sample = _Header.BytesPerSample;
        double sample_rate      = _Header.SampleRate;
        for (var i = 0; i < data_length; i++)
        {
            data_stream.Seek(Header.Length + i * sample_length, SeekOrigin.Begin);
            if (data_stream.FeelBuffer(sample_data) != sample_length)
                yield break;

            var value = bytes_per_sample switch
            {
                1 => sample_data[Channel],
                2 => BitConverter.ToInt16(sample_data, Channel * bytes_per_sample),
                4 => BitConverter.ToInt32(sample_data, Channel * bytes_per_sample),
                8 => BitConverter.ToInt64(sample_data, Channel * bytes_per_sample),
                _ => throw new NotSupportedException($"Размерность отсчёта {bytes_per_sample} байт на канал не поддерживается")
            };

            yield return (i / sample_rate, value);
        }
    }

    /// <inheritdoc />
    public override async IAsyncEnumerable<(double Time, long Value)> EnumerateSamplesAsync(
        int Channel,
        IProgress<double> Progress = null,
        [EnumeratorCancellation] CancellationToken Cancel = default)
    {
        Cancel.ThrowIfCancellationRequested();
        using var data_stream = GetDataStream();

        var channels_count = _Header.ChannelsCount;
        if (Channel >= channels_count)
            throw new ArgumentOutOfRangeException(nameof(Channel), Channel, $"В файле содержится {channels_count} каналов, а запрошен {Channel}");

        var sample_length = _Header.BlockAlign;
        var sample_data   = new byte[sample_length];

        var data_length = _Header.FrameCount;

        var    bytes_per_sample = _Header.BytesPerSample;
        double sample_rate      = _Header.SampleRate;
        for (var i = 0; i < data_length; i++)
        {
            Cancel.ThrowIfCancellationRequested();
            data_stream.Seek(Header.Length + i * sample_length, SeekOrigin.Begin);
            if (await data_stream.FeelBufferAsync(sample_data, Cancel).ConfigureAwait(false) != sample_length)
                yield break;

            var value = bytes_per_sample switch
            {
                1 => sample_data[Channel],
                2 => BitConverter.ToInt16(sample_data, Channel * bytes_per_sample),
                4 => BitConverter.ToInt32(sample_data, Channel * bytes_per_sample),
                8 => BitConverter.ToInt64(sample_data, Channel * bytes_per_sample),
                _ => throw new NotSupportedException($"Размерность отсчёта {bytes_per_sample} байт на канал не поддерживается")
            };

            Progress?.Report((double)i / data_length);
            yield return (i / sample_rate, value);
        }
    }

    /// <inheritdoc />
    public override IEnumerable<(double Time, IReadOnlyList<long> Values)> EnumerateSamples()
    {
        using var data_stream = GetDataStream();

        var channels_count = _Header.ChannelsCount;

        var sample_length = _Header.BlockAlign;
        var sample_data   = new byte[sample_length];

        var data_length = _Header.FrameCount;

        var    bytes_per_sample = _Header.BytesPerSample;
        double sample_rate      = _Header.SampleRate;
        for (var i = 0; i < data_length; i++)
        {
            var result = new long[channels_count];
            data_stream.Seek(Header.Length + i * sample_length, SeekOrigin.Begin);
            if (data_stream.FeelBuffer(sample_data) != sample_length)
                yield break;

            for (var channel = 0; channel < channels_count; channel++)
                result[channel] = bytes_per_sample switch
                {
                    1 => sample_data[channel],
                    2 => BitConverter.ToInt16(sample_data, channel * bytes_per_sample),
                    4 => BitConverter.ToInt32(sample_data, channel * bytes_per_sample),
                    8 => BitConverter.ToInt64(sample_data, channel * bytes_per_sample),
                    _ => throw new NotSupportedException($"Размерность отсчёта {bytes_per_sample} байт на канал не поддерживается")
                };

            yield return (i / sample_rate, result);
        }
    }

    /// <inheritdoc />
    public override async IAsyncEnumerable<(double Time, IReadOnlyList<long> Values)> EnumerateSamplesAsync(
        IProgress<double> Progress = null,
        [EnumeratorCancellation] CancellationToken Cancel = default)
    {
        Cancel.ThrowIfCancellationRequested();
        using var data_stream = GetDataStream();

        var channels_count = _Header.ChannelsCount;

        var sample_length = _Header.BlockAlign;
        var sample_data   = new byte[sample_length];

        var data_length = _Header.FrameCount;

        var    bytes_per_sample = _Header.BytesPerSample;
        double sample_rate      = _Header.SampleRate;
        for (var i = 0; i < data_length; i++)
        {
            Cancel.ThrowIfCancellationRequested();
            var result = new long[channels_count];
            data_stream.Seek(Header.Length + i * sample_length, SeekOrigin.Begin);
            if (await data_stream.FeelBufferAsync(sample_data, Cancel).ConfigureAwait(false) != sample_length)
                yield break;

            for (var channel = 0; channel < channels_count; channel++)
                result[channel] = bytes_per_sample switch
                {
                    1 => sample_data[channel],
                    2 => BitConverter.ToInt16(sample_data, channel * bytes_per_sample),
                    4 => BitConverter.ToInt32(sample_data, channel * bytes_per_sample),
                    8 => BitConverter.ToInt64(sample_data, channel * bytes_per_sample),
                    _ => throw new NotSupportedException($"Размерность отсчёта {bytes_per_sample} байт на канал не поддерживается")
                };

            Progress?.Report((double)i / data_length);
            yield return (i / sample_rate, result);
        }
    }

    /// <inheritdoc />
    public override IEnumerable<(double Time, IReadOnlyList<long> Values)> EnumerateSamplesWithSingleArray()
    {
        using var data_stream = GetDataStream();

        var channels_count = _Header.ChannelsCount;

        var sample_length = _Header.BlockAlign;
        var sample_data   = new byte[sample_length];

        var data_length = _Header.FrameCount;

        var    bytes_per_sample = _Header.BytesPerSample;
        var    result           = new long[channels_count];
        double sample_rate      = _Header.SampleRate;
        for (var i = 0; i < data_length; i++)
        {
            data_stream.Seek(Header.Length + i * sample_length, SeekOrigin.Begin);
            if (data_stream.FeelBuffer(sample_data) != sample_length)
                yield break;

            for (var channel = 0; channel < channels_count; channel++)
                result[channel] = bytes_per_sample switch
                {
                    1 => sample_data[channel],
                    2 => BitConverter.ToInt16(sample_data, channel * bytes_per_sample),
                    4 => BitConverter.ToInt32(sample_data, channel * bytes_per_sample),
                    8 => BitConverter.ToInt64(sample_data, channel * bytes_per_sample),
                    _ => throw new NotSupportedException($"Размерность отсчёта {bytes_per_sample} байт на канал не поддерживается")
                };

            yield return (i / (double)sample_rate, result);
        }
    }

    /// <inheritdoc />
    public override async IAsyncEnumerable<(double Time, IReadOnlyList<long> Values)> EnumerateSamplesWithSingleArrayAsync(
        IProgress<double> Progress = null,
        [EnumeratorCancellation] CancellationToken Cancel = default)
    {
        Cancel.ThrowIfCancellationRequested();
        using var data_stream = GetDataStream();

        var channels_count = _Header.ChannelsCount;

        var sample_length = _Header.BlockAlign;
        var sample_data   = new byte[sample_length];

        var data_length = _Header.FrameCount;

        var    bytes_per_sample = _Header.BytesPerSample;
        var    result           = new long[channels_count];
        double sample_rate      = _Header.SampleRate;
        for (var i = 0; i < data_length; i++)
        {
            Cancel.ThrowIfCancellationRequested();
            data_stream.Seek(Header.Length + i * sample_length, SeekOrigin.Begin);
            if (await data_stream.FeelBufferAsync(sample_data, Cancel).ConfigureAwait(false) != sample_length)
                yield break;

            for (var channel = 0; channel < channels_count; channel++)
                result[channel] = bytes_per_sample switch
                {
                    1 => sample_data[channel],
                    2 => BitConverter.ToInt16(sample_data, channel * bytes_per_sample),
                    4 => BitConverter.ToInt32(sample_data, channel * bytes_per_sample),
                    8 => BitConverter.ToInt64(sample_data, channel * bytes_per_sample),
                    _ => throw new NotSupportedException($"Размерность отсчёта {bytes_per_sample} байт на канал не поддерживается")
                };

            Progress?.Report((double)i / data_length);
            yield return (i / sample_rate, result);
        }
    }

    /* ------------------------------------------------------------------------------------- */
}