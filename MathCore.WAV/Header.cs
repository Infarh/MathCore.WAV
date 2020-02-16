using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using MathCore.WAV.Service;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
// ReSharper disable UnusedMember.Global
// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable ConvertToAutoProperty
namespace MathCore.WAV
{
    /// <summary>Заголовок WAV-файла</summary>
    [StructLayout(LayoutKind.Sequential, Size = Length, CharSet = CharSet.Auto, Pack = 1)]
    public readonly struct Header : IEquatable<Header>
    {
        /* ------------------------------------------------------------------------------------- */

        /// <summary>Загрузить данные заголовка из объекта чтения двоичных данных</summary>
        /// <param name="reader">Объект чтения двоичных данных, осуществляющий доступ к источнику данных</param>
        /// <returns>Прочитанный заголовок</returns>
        public static Header Load(BinaryReader reader) => new Header(reader);

        /// <summary>Загрузить данные заголовка из объекта чтения двоичных данных</summary>
        /// <param name="stream">Поток байт данных в WAV-формате</param>
        /// <returns>Прочитанный заголовок</returns>
        public static Header Load(Stream stream) => new Header(stream);

        /* ------------------------------------------------------------------------------------- */

        /// <summary>Длина заголовка</summary>
        public const int Length = 44;


        /* ------------------------------------------------------------------------------------- */
        /// <summary>Идентификатор заголовка файла. Должен содержать символы "RIFF"</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        private readonly byte[] _ChunkID;

        /// <summary>Оставшийся размер файла начиная с этой позиции. Должен быть равен размеру файла - 8 байт</summary>
        private readonly int _ChunkSize;

        /// <summary>Строка, содержащая символы "WAVE"</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        private readonly byte[] _Format;

        /// <summary>Строка, содержащая "fmt "</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        private readonly byte[] _SubChunk1Id;

        /// <summary>Оставшийся размер подцепочки (должен быть равен 16)</summary>
        private readonly int _SubChunk1Size;

        /// <summary>Формат файла</summary>
        private readonly Format _AudioFormat;

        /// <summary>Количество каналов</summary>
        private readonly short _ChannelsCount;

        /// <summary>Частота дискретизации Гц</summary>
        private readonly int _SampleRate;

        /// <summary>Скорость передачи Байт/с</summary>
        private readonly int _ByteRate;

        /// <summary>Размер фрейма в байтах включая все каналы</summary>
        private readonly short _BlockAlign;

        /// <summary>Бит на канал (8,16,...)</summary>
        private readonly short _BitsPerSample;

        /// <summary>Содержит символы "data"</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        private readonly byte[] _SubChunk2Id;

        /// <summary>Количество байт области данных</summary>
        private readonly int _SubChunk2Size;

        /* ------------------------------------------------------------------------------------- */

        /// <summary>Должен содержать строку "RIFF"</summary>
        public string ChunkID => Encoding.UTF8.GetString(_ChunkID);

        /// <summary>Должен содержать строку "WAVE"</summary>
        public string Format => Encoding.UTF8.GetString(_Format);

        /// <summary>Должен содержать строку "fmt\0x20"</summary>
        public string SubChunk1Id => Encoding.UTF8.GetString(_SubChunk1Id);

        /// <summary>Должен содержать строку "data"</summary>
        public string SubChunk2Id => Encoding.UTF8.GetString(_SubChunk2Id);

        /// <summary>Размер файла - 8 байт</summary>
        public int ChunkSize => _ChunkSize;

        /// <summary>Оставшийся размер цепочки (для PCM = 16)</summary>
        public int SubChunk1Size => _SubChunk1Size;

        /// <summary>Формат (по умолчанию PCM = 1)</summary>
        public Format AudioFormat => _AudioFormat;

        /// <summary>Количество каналов</summary>
        public short ChannelsCount => _ChannelsCount;

        /// <summary>Частота дискретизации</summary>
        public int SampleRate => _SampleRate;

        /// <summary>Количество байт на один фрейм (один отсчёт по всем каналам)</summary>
        public short BlockAlign => _BlockAlign;

        /// <summary>Количество бит в семпле (8, 16, 32, 64...)</summary>
        public short BitsPerSample => _BitsPerSample;

        /// <summary>Байт на один отсчёт</summary>
        public int BytesPerSample => _BitsPerSample >> 3;

        /// <summary>Количество байт области данных</summary>
        public int SubChunk2Size => _SubChunk2Size;

        /// <summary>Длина файла в секундах</summary>
        public double TimeLengthInSeconds => (double)_SubChunk2Size / _ByteRate;

        /// <summary>Длина файла во времени</summary>
        public TimeSpan TimeLength => TimeSpan.FromSeconds(TimeLengthInSeconds);

        /// <summary>Число фреймов в файле</summary>
        public int FrameCount => _SubChunk2Size / _BlockAlign;

        /* ------------------------------------------------------------------------------------- */

        /// <summary>Инициализация нового заголовка WAV-файла</summary>
        /// <param name="ChunkSize">Размер файла - 8 байт</param>
        /// <param name="SubChunk1Size">Оставшийся размер цепочки (для PCM = 16)</param>
        /// <param name="AudioFormat">Формат (по умолчанию PCM = 1)</param>
        /// <param name="ChannelsCount">Количество каналов</param>
        /// <param name="SampleRate">Частота дискретизации</param>
        /// <param name="ByteRate">Скорость передачи (байт/с) = <paramref name="SampleRate"/> * <paramref name="BlockAlign"/></param>
        /// <param name="BlockAlign">Количество байт на один фрейм (один отсчёт по всем каналам)</param>
        /// <param name="BitsPerSample">Количество бит в семпле (8, 16, 32, 64...)</param>
        /// <param name="SubChunk2Size">Количество байт области данных</param>
        public Header(
            int ChunkSize,
            int SubChunk1Size,
            Format AudioFormat,
            short ChannelsCount,
            int SampleRate,
            int ByteRate,
            short BlockAlign,
            short BitsPerSample,
            int SubChunk2Size
        )
        {
            _ChunkID = Encoding.UTF8.GetBytes("RIFF");
            _ChunkSize = ChunkSize;
            _Format = Encoding.UTF8.GetBytes("WAVE");
            _SubChunk1Id = Encoding.UTF8.GetBytes("fmt ");
            _SubChunk1Size = AudioFormat == WAV.Format.PCM ? 16 : SubChunk1Size;
            _AudioFormat = AudioFormat;
            _ChannelsCount = ChannelsCount;
            _SampleRate = SampleRate;
            _ByteRate = ByteRate;
            _BlockAlign = BlockAlign;
            _BitsPerSample = BitsPerSample;
            _SubChunk2Size = SubChunk2Size;
            _SubChunk2Id = Encoding.UTF8.GetBytes("data");
        }

        /// <summary>Инициализация нового заголовка WAV-файла в PCM-формате</summary>
        /// <param name="FileLength">Длина файла</param>
        /// <param name="ChannelsCount">Количество каналов</param>
        /// <param name="SampleRate">Частота дискретизации</param>
        /// <param name="BlockAlign">Количество байт на один фрейм (один отсчёт по всем каналам)</param>
        /// <param name="BitsPerSample">Количество бит в семпле (8, 16, 32, 64...)</param>
        /// <param name="DataLength">Количество байт области данных</param>
        public Header(
            int FileLength,
            short ChannelsCount,
            int SampleRate,
            short BlockAlign,
            short BitsPerSample,
            int DataLength
        )
        {
            _ChunkID = Encoding.UTF8.GetBytes("RIFF");
            _ChunkSize = FileLength - 8;
            _Format = Encoding.UTF8.GetBytes("WAVE");
            _SubChunk1Id = Encoding.UTF8.GetBytes("fmt ");
            _SubChunk1Size = 16;
            _AudioFormat = WAV.Format.PCM;
            _ChannelsCount = ChannelsCount;
            _SampleRate = SampleRate;
            _ByteRate = SampleRate * BlockAlign;
            _BlockAlign = BlockAlign;
            _BitsPerSample = BitsPerSample;
            _SubChunk2Size = DataLength;
            _SubChunk2Id = Encoding.UTF8.GetBytes("data");
        }

        /// <summary>Инициализация нового заголовка</summary>
        /// <param name="stream">Поток байт из которого будет осуществляться чтение данных</param>
        /// <exception cref="ArgumentNullException">В случае если поток для чтения данных не задан</exception>
        /// <exception cref="ArgumentException">В случае если длина потока байт меньше <see cref="Length"/></exception>
        /// <exception cref="FormatException">В случае нарушения формата заголовка, либо рассогласованности значений полей заголовка</exception>
        public Header(Stream stream) : this(new BinaryReader(stream ?? throw new ArgumentNullException(nameof(stream)))) { }

        /// <summary>Инициализация нового заголовка</summary>
        /// <param name="reader">Объект, осуществляющий чтение источника данных</param>
        /// <exception cref="ArgumentNullException">В случае если объект для чтения данных не задан</exception>
        /// <exception cref="ArgumentException">В случае если длина потока байт меньше <see cref="Length"/></exception>
        /// <exception cref="FormatException">В случае нарушения формата заголовка, либо рассогласованности значений полей заголовка</exception>
        public Header(BinaryReader reader)
        {
            if (reader is null)
                throw new ArgumentNullException(nameof(reader));

            var file_length = (reader.BaseStream as FileStream)?.Length ?? -1;
            if (file_length <= 44)
                throw new ArgumentException("Попытка чтения пустого файла");
            //if (file_length != -1 && file_length < 44)
            //    throw new FormatException("Размер файла недостаточен для хранения даже заголовка");

            #region Последовательная загрузка

            //Чтение текстовой метки "RIFF" (кодировка UTF-8)
            _ChunkID = reader.ReadBytes(4); // RIFF 0..3 (4)
            if (Encoding.UTF8.GetString(_ChunkID) != "RIFF")
                throw new FormatException("Ошибка формата - отсутствует сигнатура RIFF в начале потока данных");

            // Чтение размера файла без заголовка - должно быть равно длине файла минус 8 байт
            _ChunkSize = reader.ReadInt32(); // 4..7 (4) = file_length - 8
            //if (file_length != -1 && _ChunkSize != file_length - 8)
            //    throw new FormatException("Размер файла в заголовке определён неверно");

            // Чтение текстовой метки "WAVE" (кодировка UTF-8)
            _Format = reader.ReadBytes(4); // WAVE 8..11 (4)
            if (Encoding.UTF8.GetString(_Format) != "WAVE")
                throw new FormatException("Ошибка формата - отсутствует сигнатура WAVE в заголовке");

            // Чтение текстовой метки "fmt " (кодировка UTF-8)
            _SubChunk1Id = reader.ReadBytes(4); // fmt\0x20 12..15 (4)
            if (Encoding.UTF8.GetString(_SubChunk1Id) != "fmt ")
                throw new FormatException("Ошибка формата - отсутствует сигнатура \"fmt \" в заголовке");

            // Чтение оставшейся длины заголовка - для PCM-формата файла должно быть равно 16 байтам
            _SubChunk1Size = reader.ReadInt32(); // 16..19 (4) = 16
            _AudioFormat = (Format)reader.ReadInt16(); // 20..21 (2)
            //if (_AudioFormat == WAV.Format.PCM && _SubChunk1Size != 16)
            //    throw new FormatException("Размер оставшейся части заголовка PCM формата не равен 16");

            // Чтение числа каналов
            _ChannelsCount = reader.ReadInt16(); // 22..23 (2)
            if (_ChannelsCount <= 0)
                throw new FormatException("Ошибка формата заголовка: количество каналов не может быть меньше, либо равно 0");

            // Чтение частоты дискретизации в Гц
            _SampleRate = reader.ReadInt32(); // 24..27 (4)

            // Чтение скорости передачи в Байт/с
            _ByteRate = reader.ReadInt32(); // 28..31 (4)

            // Чтение длины кадра - число байт на все каналы на одно значение в один момент времени
            _BlockAlign = reader.ReadInt16(); // 32..33 (2)
            if (_ByteRate / _BlockAlign != _SampleRate)
                throw new FormatException("Ошибка формата файла: скорость потока (Байт/с) делёная на размер фрейма не равна частоте дискретизации");

            // Чтение числа бит, приходящегося на один канал
            _BitsPerSample = reader.ReadInt16(); // 34..35 (2)
            if (_ChannelsCount * _BitsPerSample / 8 != _BlockAlign)
                throw new FormatException($"Произведение числа байт на канал ({_BitsPerSample / 8}) "
                                          + $"и числа каналов ({_ChannelsCount}) "
                                          + $"не равно размеру фрейма ({_BlockAlign})");

            // Чтение текстовой метки "data" (кодировка UTF-8)
            _SubChunk2Id = reader.ReadBytes(4); // 36..39 (4)
            if (Encoding.UTF8.GetString(_SubChunk2Id) != "data")
                throw new FormatException("Ошибка формата - отсутствует сигнатура WAVE в заголовке");

            // Чтение оставшейся после заголовка длины файла. Должно быть равно длине файла - 44 байта для формата PCM
            _SubChunk2Size = reader.ReadInt32(); // 40..43 (4) = file_length - 44
            if (file_length != -1 && _AudioFormat == WAV.Format.PCM && _SubChunk2Size != file_length - Length)
                throw new FormatException("Ошибка формата заголовка: неверно указан размер области данных файла "
                                          + $"- длина файла {file_length}, длина заголовка {Length} байта, "
                                          + $"оставшаяся длина {file_length} - {Length} = {file_length - Length}, "
                                          + $"а указано {_SubChunk2Size}");

            #endregion
        }

        /* ------------------------------------------------------------------------------------- */

        /// <summary>Выполнить запись заголовка в объект для записи двоичных данных</summary>
        /// <param name="writer">Объект, осуществляющий запись данных в двоичном виде</param>
        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(_ChunkID);
            writer.Write(_ChunkSize);
            writer.Write(_Format);

            writer.Write(_SubChunk1Id);
            writer.Write(_SubChunk1Size);

            writer.Write((short)_AudioFormat);

            writer.Write(_ChannelsCount);
            writer.Write(_SampleRate);
            writer.Write(_ByteRate);
            writer.Write(_BlockAlign);
            writer.Write(_BitsPerSample);

            writer.Write(_SubChunk2Id);
            writer.Write(_SubChunk2Size);
        }

        /// <summary>Выполнить асинхронную запись заголовка в объект для записи двоичных данных</summary>
        /// <param name="writer">Объект, осуществляющий запись данных в двоичном виде</param>
        /// <param name="Cancel">Признак отмены асинхронной операции</param>
        public async Task WriteToAsync(BinaryWriter writer, CancellationToken Cancel = default)
        {
            await writer.WriteAsync(_ChunkID, Cancel);
            await writer.WriteAsync(_ChunkSize, Cancel);
            await writer.WriteAsync(_Format, Cancel);

            await writer.WriteAsync(_SubChunk1Id, Cancel);
            await writer.WriteAsync(_SubChunk1Size, Cancel);

            await writer.WriteAsync((short)_AudioFormat, Cancel);

            await writer.WriteAsync(_ChannelsCount, Cancel);
            await writer.WriteAsync(_SampleRate, Cancel);
            await writer.WriteAsync(_ByteRate, Cancel);
            await writer.WriteAsync(_BlockAlign, Cancel);
            await writer.WriteAsync(_BitsPerSample, Cancel);

            await writer.WriteAsync(_SubChunk2Id, Cancel);
            await writer.WriteAsync(_SubChunk2Size, Cancel);
        }

        /// <inheritdoc />
        public bool Equals(Header other) =>
            _ChunkSize == other._ChunkSize
            && _AudioFormat == other._AudioFormat
            && _ChannelsCount == other._ChannelsCount
            && _SampleRate == other._SampleRate
            && _ByteRate == other._ByteRate
            && _BlockAlign == other._BlockAlign
            && _BitsPerSample == other._BitsPerSample
            && _SubChunk2Size == other._SubChunk2Size;

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is Header other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hash_code = _ChunkID.GetItemsHashCode();
                hash_code = (hash_code * 397) ^ _ChunkSize;
                hash_code = (hash_code * 397) ^ _Format.GetItemsHashCode();
                hash_code = (hash_code * 397) ^ _SubChunk1Id.GetItemsHashCode();
                hash_code = (hash_code * 397) ^ _SubChunk1Size;
                hash_code = (hash_code * 397) ^ (int)_AudioFormat;
                hash_code = (hash_code * 397) ^ _ChannelsCount.GetHashCode();
                hash_code = (hash_code * 397) ^ _SampleRate;
                hash_code = (hash_code * 397) ^ _ByteRate;
                hash_code = (hash_code * 397) ^ _BlockAlign.GetHashCode();
                hash_code = (hash_code * 397) ^ _BitsPerSample.GetHashCode();
                hash_code = (hash_code * 397) ^ _SubChunk2Id.GetItemsHashCode();
                hash_code = (hash_code * 397) ^ _SubChunk2Size;
                return hash_code;
            }
        }

        /// <summary>Оператор проверки равенства двух заголовков</summary>
        public static bool operator ==(Header left, Header right) => left.Equals(right);

        /// <summary>Оператор проверки неравенства двух заголовков</summary>
        public static bool operator !=(Header left, Header right) => !left.Equals(right);

        /* ------------------------------------------------------------------------------------- */
    }
}