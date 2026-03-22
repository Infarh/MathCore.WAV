// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
namespace MathCore.WAV;

/// <summary>Объект для чтения данных WAV в формате PCM</summary>
public abstract class Wav
{
    /* ------------------------------------------------------------------------------------- */

    public static long ValueToSample(double Value, double Resolution) => (long)Math.Round(Value * Resolution);

    public static long ValueToSample(decimal Value, decimal Resolution) => (long)Math.Round(Value * Resolution);

    public static double SampleToValue(long Sample, double Resolution) => Sample * Resolution;

    public static decimal SampleToValue(long Sample, decimal Resolution) => Sample * Resolution;

    /* ------------------------------------------------------------------------------------- */

    /// <summary>Заголовок файла</summary>
    protected readonly Header _Header;

    /* ------------------------------------------------------------------------------------- */

    /// <summary>Полная длина данных в байтах (без заголовка)</summary>
    public long DataLength => _Header.SubChunk2Size;

    /// <summary>Частота дискретизации</summary>
    public int SampleRate => _Header.SampleRate;

    /// <summary>Байт на один отсчёт</summary>
    public int BytesPerSample => _Header.BytesPerSample;

    /// <summary>Количество бит в семпле (8, 16, 32, 64...)</summary>
    public short BitsPerSample => _Header.BitsPerSample;

    /// <summary>Количество байт на один фрейм (один отсчёт по всем каналам)</summary>
    public short FrameLength => _Header.BlockAlign;

    /// <summary>Число фреймов в файле</summary>
    public long FramesCount => _Header.FrameCount;

    /// <summary>Период дискретизации</summary>
    public double dt => 1d / _Header.SampleRate;

    /// <summary>Длина файла в секундах</summary>
    public double FileTimeLength => _Header.TimeLengthInSeconds;

    public TimeSpan FileTime => TimeSpan.FromSeconds(FileTimeLength);

    /// <summary>Количество каналов</summary>
    public int ChannelsCount => _Header.ChannelsCount;

    private double _valuesOffset;
    /// <summary>Смещение центра интервала физической величины</summary>
    public double ValuesOffset
    {
        get => _valuesOffset;
        set
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new ArgumentOutOfRangeException(nameof(value), value, "Некорректное значение смещения");
            _valuesOffset = value;
        }
    }

    private double _amplitude = double.NaN;
    /// <summary>Амплитуда физической величины</summary>
    public double Amplitude
    {
        get => _amplitude;
        set
        {
            if (value <= double.Epsilon)
                throw new ArgumentOutOfRangeException(nameof(value), value, "Требуется положительное значение");
            _amplitude = value;
        }
    }

    private static long GetChannelAmplitude(short BitsPerSample) => BitsPerSample switch
    {
        64 => long.MaxValue,
        > 0 and < 64 => (1L << (BitsPerSample - 1)) - 1,
        _ => throw new NotSupportedException($"Размерность отсчёта {BitsPerSample} бит на канал не поддерживается")
    };

    protected static long ReadChannelValue(byte[] SampleData, int Channel, int BytesPerSample) => BytesPerSample switch
    {
        1 => SampleData[Channel],
        2 => BitConverter.ToInt16(SampleData, Channel * BytesPerSample),
        4 => BitConverter.ToInt32(SampleData, Channel * BytesPerSample),
        8 => BitConverter.ToInt64(SampleData, Channel * BytesPerSample),
        _ => throw new NotSupportedException($"Размерность отсчёта {BytesPerSample} байт на канал не поддерживается")
    };

    protected int ValidateChannelIndex(int Channel)
    {
        var channels_count = _Header.ChannelsCount;
        if (Channel < 0 || Channel >= channels_count)
            throw new ArgumentOutOfRangeException(nameof(Channel), Channel, $"В файле содержится {channels_count} каналов, а запрошен {Channel}");
        return Channel;
    }

    /// <summary>Амплитуда канала</summary>
    public long ChannelAmplitude => GetChannelAmplitude(_Header.BitsPerSample);

    public double ChannelResolution => Amplitude / ChannelAmplitude;

    public double AmplitudeResolution => ChannelAmplitude / Amplitude;

    /// <summary>Индексатор фреймов</summary>
    /// <param name="i">Номер отсчёта в потоке</param>
    /// <returns>Фрейм со значениями всех каналов</returns>
    /// <exception cref="EndOfStreamException">Если индекс фрейма выходит за пределы данных</exception>
    public virtual Frame this[int i] => TryGetFrame(i, out var frame)
        ? frame
        : throw new EndOfStreamException("Попытка чтения данных за пределами потока");

    /// <summary>Попытка получить фрейм по индексу без генерации исключения при выходе за границы данных</summary>
    /// <param name="Index">Индекс фрейма</param>
    /// <param name="Frame">Найденный фрейм</param>
    /// <returns>Истина, если фрейм прочитан успешно</returns>
    public virtual bool TryGetFrame(int Index, out Frame Frame)
    {
        using var data_stream = GetDataStream();

        var sample_length = _Header.BlockAlign;
        var data_offset = Header.Length + Index * sample_length;
        if (Index < 0 || data_offset > data_stream.Length - sample_length)
        {
            Frame = default;
            return false;
        }

        var sample_data = new byte[sample_length];
        data_stream.Seek(data_offset, SeekOrigin.Begin);
        if (data_stream.FillBuffer(sample_data) != sample_length)
            throw new InvalidOperationException($"Ошибка чтения файла при загрузке данных {Index} фрейма");

        Frame = new(Index / (double)_Header.SampleRate, _Header.ChannelsCount, sample_data);
        return true;
    }

    /* ------------------------------------------------------------------------------------- */

    /// <summary>Инициализатор данных <see cref="Wav"/></summary>
    /// <param name="Header">Заголовок <see cref="Header"/></param>
    protected Wav(Header Header) => _Header = Header;

    /* ------------------------------------------------------------------------------------- */

    /// <summary>Получить поток байт данных для осуществления процедуры чтения</summary>
    /// <returns>Поток байт данных WAV</returns>
    public abstract Stream GetDataStream();

    /// <summary>Прочитать значения отсчётов канала в буфер</summary>
    /// <param name="Channel">Номер канала</param>
    /// <param name="Buffer">Буфер значений канала</param>
    /// <param name="Offset">Смещение в буфере</param>
    /// <returns>Количество прочитанных отсчётов</returns>
    public int ReadChannel(int Channel, long[] Buffer, int Offset = 0)
    {
        if (Buffer is null)
            throw new ArgumentNullException(nameof(Buffer));

        if (Offset < 0 || Offset > Buffer.Length)
            throw new ArgumentOutOfRangeException(nameof(Offset), Offset, "Смещение должно быть в диапазоне буфера");

        Channel = ValidateChannelIndex(Channel);

        var frames_to_read = Math.Min(_Header.FrameCount, Buffer.Length - Offset);
        if (frames_to_read <= 0)
            return 0;

        using var data_stream = GetDataStream();

        var sample_length = _Header.BlockAlign;
        var sample_data = new byte[sample_length];
        var bytes_per_sample = _Header.BytesPerSample;

        for (var i = 0; i < frames_to_read; i++)
        {
            if (data_stream.FillBuffer(sample_data) != sample_length)
                throw new InvalidOperationException($"Ошибка чтения файла при загрузке данных {i} фрейма");

            Buffer[Offset + i] = ReadChannelValue(sample_data, Channel, bytes_per_sample);
        }

        return frames_to_read;
    }

    /// <summary>Асинхронно прочитать значения отсчётов канала в буфер</summary>
    /// <param name="Channel">Номер канала</param>
    /// <param name="Buffer">Буфер значений канала</param>
    /// <param name="Offset">Смещение в буфере</param>
    /// <param name="Progress">Объект информирования о прогрессе</param>
    /// <param name="Cancel">Признак отмены операции</param>
    /// <returns>Количество прочитанных отсчётов</returns>
    public async Task<int> ReadChannelAsync(int Channel, long[] Buffer, int Offset = 0, IProgress<double> Progress = null, CancellationToken Cancel = default)
    {
        if (Buffer is null)
            throw new ArgumentNullException(nameof(Buffer));

        if (Offset < 0 || Offset > Buffer.Length)
            throw new ArgumentOutOfRangeException(nameof(Offset), Offset, "Смещение должно быть в диапазоне буфера");

        Cancel.ThrowIfCancellationRequested();
        Channel = ValidateChannelIndex(Channel);

        var frames_to_read = Math.Min(_Header.FrameCount, Buffer.Length - Offset);
        if (frames_to_read <= 0)
            return 0;

        using var data_stream = GetDataStream();

        var sample_length = _Header.BlockAlign;
        var sample_data = new byte[sample_length];
        var bytes_per_sample = _Header.BytesPerSample;

        for (var i = 0; i < frames_to_read; i++)
        {
            Cancel.ThrowIfCancellationRequested();
            if (await data_stream.FillBufferAsync(sample_data, Cancel).ConfigureAwait(false) != sample_length)
                throw new InvalidOperationException($"Ошибка чтения файла при загрузке данных {i} фрейма");

            Buffer[Offset + i] = ReadChannelValue(sample_data, Channel, bytes_per_sample);
            Progress?.Report((double)i / frames_to_read);
        }

        return frames_to_read;
    }

    /// <summary>Прочитать значения отсчётов всех каналов в буферы</summary>
    /// <param name="Buffers">Буферы каналов</param>
    /// <param name="Offset">Смещение в буферах</param>
    /// <returns>Количество прочитанных фреймов</returns>
    public int ReadChannels(long[][] Buffers, int Offset = 0)
    {
        if (Buffers is null)
            throw new ArgumentNullException(nameof(Buffers));

        var channels_count = _Header.ChannelsCount;
        if (Buffers.Length != channels_count)
            throw new ArgumentException($"Число буферов должно быть равно числу каналов {channels_count}", nameof(Buffers));

        if (Offset < 0)
            throw new ArgumentOutOfRangeException(nameof(Offset), Offset, "Смещение должно быть неотрицательным");

        var frames_to_read = _Header.FrameCount;
        for (var channel = 0; channel < channels_count; channel++)
        {
            var buffer = Buffers[channel];
            if (buffer is null)
                throw new ArgumentNullException(nameof(Buffers), $"Буфер канала {channel} не задан");

            if (Offset > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(Offset), Offset, "Смещение должно быть в диапазоне каждого буфера");

            frames_to_read = Math.Min(frames_to_read, buffer.Length - Offset);
        }

        if (frames_to_read <= 0)
            return 0;

        using var data_stream = GetDataStream();

        var sample_length = _Header.BlockAlign;
        var sample_data = new byte[sample_length];
        var bytes_per_sample = _Header.BytesPerSample;

        for (var i = 0; i < frames_to_read; i++)
        {
            if (data_stream.FillBuffer(sample_data) != sample_length)
                throw new InvalidOperationException($"Ошибка чтения файла при загрузке данных {i} фрейма");

            for (var channel = 0; channel < channels_count; channel++)
                Buffers[channel][Offset + i] = ReadChannelValue(sample_data, channel, bytes_per_sample);
        }

        return frames_to_read;
    }

    /// <summary>Асинхронно прочитать значения отсчётов всех каналов в буферы</summary>
    /// <param name="Buffers">Буферы каналов</param>
    /// <param name="Offset">Смещение в буферах</param>
    /// <param name="Progress">Объект информирования о прогрессе</param>
    /// <param name="Cancel">Признак отмены операции</param>
    /// <returns>Количество прочитанных фреймов</returns>
    public async Task<int> ReadChannelsAsync(long[][] Buffers, int Offset = 0, IProgress<double> Progress = null, CancellationToken Cancel = default)
    {
        if (Buffers is null)
            throw new ArgumentNullException(nameof(Buffers));

        var channels_count = _Header.ChannelsCount;
        if (Buffers.Length != channels_count)
            throw new ArgumentException($"Число буферов должно быть равно числу каналов {channels_count}", nameof(Buffers));

        if (Offset < 0)
            throw new ArgumentOutOfRangeException(nameof(Offset), Offset, "Смещение должно быть неотрицательным");

        Cancel.ThrowIfCancellationRequested();

        var frames_to_read = _Header.FrameCount;
        for (var channel = 0; channel < channels_count; channel++)
        {
            var buffer = Buffers[channel];
            if (buffer is null)
                throw new ArgumentNullException(nameof(Buffers), $"Буфер канала {channel} не задан");

            if (Offset > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(Offset), Offset, "Смещение должно быть в диапазоне каждого буфера");

            frames_to_read = Math.Min(frames_to_read, buffer.Length - Offset);
        }

        if (frames_to_read <= 0)
            return 0;

        using var data_stream = GetDataStream();

        var sample_length = _Header.BlockAlign;
        var sample_data = new byte[sample_length];
        var bytes_per_sample = _Header.BytesPerSample;

        for (var i = 0; i < frames_to_read; i++)
        {
            Cancel.ThrowIfCancellationRequested();
            if (await data_stream.FillBufferAsync(sample_data, Cancel).ConfigureAwait(false) != sample_length)
                throw new InvalidOperationException($"Ошибка чтения файла при загрузке данных {i} фрейма");

            for (var channel = 0; channel < channels_count; channel++)
                Buffers[channel][Offset + i] = ReadChannelValue(sample_data, channel, bytes_per_sample);

            Progress?.Report((double)i / frames_to_read);
        }

        return frames_to_read;
    }

    /// <summary>Прочитать все значения отсчётов канала</summary>
    /// <param name="Channel">Номер канала</param>
    /// <returns>Массив отсчётов канала</returns>
    public long[] GetChannel(int Channel)
    {
        Channel = ValidateChannelIndex(Channel);

        var data_length = _Header.FrameCount;
        var result = new long[data_length];
        _ = ReadChannel(Channel, result);
        return result;
    }

    public double[] GetChannelDouble(int Channel)
    {
        Channel = ValidateChannelIndex(Channel);
        using var data_stream = GetDataStream();

        var sample_length = _Header.BlockAlign;
        var sample_data   = new byte[sample_length];

        var data_length = _Header.FrameCount;
        var result      = new double[data_length];

        var bytes_per_sample = _Header.BytesPerSample;
        var resolution       = ChannelResolution;
        if (double.IsNaN(resolution))
            for (var i = 0; i < data_length; i++)
            {
                if (data_stream.FillBuffer(sample_data) != sample_length)
                    throw new InvalidOperationException($"Ошибка чтения файла при загрузке данных {i} фрейма");
                result[i] = ReadChannelValue(sample_data, Channel, bytes_per_sample);
            }
        else
            for (var i = 0; i < data_length; i++)
            {
                if (data_stream.FillBuffer(sample_data) != sample_length)
                    throw new InvalidOperationException($"Ошибка чтения файла при загрузке данных {i} фрейма");
                result[i] = SampleToValue(ReadChannelValue(sample_data, Channel, bytes_per_sample), resolution);
            }
        return result;
    }

    public decimal[] GetChannelDecimal(int Channel)
    {
        Channel = ValidateChannelIndex(Channel);
        using var data_stream = GetDataStream();

        var sample_length = _Header.BlockAlign;
        var sample_data   = new byte[sample_length];

        var data_length = _Header.FrameCount;
        var result      = new decimal[data_length];

        var bytes_per_sample   = _Header.BytesPerSample;
        var channel_resolution = ChannelResolution;
        var resolution         = (decimal)channel_resolution;
        if (double.IsNaN(channel_resolution))
            for (var i = 0; i < data_length; i++)
            {
                if (data_stream.FillBuffer(sample_data) != sample_length)
                    throw new InvalidOperationException($"Ошибка чтения файла при загрузке данных {i} фрейма");
                result[i] = ReadChannelValue(sample_data, Channel, bytes_per_sample);
            }
        else
            for (var i = 0; i < data_length; i++)
            {
                if (data_stream.FillBuffer(sample_data) != sample_length)
                    throw new InvalidOperationException($"Ошибка чтения файла при загрузке данных {i} фрейма");
                result[i] = SampleToValue(ReadChannelValue(sample_data, Channel, bytes_per_sample), resolution);
            }
        return result;
    }

    /// <summary>Асинхронно прочитать все значения отсчётов канала</summary>
    /// <param name="Channel">Номер канала</param>
    /// <param name="Progress">Объект информирования о прогрессе чтения в интервале значений от 0 до 1</param>
    /// <param name="Cancel">Признак отмены асинхронной операции чтения</param>
    /// <returns>Задача, возвращающая массив отсчётов канала</returns>
    public async Task<long[]> GetChannelAsync(int Channel, IProgress<double> Progress = null, CancellationToken Cancel = default)
    {
        Cancel.ThrowIfCancellationRequested();
        Channel = ValidateChannelIndex(Channel);

        var data_length = _Header.FrameCount;
        var result = new long[data_length];
        _ = await ReadChannelAsync(Channel, result, Progress: Progress, Cancel: Cancel).ConfigureAwait(false);
        return result;
    }

    /// <summary>Получить массивы значений отсчётов всех каналов</summary>
    /// <returns>Массив массивов значений всех каналов</returns>
    /// <exception cref="InvalidOperationException">Если при чтении очередного значения будет число прочитанных байт не будет равно размеру одного кадра</exception>
    public long[][] GetChannels()
    {
        var channels_count = _Header.ChannelsCount;
        var data_length = _Header.FrameCount;

        var result = new long[channels_count][];
        for (var channel = 0; channel < channels_count; channel++)
            result[channel] = new long[data_length];

        _ = ReadChannels(result);
        return result;
    }

    /// <summary>Асинхронно получить массивы значений отсчётов всех каналов</summary>
    /// <param name="Progress">Объект информирования о прогрессе асинхронной операции чтения в диапазоне значений от 0 до 1</param>
    /// <param name="Cancel">Признак отмены асинхронной операции</param>
    /// <returns>Задача, возвращающая массив массивов значений всех каналов</returns>
    /// <exception cref="InvalidOperationException">Если при чтении очередного значения будет число прочитанных байт не будет равно размеру одного кадра</exception>
    public async Task<long[][]> GetChannelsAsync(IProgress<double> Progress = null, CancellationToken Cancel = default)
    {
        Cancel.ThrowIfCancellationRequested();

        var channels_count = _Header.ChannelsCount;
        var data_length = _Header.FrameCount;

        var result = new long[channels_count][];
        for (var channel = 0; channel < channels_count; channel++)
            result[channel] = new long[data_length];

        _ = await ReadChannelsAsync(result, Progress: Progress, Cancel: Cancel).ConfigureAwait(false);
        return result;
    }

    /// <summary>Выполнить перечисление отсчётов значений канала</summary>
    /// <param name="Channel">Индекс канала, отсчёты которого надо перечислить</param>
    /// <returns>Перечисление кортежей, включающих в себя временную отметку в секундах от начала файла и ей соответствующее значение</returns>
    public abstract IEnumerable<(double Time, long Value)> EnumerateSamples(int Channel);

    /// <summary>Выполнить асинхронное перечисление отсчётов значений канала</summary>
    /// <param name="Channel">Индекс канала, отсчёты которого надо перечислить</param>
    /// <param name="Progress">Объект информирования о прогрессе асинхронной операции чтения в диапазоне значений от 0 до 1</param>
    /// <param name="Cancel">Признак отмены асинхронной операции</param>
    /// <returns>Перечисление кортежей, включающих в себя временную отметку в секундах от начала файла и ей соответствующее значение</returns>
    public abstract IAsyncEnumerable<(double Time, long Value)> EnumerateSamplesAsync(int Channel, IProgress<double> Progress = null, CancellationToken Cancel = default);

    /// <summary>Выполнить перечисление отсчётов значений всех каналов</summary>
    /// <returns>Перечисление кортежей, включающих в себя временную отметку в секундах от начала файла и ей соответствующие значения всех каналов</returns>
    public abstract IEnumerable<(double Time, IReadOnlyList<long> Values)> EnumerateSamples();

    /// <summary>Выполнить асинхронное перечисление отсчётов значений всех каналов</summary>
    /// <param name="Progress">Объект информирования о прогрессе асинхронной операции чтения в диапазоне значений от 0 до 1</param>
    /// <param name="Cancel">Признак отмены асинхронной операции</param>
    /// <returns>Перечисление кортежей, включающих в себя временную отметку в секундах от начала файла и ей соответствующие значения всех каналов</returns>
    public abstract IAsyncEnumerable<(double Time, IReadOnlyList<long> Values)> EnumerateSamplesAsync(IProgress<double> Progress = null, CancellationToken Cancel = default);

    /// <summary>Выполнить перечисление отсчётов значений всех каналов использующее в процессе один и тот же буферный массив</summary>
    /// <returns>Перечисление кортежей, включающих в себя временную отметку в секундах от начала файла и ей соответствующие значения всех каналов, значения которых копируются в один и тот же передаваемый массив</returns>
    public abstract IEnumerable<(double Time, IReadOnlyList<long> Values)> EnumerateSamplesWithSingleArray();

    /// <summary>Выполнить асинхронное перечисление отсчётов значений всех каналов использующее в процессе один и тот же буферный массив</summary>
    /// <param name="Progress">Объект информирования о прогрессе асинхронной операции чтения в диапазоне значений от 0 до 1</param>
    /// <param name="Cancel">Признак отмены асинхронной операции</param>
    /// <returns>Перечисление кортежей, включающих в себя временную отметку в секундах от начала файла и ей соответствующие значения всех каналов, значения которых копируются в один и тот же передаваемый массив</returns>
    public abstract IAsyncEnumerable<(double Time, IReadOnlyList<long> Values)> EnumerateSamplesWithSingleArrayAsync(IProgress<double> Progress = null, CancellationToken Cancel = default);

    /* ------------------------------------------------------------------------------------- */
}