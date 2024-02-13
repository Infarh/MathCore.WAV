#nullable enable
using MathCore.WAV.Exceptions;
using MathCore.WAV.Infrastructure;

// ReSharper disable UnusedMethodReturnValue.Global

// ReSharper disable ClassWithVirtualMembersNeverInherited.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable ConvertToAutoPropertyWhenPossible
namespace MathCore.WAV;

/// <summary>Объект для записи файла в формате wav PCM</summary>
public class WavFileWriter : IDisposable, IAsyncDisposable
{
    /* ------------------------------------------------------------------------------------- */

    /// <summary>Поток данных</summary>
    private readonly Stream _DataStream;

    /// <summary>Число каналов</summary>
    private readonly short _ChannelsCount;

    /// <summary>Частота дискретизации</summary>
    private readonly int _SampleRate;

    /// <summary>Количество байт на один фрейм (один отсчёт по всем каналам)</summary>
    private readonly short _BlockAlign;

    /// <summary>Число бит на канал</summary>
    private readonly short _BitsPerSample;

    /// <summary>Смещение центра интервала физической величины</summary>
    private double _ValuesOffset;

    /// <summary>Буферный массив значений для записи вещественных значений</summary>
    private readonly long[] _ChannelValues;

    /// <summary>Амплитуда физической величины</summary>
    private double _Amplitude = double.NaN;

    /* ------------------------------------------------------------------------------------- */

    /// <summary>Число каналов</summary>
    public int ChannelsCount => _ChannelsCount;

    /// <summary>Частота дискретизации</summary>
    public int SampleRate => _SampleRate;

    /// <summary>Период дискретизации</summary>
#pragma warning disable IDE1006 // Стили именования
    // ReSharper disable once InconsistentNaming
    public double dt => 1d / _SampleRate;
#pragma warning restore IDE1006 // Стили именования

    /// <summary>Смещение центра интервала физической величины</summary>
    public double ValuesOffset
    {
        get => _ValuesOffset;
        set
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new ArgumentOutOfRangeException(nameof(value), value, "Некорректное значение смещения");
            _ValuesOffset = value;
        }
    }

    /// <summary>Амплитуда физической величины</summary>
    public double Amplitude
    {
        get => _Amplitude;
        set
        {
            if (value <= double.Epsilon) 
                throw new ArgumentOutOfRangeException(nameof(value), value, "Требуется положительное значение");

            _Amplitude = value;
        }
    }

    /// <summary>Амплитуда канала</summary>
    public long ChannelAmplitude => (1 << (_BitsPerSample - 1)) - 1;

    public double ChannelResolution => _Amplitude / ChannelAmplitude;

    public double AmplitudeResolution => ChannelAmplitude / _Amplitude;

    public Header Header => new(
        (int)_DataStream.Length,
        _ChannelsCount,
        _SampleRate,
        _BlockAlign,
        _BitsPerSample,
        (int)_DataStream.Length - Header.Length);

    /* ------------------------------------------------------------------------------------- */

    /// <summary>Инициализация нового экземпляра <see cref="WavFileWriter"/></summary>
    /// <param name="FileName">Имя файла для записи данных</param>
    /// <param name="ChannelsCount">Число каналов (по умолчанию 1)</param>
    /// <param name="SampleRate">Частота дискретизации в Гц (по умолчанию 44'100 Гц, или 44,1кГц)</param>
    /// <param name="BitsPerSample">Число бит на канал (по умолчанию 16 бит на канал)</param>
    public WavFileWriter(string FileName, short ChannelsCount = 1, int SampleRate = 44100, short BitsPerSample = 16)
        : this(
            new FileInfo(FileName ?? throw new ArgumentNullException(nameof(FileName))), 
            ChannelsCount, SampleRate, BitsPerSample)
    {

    }

    /// <summary>Инициализация нового экземпляра <see cref="WavFileWriter"/></summary>
    /// <param name="file">Файл записи данных</param>
    /// <param name="ChannelsCount">Число каналов (по умолчанию 1)</param>
    /// <param name="SampleRate">Частота дискретизации в Гц (по умолчанию 44'100 Гц, или 44,1кГц)</param>
    /// <param name="BitsPerSample">Число бит на канал (по умолчанию 16 бит на канал)</param>
    public WavFileWriter(FileInfo file, short ChannelsCount = 1, int SampleRate = 44100, short BitsPerSample = 16)
    {
        if (file is null) throw new ArgumentNullException(nameof(file));
        if (ChannelsCount <= 0) throw new ArgumentOutOfRangeException(nameof(ChannelsCount), "Число каналов должно быть больше 0");
        if (BitsPerSample <= 0) throw new ArgumentOutOfRangeException(nameof(BitsPerSample), "Число бит на канал должно быть больше 0");

        if(BitsPerSample is not (8 or 16 or 32 or 64))
            throw new ArgumentOutOfRangeException(nameof(BitsPerSample), BitsPerSample, "Поддерживается 8, 16, 32 и 64 бит на канал");

        _ChannelsCount = ChannelsCount;
        _SampleRate    = SampleRate;
        // ReSharper disable once UselessBinaryOperation
        _BlockAlign    = (short)(ChannelsCount * (BitsPerSample >> 3));
        _BitsPerSample = BitsPerSample;
        _WriteBuffer   = new byte[_BlockAlign];
        _ChannelValues = new long[_ChannelsCount];

        _DataStream = file.Create();
        var header_bytes = new byte[Header.Length];
        _DataStream.Write(header_bytes, 0, header_bytes.Length);
    }

    /* ------------------------------------------------------------------------------------- */

    /// <summary>Записать вещественные значения в поток</summary>
    /// <param name="Values">Массив значений каналов, записываемых данных</param>
    /// <returns>Текущее значение времени записанных данных в секундах</returns>
    public double Write(params short[] Values)
    {
        if (Values is null) throw new ArgumentNullException(nameof(Values));
        if (Values.Length != _ChannelsCount)
            throw new ArgumentArrayLengthException(nameof(Values), Values.Length, _ChannelsCount, "Размер массива параметров не соответствует числу каналов файла");

        for (var i = 0; i < _ChannelsCount; i++)
            _ChannelValues[i] = Values[i];

        return Write(_ChannelValues);
    }

    /// <summary>Записать вещественные значения в поток</summary>
    /// <param name="Values">Массив значений каналов, записываемых данных</param>
    /// <returns>Текущее значение времени записанных данных в секундах</returns>
    public double Write(params int[] Values)
    {
        if (Values is null) throw new ArgumentNullException(nameof(Values));
        if (Values.Length != _ChannelsCount)
            throw new ArgumentArrayLengthException(nameof(Values), Values.Length, _ChannelsCount, "Размер массива параметров не соответствует числу каналов файла");

        for (var i = 0; i < _ChannelsCount; i++)
            _ChannelValues[i] = Values[i];

        return Write(_ChannelValues);
    }

    /// <summary>Записать вещественные значения в поток</summary>
    /// <param name="Values">Массив значений каналов, записываемых данных</param>
    /// <returns>Текущее значение времени записанных данных в секундах</returns>
    public double Write(params double[] Values)
    {
        if (Values is null) throw new ArgumentNullException(nameof(Values));
        if (Values.Length != _ChannelsCount)
            throw new ArgumentArrayLengthException(nameof(Values), Values.Length, _ChannelsCount, "Размер массива параметров не соответствует числу каналов файла");

        var a = _Amplitude;
        if (double.IsNaN(a))
            for (var i = 0; i < _ChannelsCount; i++)
                _ChannelValues[i] = (long)Math.Round(Values[i]);
        else
        {
            var k = AmplitudeResolution;
            for (var i = 0; i < _ChannelsCount; i++)
                _ChannelValues[i] = Wav.ValueToSample(Math.Min(a, Math.Max(-a, Values[i])) - _ValuesOffset, k);
        }

        return Write(_ChannelValues);
    }

    /// <summary>Записать вещественные значения (в двоично-десятичном представлении) в поток</summary>
    /// <param name="Values">Массив значений каналов, записываемых данных</param>
    /// <returns>Текущее значение времени записанных данных в секундах</returns>
    public double Write(params decimal[] Values)
    {
        if (Values is null) throw new ArgumentNullException(nameof(Values));
        if (Values.Length != _ChannelsCount)
            throw new ArgumentArrayLengthException(nameof(Values), Values.Length, _ChannelsCount, "Размер массива параметров не соответствует числу каналов файла");

        if (double.IsNaN(_Amplitude))
            for (var i = 0; i < _ChannelsCount; i++)
                _ChannelValues[i] = (long)Math.Round(Values[i]);
        else
        {
            var a = (decimal)_Amplitude;
            var k = (decimal)AmplitudeResolution;
            for (var i = 0; i < _ChannelsCount; i++)
                _ChannelValues[i] = Wav.ValueToSample(Math.Min(a, Math.Max(-a, Values[i])) - (decimal)_ValuesOffset, k);
        }

        return Write(_ChannelValues);
    }

    /// <summary>Буферный массив байт для записи данных одного фрейма, включающего отсчёты всех каналов в текущий момент времени</summary>
    private readonly byte[] _WriteBuffer;

    /// <summary>Записать значения всех каналов на текущий момент времени</summary>
    /// <param name="Values">Массив значений всех каналов в текущий момент времени</param>
    /// <returns>Значение текущего времени записанного отсчёта в секундах</returns>
    public double Write(params long[] Values)
    {
        if (Values is null) throw new ArgumentNullException(nameof(Values));
        if (Values.Length != _ChannelsCount)
            throw new ArgumentArrayLengthException(nameof(Values), Values.Length, _ChannelsCount, "Размер массива параметров не соответствует числу каналов файла");

        var byte_per_sample = _BitsPerSample >> 3;
        if (byte_per_sample is 1 or 2 or 4 or 8)
            for (var channel = 0; channel < _ChannelsCount; channel++)
                Buffer.BlockCopy(Values, channel << 3, _WriteBuffer, channel * byte_per_sample, byte_per_sample);
        else
            throw new ArgumentOutOfRangeException(nameof(_BitsPerSample), _BitsPerSample, "Поддерживается 8, 16, 32 и 64 бит на канал");

        _DataStream.Write(_WriteBuffer, 0, _BlockAlign);

        var pos = _DataStream.Length - Header.Length;
        return (double)pos / _BlockAlign / _SampleRate;
    }

    /// <summary>Выполнить асинхронную операцию записи значений всех каналов на текущий момент времени</summary>
    /// <param name="Values">Массив значений всех каналов в текущий момент времени</param>
    /// <returns>Значение текущего времени записанного отсчёта в секундах</returns>
    public ValueTask<double> WriteAsync(params int[] Values) => WriteAsync(CancellationToken.None, Values);

    /// <summary>Выполнить асинхронную операцию записи значений всех каналов на текущий момент времени</summary>
    /// <param name="Cancel">Признак отмены асинхронной операции</param>
    /// <param name="Values">Массив значений всех каналов в текущий момент времени</param>
    /// <returns>Значение текущего времени записанного отсчёта в секундах</returns>
    public async ValueTask<double> WriteAsync(CancellationToken Cancel, params int[] Values)
    {
        if (Values is null) throw new ArgumentNullException(nameof(Values));
        if (Values.Length != _ChannelsCount)
            throw new ArgumentArrayLengthException(nameof(Values), Values.Length, _ChannelsCount, "Размер массива параметров не соответствует числу каналов файла");

        for (var i = 0; i < _ChannelsCount; i++)
            _ChannelValues[i] = Values[i];

        return await WriteAsync(Cancel, _ChannelValues).ConfigureAwait(false);
    }

    /// <summary>Выполнить асинхронную операцию записи значений всех каналов на текущий момент времени</summary>
    /// <param name="Values">Массив значений всех каналов в текущий момент времени</param>
    /// <returns>Значение текущего времени записанного отсчёта в секундах</returns>
    public ValueTask<double> WriteAsync(params double[] Values) => WriteAsync(CancellationToken.None, Values);

    /// <summary>Выполнить асинхронную операцию записи значений всех каналов на текущий момент времени</summary>
    /// <param name="Cancel">Признак отмены асинхронной операции</param>
    /// <param name="Values">Массив значений всех каналов в текущий момент времени</param>
    /// <returns>Значение текущего времени записанного отсчёта в секундах</returns>
    public async ValueTask<double> WriteAsync(CancellationToken Cancel, params double[] Values)
    {
        if (Values is null) throw new ArgumentNullException(nameof(Values));
        if (Values.Length != _ChannelsCount)
            throw new ArgumentArrayLengthException(nameof(Values), Values.Length, _ChannelsCount, "Размер массива параметров не соответствует числу каналов файла");

        if (_Amplitude is double.NaN or 1 && _ValuesOffset == 0)
            for (var i = 0; i < _ChannelsCount; i++)
                _ChannelValues[i] = (long)Math.Round(Values[i]);
        else
            for (var i = 0; i < _ChannelsCount; i++)
                _ChannelValues[i] = (long)(Math.Round((Values[i] - _ValuesOffset) / _Amplitude) * _Amplitude);

        return await WriteAsync(Cancel, _ChannelValues).ConfigureAwait(false);
    }

    /// <summary>Выполнить асинхронную операцию записи значений всех каналов на текущий момент времени</summary>
    /// <param name="Values">Массив значений всех каналов в текущий момент времени</param>
    /// <returns>Значение текущего времени записанного отсчёта в секундах</returns>
    public ValueTask<double> WriteAsync(params decimal[] Values) => WriteAsync(CancellationToken.None, Values);

    /// <summary>Выполнить асинхронную операцию записи значений всех каналов на текущий момент времени</summary>
    /// <param name="Cancel">Признак отмены асинхронной операции</param>
    /// <param name="Values">Массив значений всех каналов в текущий момент времени</param>
    /// <returns>Значение текущего времени записанного отсчёта в секундах</returns>
    public async ValueTask<double> WriteAsync(CancellationToken Cancel, params decimal[] Values)
    {
        if (Values is null) throw new ArgumentNullException(nameof(Values));
        if (Values.Length != _ChannelsCount)
            throw new ArgumentArrayLengthException(nameof(Values), Values.Length, _ChannelsCount, "Размер массива параметров не соответствует числу каналов файла");

        if (_Amplitude is double.NaN or 1 && _ValuesOffset == 0)
            for (var i = 0; i < _ChannelsCount; i++)
                _ChannelValues[i] = (long)Math.Round(Values[i]);
        else
            for (var i = 0; i < _ChannelsCount; i++)
                _ChannelValues[i] = (long)(Math.Round((Values[i] - (decimal)_ValuesOffset) / (decimal)_Amplitude) * (decimal)_Amplitude);

        return await WriteAsync(Cancel, _ChannelValues).ConfigureAwait(false);
    }

    /// <summary>Выполнить асинхронную операцию записи значений всех каналов на текущий момент времени</summary>
    /// <param name="Values">Массив значений всех каналов в текущий момент времени</param>
    /// <returns>Значение текущего времени записанного отсчёта в секундах</returns>
    public ValueTask<double> WriteAsync(params long[] Values) => WriteAsync(CancellationToken.None, Values);

    /// <summary>Выполнить асинхронную операцию записи значений всех каналов на текущий момент времени</summary>
    /// <param name="Cancel">Признак отмены асинхронной операции</param>
    /// <param name="Values">Массив значений всех каналов в текущий момент времени</param>
    /// <returns>Значение текущего времени записанного отсчёта в секундах</returns>
    public async ValueTask<double> WriteAsync(CancellationToken Cancel, params long[] Values)
    {
        if (Values is null) throw new ArgumentNullException(nameof(Values));
        if (Values.Length < _ChannelsCount)
            throw new ArgumentException($"Число каналов в файле задано равным {_ChannelsCount}, а для записи передано {Values.Length} значений");
        Cancel.ThrowIfCancellationRequested();

        var byte_per_sample = _BitsPerSample >> 3;
        if (byte_per_sample is 1 or 2 or 4 or 8)
            for (var channel = 0; channel < _ChannelsCount; channel++)
                Buffer.BlockCopy(Values, channel << 3, _WriteBuffer, channel * byte_per_sample, byte_per_sample);
        else
            throw new ArgumentOutOfRangeException(nameof(_BitsPerSample), _BitsPerSample, "Поддерживается 8, 16, 32 и 64 бит на канал");

        await _DataStream.WriteAsync(_WriteBuffer, 0, _BlockAlign, Cancel).ConfigureAwait(false);

        var pos = _DataStream.Length - Header.Length;
        return (double)pos / _BlockAlign / _SampleRate;
    }

    public void Write(params IEnumerable<long>[] Signals)
    {
        foreach (var values in Signals.AsBlockEnumerableWithSingleArray())
            Write(values);
    }

    public void Write(params IEnumerable<double>[] Signals)
    {
        foreach (var values in Signals.AsBlockEnumerableWithSingleArray())
            Write(values);
    }

    public void Write(params IEnumerable<decimal>[] Signals)
    {
        foreach (var values in Signals.AsBlockEnumerableWithSingleArray())
            Write(values);
    }

    public Task WriteAsync(params IEnumerable<long>[] Signals) => WriteAsync(null, CancellationToken.None, Signals);

    public Task WriteAsync(CancellationToken Cancel, params IEnumerable<long>[] Signals) => WriteAsync(null, Cancel, Signals);

    public async Task WriteAsync(IProgress<long>? Progress, CancellationToken Cancel, params IEnumerable<long>[] Signals)
    {
        Cancel.ThrowIfCancellationRequested();
        if (Progress is null)
            foreach (var values in Signals.AsBlockEnumerableWithSingleArray())
                await WriteAsync(Cancel, values).ConfigureAwait(false);
        else
        {
            var i = 0L;
            foreach (var values in Signals.AsBlockEnumerableWithSingleArray())
            {
                await WriteAsync(Cancel, values).ConfigureAwait(false);
                Progress.Report(++i);
            }
        }
    }

    public Task WriteAsync(params IEnumerable<int>[] Signals) => WriteAsync(null, CancellationToken.None, Signals);

    public Task WriteAsync(CancellationToken Cancel, params IEnumerable<int>[] Signals) => WriteAsync(null, Cancel, Signals);

    public async Task WriteAsync(IProgress<long>? Progress, CancellationToken Cancel, params IEnumerable<int>[] Signals)
    {
        Cancel.ThrowIfCancellationRequested();
        if (Progress is null)
            foreach (var values in Signals.AsBlockEnumerableWithSingleArray())
                await WriteAsync(Cancel, values).ConfigureAwait(false);
        else
        {
            var i = 0L;
            foreach (var values in Signals.AsBlockEnumerableWithSingleArray())
            {
                await WriteAsync(Cancel, values).ConfigureAwait(false);
                Progress.Report(++i);
            }
        }
    }

    public Task WriteAsync(params IEnumerable<double>[] Signals) => WriteAsync(null, CancellationToken.None, Signals);

    public Task WriteAsync(CancellationToken Cancel, params IEnumerable<double>[] Signals) => WriteAsync(null, Cancel, Signals);

    public async Task WriteAsync(IProgress<long>? Progress, CancellationToken Cancel, params IEnumerable<double>[] Signals)
    {
        Cancel.ThrowIfCancellationRequested();
        if (Progress is null)
            foreach (var values in Signals.AsBlockEnumerableWithSingleArray())
                await WriteAsync(Cancel, values).ConfigureAwait(false);
        else
        {
            var i = 0L;
            foreach (var values in Signals.AsBlockEnumerableWithSingleArray())
            {
                await WriteAsync(Cancel, values).ConfigureAwait(false);
                Progress.Report(++i);
            }
        }
    }

    public Task WriteAsync(params IEnumerable<decimal>[] Signals) => WriteAsync(null, CancellationToken.None, Signals);

    public Task WriteAsync(CancellationToken Cancel, params IEnumerable<decimal>[] Signals) => WriteAsync(null, Cancel, Signals);

    public async Task WriteAsync(IProgress<long>? Progress, CancellationToken Cancel, params IEnumerable<decimal>[] Signals)
    {
        Cancel.ThrowIfCancellationRequested();
        if (Progress is null)
            foreach (var values in Signals.AsBlockEnumerableWithSingleArray())
                await WriteAsync(Cancel, values).ConfigureAwait(false);
        else
        {
            var i = 0L;
            foreach (var values in Signals.AsBlockEnumerableWithSingleArray())
            {
                await WriteAsync(Cancel, values).ConfigureAwait(false);
                Progress.Report(++i);
            }
        }
    }

    public void WriteRaw(byte[] buffer, int offset, int count) => _DataStream.Write(buffer, offset, count);

    public void WriteRaw(byte[] buffer) => WriteRaw(buffer, 0, buffer.Length);

    public async Task WriteRawAsync(byte[] buffer, int offset, int count, CancellationToken Cancel = default) =>
        await _DataStream.WriteAsync(buffer, offset, count, Cancel).ConfigureAwait(false);

    public Task WriteRawAsync(byte[] buffer, CancellationToken Cancel = default) => WriteRawAsync(buffer, 0, buffer.Length, Cancel);

    public Stream GetDataStream() => new WriteOnlyStreamWrapper(_DataStream);

    public BinaryWriter GetWriter() => new(GetDataStream(), System.Text.Encoding.UTF8, true);

    /* ------------------------------------------------------------------------------------- */

    #region IDisposable

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Признак того, что файл был закрыт</summary>
    private int _Disposed;

    /// <summary>Выполняет запись заголовка файла (обновление данных о параметрах)</summary>
    /// <param name="disposing">Выполнить освобождение ресурсов</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing) return;
        if(Interlocked.Exchange(ref _Disposed, 1) == 1) return;

        //var header = new Header(
        //    (int)_DataStream.Length,
        //    _ChannelsCount,
        //    _SampleRate,
        //    _BlockAlign,
        //    _BitsPerSample,
        //    (int)_DataStream.Length - Header.Length);

        _DataStream.Seek(0, SeekOrigin.Begin);
        using (_DataStream) 
            Header.WriteTo(new(_DataStream));
    }

    /// <summary>Выполняет процедуру асинхронной записи заголовка файла (обновление данных о параметрах)</summary>
    public virtual async ValueTask DisposeAsync()
    {
        if(Interlocked.Exchange(ref _Disposed, 1) == 1) return;

        //var header = new Header(
        //    (int)_DataStream.Length,
        //    _ChannelsCount,
        //    _SampleRate,
        //    _BlockAlign,
        //    _BitsPerSample,
        //    (int)_DataStream.Length - Header.Length);

        _DataStream.Seek(0, SeekOrigin.Begin);
        using (_DataStream) 
            await Header.WriteToAsync(new(_DataStream));
    }

    #endregion

    /* ------------------------------------------------------------------------------------- */
}