using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
namespace MathCore.WAV
{
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

        /// <summary>Смещение центра интервала физической величины</summary>
        private double _ValuesOffset;

        /// <summary>Амплитуда физической величины</summary>
        private double _Amplitude = double.NaN;

        /* ------------------------------------------------------------------------------------- */

        /// <summary>Полная длина файла в байтах включая заголовок</summary>
        public long DataLength => _Header.SubChunk2Size;

        /// <summary>Частота дискретизации</summary>
        public int SampleRate => _Header.SampleRate;

        /// <summary>Байт на один отсчёт</summary>
        public int BytesPerSample => _Header.BytesPerSample;

        /// <summary>Количество бит в семпле (8, 16, 32, 64...)</summary>
        public int BitsPerSample => _Header.BitsPerSample;

        /// <summary>Количество байт на один фрейм (один отсчёт по всем каналам)</summary>
        public short FrameLength => _Header.BlockAlign;

        /// <summary>Число фреймов в файле</summary>
        public long FramesCount => _Header.FrameCount;

        /// <summary>Период дискретизации</summary>
#pragma warning disable IDE1006 // Стили именования
        public double dt => 1d / _Header.SampleRate;
#pragma warning restore IDE1006 // Стили именования

        /// <summary>Длина файла в секундах</summary>
        public double FileTimeLength => _Header.TimeLengthInSeconds;

        /// <summary>Количество каналов</summary>
        public int ChannelsCount => _Header.ChannelsCount;

        /// <summary>Байт на один отсчёт</summary>
        public int SampleLength => _Header.BytesPerSample;

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
        public long ChannelAmplitude => (1 << (_Header.BitsPerSample - 1)) - 1;

        public double ChannelResolution => _Amplitude / ChannelAmplitude;

        public double AmplitudeResolution => ChannelAmplitude / _Amplitude;

        /// <summary>Индексатор фреймов</summary>
        /// <param name="i">Номер отсчёта в потоке</param>
        /// <returns>Фрейм со значениями всех каналов</returns>
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
                if (data_stream.Read(sample_data, 0, sample_length) != sample_length)
                    throw new InvalidOperationException($"Ошибка чтения файла при загрузке данных {i} фрейма");
                return new Frame(i / (double)_Header.SampleRate, _Header.ChannelsCount, sample_data);
            }
        }

        /* ------------------------------------------------------------------------------------- */

        /// <summary>Инициализатор данных <see cref="Wav"/></summary>
        /// <param name="Header">Заголовок <see cref="Header"/></param>
        protected Wav(Header Header) => _Header = Header;

        /* ------------------------------------------------------------------------------------- */

        /// <summary>Получить поток байт данных для осуществления процедуры чтения</summary>
        /// <returns>Поток байт данных WAV</returns>
        protected abstract Stream GetDataStream();

        /// <summary>Прочитать все значения отсчётов канала</summary>
        /// <param name="Channel">Номер канала</param>
        /// <returns>Массив отсчётов канала</returns>
        public long[] GetChannel(int Channel)
        {
            var channels_count = _Header.ChannelsCount;
            if (Channel >= channels_count)
                throw new ArgumentOutOfRangeException(nameof(Channel), Channel, $"В файле содержится {channels_count} каналов, а запрошен {Channel}");
            using var data_stream = GetDataStream();

            var sample_length = _Header.BlockAlign;
            var sample_data = new byte[sample_length];

            var data_length = _Header.FrameCount;
            var result = new long[data_length];

            var bytes_per_sample = _Header.BytesPerSample;
            for (var i = 0; i < data_length; i++)
            {
                data_stream.Seek(Header.Length + i * sample_length, SeekOrigin.Begin);
                if (data_stream.Read(sample_data, 0, sample_length) != sample_length)
                    throw new InvalidOperationException($"Ошибка чтения файла при загрузке данных {i} фрейма");
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

        public double[] GetChannelDouble(int Channel)
        {
            var channels_count = _Header.ChannelsCount;
            if (Channel >= channels_count)
                throw new ArgumentOutOfRangeException(nameof(Channel), Channel, $"В файле содержится {channels_count} каналов, а запрошен {Channel}");
            using var data_stream = GetDataStream();

            var sample_length = _Header.BlockAlign;
            var sample_data = new byte[sample_length];

            var data_length = _Header.FrameCount;
            var result = new double[data_length];

            var bytes_per_sample = _Header.BytesPerSample;
            var resolution = ChannelResolution;
            if (double.IsNaN(resolution))
                for (var i = 0; i < data_length; i++)
                {
                    data_stream.Seek(Header.Length + i * sample_length, SeekOrigin.Begin);
                    if (data_stream.Read(sample_data, 0, sample_length) != sample_length)
                        throw new InvalidOperationException($"Ошибка чтения файла при загрузке данных {i} фрейма");
                    var value = bytes_per_sample switch
                    {
                        1 => sample_data[Channel],
                        2 => BitConverter.ToInt16(sample_data, Channel * bytes_per_sample),
                        4 => BitConverter.ToInt32(sample_data, Channel * bytes_per_sample),
                        8 => BitConverter.ToInt64(sample_data, Channel * bytes_per_sample),
                        _ => throw new NotSupportedException($"Размерность отсчёта {bytes_per_sample} байт на канал не поддерживается")
                    };
                    result[i] = value;
                    //result[i] = Wav.SampleToValue(value, )
                }
            else
                for (var i = 0; i < data_length; i++)
                {
                    data_stream.Seek(Header.Length + i * sample_length, SeekOrigin.Begin);
                    if (data_stream.Read(sample_data, 0, sample_length) != sample_length)
                        throw new InvalidOperationException($"Ошибка чтения файла при загрузке данных {i} фрейма");
                    var value = bytes_per_sample switch
                    {
                        1 => sample_data[Channel],
                        2 => BitConverter.ToInt16(sample_data, Channel * bytes_per_sample),
                        4 => BitConverter.ToInt32(sample_data, Channel * bytes_per_sample),
                        8 => BitConverter.ToInt64(sample_data, Channel * bytes_per_sample),
                        _ => throw new NotSupportedException($"Размерность отсчёта {bytes_per_sample} байт на канал не поддерживается")
                    };
                    result[i] = SampleToValue(value, resolution);
                }
            return result;
        }

        public decimal[] GetChannelDecimal(int Channel)
        {
            var channels_count = _Header.ChannelsCount;
            if (Channel >= channels_count)
                throw new ArgumentOutOfRangeException(nameof(Channel), Channel, $"В файле содержится {channels_count} каналов, а запрошен {Channel}");
            using var data_stream = GetDataStream();

            var sample_length = _Header.BlockAlign;
            var sample_data = new byte[sample_length];

            var data_length = _Header.FrameCount;
            var result = new decimal[data_length];

            var bytes_per_sample = _Header.BytesPerSample;
            var channel_resolution = ChannelResolution;
            var resolution = (decimal)channel_resolution;
            if (double.IsNaN(channel_resolution))
                for (var i = 0; i < data_length; i++)
                {
                    data_stream.Seek(Header.Length + i * sample_length, SeekOrigin.Begin);
                    if (data_stream.Read(sample_data, 0, sample_length) != sample_length)
                        throw new InvalidOperationException($"Ошибка чтения файла при загрузке данных {i} фрейма");
                    var value = bytes_per_sample switch
                    {
                        1 => sample_data[Channel],
                        2 => BitConverter.ToInt16(sample_data, Channel * bytes_per_sample),
                        4 => BitConverter.ToInt32(sample_data, Channel * bytes_per_sample),
                        8 => BitConverter.ToInt64(sample_data, Channel * bytes_per_sample),
                        _ => throw new NotSupportedException($"Размерность отсчёта {bytes_per_sample} байт на канал не поддерживается")
                    };
                    result[i] = value;
                }
            else
                for (var i = 0; i < data_length; i++)
                {
                    data_stream.Seek(Header.Length + i * sample_length, SeekOrigin.Begin);
                    if (data_stream.Read(sample_data, 0, sample_length) != sample_length)
                        throw new InvalidOperationException($"Ошибка чтения файла при загрузке данных {i} фрейма");
                    var value = bytes_per_sample switch
                    {
                        1 => sample_data[Channel],
                        2 => BitConverter.ToInt16(sample_data, Channel * bytes_per_sample),
                        4 => BitConverter.ToInt32(sample_data, Channel * bytes_per_sample),
                        8 => BitConverter.ToInt64(sample_data, Channel * bytes_per_sample),
                        _ => throw new NotSupportedException($"Размерность отсчёта {bytes_per_sample} байт на канал не поддерживается")
                    };
                    result[i] = SampleToValue(value, resolution);
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
            var channels_count = _Header.ChannelsCount;
            if (Channel >= channels_count)
                throw new ArgumentOutOfRangeException(nameof(Channel), Channel, $"В файле содержится {channels_count} каналов, а запрошен {Channel}");
            using var data_stream = GetDataStream();

            var sample_length = _Header.BlockAlign;
            var sample_data = new byte[sample_length];

            var data_length = _Header.FrameCount;
            var result = new long[data_length];

            var bytes_per_sample = _Header.BytesPerSample;
            for (var i = 0; i < data_length; i++)
            {
                Cancel.ThrowIfCancellationRequested();
                data_stream.Seek(Header.Length + i * sample_length, SeekOrigin.Begin);
                if (await data_stream.ReadAsync(sample_data, 0, sample_length, Cancel).ConfigureAwait(false) != sample_length)
                    throw new InvalidOperationException($"Ошибка чтения файла при загрузке данных {i} фрейма");
                result[i] = bytes_per_sample switch
                {
                    1 => sample_data[Channel],
                    2 => BitConverter.ToInt16(sample_data, Channel * bytes_per_sample),
                    4 => BitConverter.ToInt32(sample_data, Channel * bytes_per_sample),
                    8 => BitConverter.ToInt64(sample_data, Channel * bytes_per_sample),
                    _ => throw new NotSupportedException($"Размерность отсчёта {bytes_per_sample} байт на канал не поддерживается")
                };
                Progress?.Report((double)i / data_length);
            }
            return result;
        }

        /// <summary>Получить массивы значений отсчётов всех каналов</summary>
        /// <returns>Массив массивов значений всех каналов</returns>
        /// <exception cref="InvalidOperationException">Если при чтении очередного значения будет число прочитанных байт не будет равно размеру одного кадра</exception>
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
            for (var i = 0; i < data_length; i++)
            {
                data_stream.Seek(Header.Length + i * sample_length, SeekOrigin.Begin);
                if (data_stream.Read(sample_data, 0, sample_length) != sample_length)
                    throw new InvalidOperationException($"Ошибка чтения файла при загрузке данных {i} фрейма");
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

        /// <summary>Асинхронно получить массивы значений отсчётов всех каналов</summary>
        /// <param name="Progress">Объект информирования о прогрессе асинхронной операции чтения в диапазоне значений от 0 до 1</param>
        /// <param name="Cancel">Признак отмены асинхронной операции</param>
        /// <returns>Задача, возвращающая массив массивов значений всех каналов</returns>
        /// <exception cref="InvalidOperationException">Если при чтении очередного значения будет число прочитанных байт не будет равно размеру одного кадра</exception>
        public async Task<long[][]> GetChannelsAsync(IProgress<double> Progress, CancellationToken Cancel = default)
        {
            Cancel.ThrowIfCancellationRequested();
            using var data_stream = GetDataStream();
            var channels_count = _Header.ChannelsCount;

            var sample_length = _Header.BlockAlign;
            var sample_data = new byte[sample_length];

            var data_length = _Header.FrameCount;
            var result = new long[channels_count][];
            for (var channel = 0; channel < channels_count; channel++)
                result[channel] = new long[data_length];

            var bytes_per_sample = _Header.BytesPerSample;
            for (var i = 0; i < data_length; i++)
            {
                Cancel.ThrowIfCancellationRequested();
                data_stream.Seek(Header.Length + i * sample_length, SeekOrigin.Begin);
                if (await data_stream.ReadAsync(sample_data, 0, sample_length, Cancel) != sample_length)
                    throw new InvalidOperationException($"Ошибка чтения файла при загрузке данных {i} фрейма");
                for (var channel = 0; channel < channels_count; channel++)
                    result[channel][i] = bytes_per_sample switch
                    {
                        1 => sample_data[channel],
                        2 => BitConverter.ToInt16(sample_data, channel * bytes_per_sample),
                        4 => BitConverter.ToInt32(sample_data, channel * bytes_per_sample),
                        8 => BitConverter.ToInt64(sample_data, channel * bytes_per_sample),
                        _ => throw new NotSupportedException($"Размерность отсчёта {bytes_per_sample} байт на канал не поддерживается")
                    };
                Progress?.Report((double)i / data_length);
            }
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
}