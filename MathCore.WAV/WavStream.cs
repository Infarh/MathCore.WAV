#nullable enable
using System.Runtime.CompilerServices;

// ReSharper disable UnusedType.Global
namespace MathCore.WAV;

/// <summary>Объект чтения данных WAV в формате PCM из потока</summary>
/// <remarks>Инициализация нового экземпляра <see cref="WavStream"/></remarks>
/// <param name="DataStream">Поток байт данных, из которого требуется выполнять чтение</param>
/// <param name="LeaveOpen">Признак того, что при вызове метода <see cref="Dispose()"/> поток <paramref name="DataStream"/> закрывать не требуется</param>
public class WavStream(Stream DataStream, bool LeaveOpen = false) : Wav(Header.Load(DataStream ?? throw new ArgumentNullException(nameof(DataStream)))), IDisposable
{
    /* ------------------------------------------------------------------------------------- */

    /// <summary>Поток байт данных, из которого требуется выполнять чтение</summary>
    private readonly Stream _DataStream = DataStream;

    /// <summary>Признак того, что при вызове метода <see cref="Dispose()"/> поток <see cref="_DataStream"/> закрывать не требуется</summary>
    private readonly bool _LeaveOpen = LeaveOpen;

    /* ------------------------------------------------------------------------------------- */

    /// <inheritdoc />
    public override Frame this[int i]
    {
        get
        {
            if (!_DataStream.CanSeek)
                throw new NotSupportedException("Поток данных не поддерживает возможностей поиска положения в нём");

            var sample_length = _Header.BlockAlign;
            var data_offset   = Header.Length + i * sample_length;
            if (i < 0 || data_offset + sample_length > _DataStream.Length)
                throw new EndOfStreamException("Попытка чтения данных за пределами потока");

            var sample_data = new byte[sample_length];
            _DataStream.Seek(data_offset, SeekOrigin.Begin);

            if (_DataStream.Read(sample_data, 0, sample_length) != sample_length)
                throw new InvalidOperationException($"Ошибка чтения файла при загрузке данных {i} фрейма");

            return new(i / (double)_Header.SampleRate, _Header.ChannelsCount, sample_data);
        }
    }

    /* ------------------------------------------------------------------------------------- */

    /// <summary>
    /// Если поток является файловым, то файл открывается вновь.<br/>
    /// Если в потоке можно выполнять перемещение, то положение в потоке изменяется на 44 байт (конец заголовка).
    /// Иначе возвращается <see cref="_DataStream"/>
    /// </summary>
    /// <returns>Поток для чтения данных</returns>
    public override Stream GetDataStream()
    {
        if (_DataStream is FileStream file)
        {
            var stream = new FileStream(file.Name, FileMode.Open, FileAccess.Read, FileShare.Read);
            stream.Seek(Header.Length, SeekOrigin.Begin);
            return stream;
        }

        if (_DataStream.CanSeek)
            _DataStream.Seek(Header.Length, SeekOrigin.Begin);

        return _DataStream;
    }

    /// <inheritdoc />
    public override IEnumerable<(double Time, long Value)> EnumerateSamples(int Channel)
    {
        Stream? data_stream = null;
        try
        {
            data_stream = GetDataStream();

            Channel = ValidateChannelIndex(Channel);

            var sample_length = _Header.BlockAlign;
            var sample_data   = new byte[sample_length];

            var data_length = _Header.FrameCount;

            var    bytes_per_sample = _Header.BytesPerSample;
            double sample_rate      = _Header.SampleRate;
            for (var i = 0; i < data_length; i++)
            {
                if (data_stream.FillBuffer(sample_data) != sample_length)
                    yield break;

                yield return (i / sample_rate, ReadChannelValue(sample_data, Channel, bytes_per_sample));
            }
        }
        finally
        {
            if (!ReferenceEquals(data_stream, _DataStream))
                data_stream?.Dispose();
        }
    }

    /// <inheritdoc />
    public override async IAsyncEnumerable<(double Time, long Value)> EnumerateSamplesAsync(
        int Channel,
        IProgress<double>? Progress = null,
        [EnumeratorCancellation] CancellationToken Cancel = default)
    {
        Cancel.ThrowIfCancellationRequested();
        Stream? data_stream = null;
        try
        {
            data_stream = GetDataStream();

            Channel = ValidateChannelIndex(Channel);

            var sample_length = _Header.BlockAlign;
            var sample_data   = new byte[sample_length];

            var data_length = _Header.FrameCount;

            var    bytes_per_sample = _Header.BytesPerSample;
            double sample_rate      = _Header.SampleRate;
            for (var i = 0; i < data_length; i++)
            {
                Cancel.ThrowIfCancellationRequested();
                if (await data_stream.FillBufferAsync(sample_data, Cancel).ConfigureAwait(false) != sample_length)
                    yield break;

                Progress?.Report((double)i / data_length);
                yield return (i / sample_rate, ReadChannelValue(sample_data, Channel, bytes_per_sample));
            }
        }
        finally
        {
            if (!ReferenceEquals(data_stream, _DataStream))
                data_stream?.Dispose();
        }
    }

    /// <inheritdoc />
    public override IEnumerable<(double Time, IReadOnlyList<long> Values)> EnumerateSamples()
    {
        Stream? data_stream = null;
        try
        {
            data_stream = GetDataStream();

            var channels_count = _Header.ChannelsCount;

            var sample_length = _Header.BlockAlign;
            var sample_data   = new byte[sample_length];

            var data_length = _Header.FrameCount;

            var    bytes_per_sample = _Header.BytesPerSample;
            double sample_rate      = _Header.SampleRate;
            for (var i = 0; i < data_length; i++)
            {
                var result = new long[channels_count];
                if (data_stream.FillBuffer(sample_data) != sample_length)
                    yield break;

                for (var channel = 0; channel < channels_count; channel++)
                    result[channel] = ReadChannelValue(sample_data, channel, bytes_per_sample);

                yield return (i / sample_rate, result);
            }
        }
        finally
        {
            if (!ReferenceEquals(data_stream, _DataStream))
                data_stream?.Dispose();
        }
    }

    /// <inheritdoc />
    public override async IAsyncEnumerable<(double Time, IReadOnlyList<long> Values)> EnumerateSamplesAsync(
        IProgress<double>? Progress = null,
        [EnumeratorCancellation] CancellationToken Cancel = default)
    {
        Cancel.ThrowIfCancellationRequested();
        Stream? data_stream = null;
        try
        {
            data_stream = GetDataStream();

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
                if (await data_stream.FillBufferAsync(sample_data, Cancel).ConfigureAwait(false) != sample_length)
                    yield break;

                for (var channel = 0; channel < channels_count; channel++)
                    result[channel] = ReadChannelValue(sample_data, channel, bytes_per_sample);

                Progress?.Report((double)i / data_length);
                yield return (i / sample_rate, result);
            }
        }
        finally
        {
            if (!ReferenceEquals(data_stream, _DataStream))
                data_stream?.Dispose();
        }
    }

    /// <inheritdoc />
    public override IEnumerable<(double Time, IReadOnlyList<long> Values)> EnumerateSamplesWithSingleArray()
    {
        Stream? data_stream = null;
        try
        {
            data_stream = GetDataStream();

            var channels_count = _Header.ChannelsCount;

            var sample_length = _Header.BlockAlign;
            var sample_data   = new byte[sample_length];

            var data_length = _Header.FrameCount;

            var    bytes_per_sample = _Header.BytesPerSample;
            var    result           = new long[channels_count];
            double sample_rate      = _Header.SampleRate;
            for (var i = 0; i < data_length; i++)
            {
                if (data_stream.FillBuffer(sample_data) != sample_length)
                    yield break;

                for (var channel = 0; channel < channels_count; channel++)
                    result[channel] = ReadChannelValue(sample_data, channel, bytes_per_sample);

                yield return (i / sample_rate, result);
            }
        }
        finally
        {
            if (!ReferenceEquals(data_stream, _DataStream))
                data_stream?.Dispose();
        }
    }

    /// <inheritdoc />
    public override async IAsyncEnumerable<(double Time, IReadOnlyList<long> Values)> EnumerateSamplesWithSingleArrayAsync(
        IProgress<double>? Progress = null,
        [EnumeratorCancellation] CancellationToken Cancel = default)
    {
        Cancel.ThrowIfCancellationRequested();
        Stream? data_stream = null;
        try
        {
            data_stream = GetDataStream();

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
                if (await data_stream.FillBufferAsync(sample_data, Cancel).ConfigureAwait(false) != sample_length)
                    yield break;

                for (var channel = 0; channel < channels_count; channel++)
                    result[channel] = ReadChannelValue(sample_data, channel, bytes_per_sample);

                Progress?.Report((double)i / data_length);
                yield return (i / sample_rate, result);
            }
        }
        finally
        {
            if (!ReferenceEquals(data_stream, _DataStream))
                data_stream?.Dispose();
        }
    }

    /* ------------------------------------------------------------------------------------- */

    #region IDisposable

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Признак того, что объект разрушен</summary>
    private bool _Disposed;

    /// <summary>Закрывает поток, если это необходимо</summary>
    /// <param name="disposing">Выполнить освобождение управляемых ресурсов?</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_Disposed || !disposing) return;
        _Disposed = true;
        if (!_LeaveOpen)
            _DataStream.Dispose();
    }

    #endregion

    /* ------------------------------------------------------------------------------------- */
}