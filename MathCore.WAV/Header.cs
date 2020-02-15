using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using MathCore.WAV.Service;

namespace MathCore.WAV
{
    [StructLayout(LayoutKind.Sequential, Size = Length, CharSet = CharSet.Auto, Pack = 1)]
    public struct Header
    {
        public static Header Create(BinaryReader reader) => new Header(reader);

        public static Header Create(Stream stream) => new Header(stream);

        public static Header Create(byte[] buffer) => buffer.ConvertToStructure<Header>();

        /// <summary>Длина заголовка</summary>
        public const int Length = 44;

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
        private readonly short _NumChannels;

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
        public short NumChannels => _NumChannels;

        /// <summary>Частота дискретизации</summary>
        public int SampleRate => _SampleRate;

        /// <summary>Количество байт на один фрейм (один отсчёт по всем кналам)</summary>
        public short BlockAlign => _BlockAlign;

        /// <summary>Количество бит в семпле (8, 16, 32, 64...)</summary>
        public short BitsPerSample => _BitsPerSample;

        public int BytesPerSample => _BitsPerSample >> 3;

        /// <summary>Количество байт области данных</summary>
        public int SubChunk2Size => _SubChunk2Size;

        /// <summary>Инициализация нового заголовка WAV-файла</summary>
        /// <param name="ChunkSize">Размер файла - 8 байт</param>
        /// <param name="SubChunk1Size">Оставшийся размер цепочки (для PCM = 16)</param>
        /// <param name="AudioFormat">Формат (по умолчанию PCM = 1)</param>
        /// <param name="NumChannels">Количество каналов</param>
        /// <param name="SampleRate">Частота дискретизации</param>
        /// <param name="ByteRate">Скорость передачи (байт/с)</param>
        /// <param name="BlockAlign">Количество байт на один фрейм (один отсчёт по всем кналам)</param>
        /// <param name="BitsPerSample">Количество бит в семпле (8, 16, 32, 64...)</param>
        /// <param name="SubChunk2Size">Количество байт области данных</param>
        public Header
        (
            int ChunkSize,
            int SubChunk1Size,
            Format AudioFormat,
            short NumChannels,
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
            _SubChunk1Id = Encoding.UTF8.GetBytes("fmt\0x20");
            _SubChunk1Size = SubChunk1Size;
            _AudioFormat = AudioFormat;
            _NumChannels = NumChannels;
            _SampleRate = SampleRate;
            _ByteRate = ByteRate;
            _BlockAlign = BlockAlign;
            _BitsPerSample = BitsPerSample;
            _SubChunk2Size = SubChunk2Size;
            _SubChunk2Id = Encoding.UTF8.GetBytes("data");
        }

        public Header(Stream stream) : this(new BinaryReader(stream)) { }

        public Header(BinaryReader reader)
        {
            var file_length = (reader.BaseStream as FileStream)?.Length ?? -1;
            if (file_length == 0) throw new ArgumentException("Попытка чтения пустого файла");
            if (file_length != -1 && file_length < 44) throw new FormatException("Размер файла недостаточен для хранения даже заголовка");

            _ChunkID = reader.ReadBytes(4); // RIFF 0..3 (4)
            if (Encoding.UTF8.GetString(_ChunkID) != "RIFF")
                throw new FormatException("Ошибка формата - отсутствует сигнатура RIFF в начале потока данных");
            _ChunkSize = reader.ReadInt32(); // 4..7 (4) = file_length - 8
            if (file_length != -1 && _ChunkSize != file_length - 8)
                throw new FormatException("Размер файла в заголовке определён неверно");
            _Format = reader.ReadBytes(4); // WAVE 8..11 (4)
            if (Encoding.UTF8.GetString(_Format) != "WAVE")
                throw new FormatException("Ошибка формата - отсутствует сигнатура WAVE в заголовке");
            _SubChunk1Id = reader.ReadBytes(4); // fmt\0x20 12..15 (4)
            if (Encoding.UTF8.GetString(_SubChunk1Id) != "fmt ")
                throw new FormatException("Ошибка формата - отсутствует сигнатура \"fmt \" в заголовке");
            _SubChunk1Size = reader.ReadInt32(); // 16..19 (4) = 16
            _AudioFormat = (Format)reader.ReadInt16(); // 20..21 (2)
            if (_AudioFormat == WAV.Format.PCM && _SubChunk1Size != 16)
                throw new FormatException("Размер оставшейся части заголовка PCM формата не равен 16");
            _NumChannels = reader.ReadInt16(); // 22..23 (2)
            if (_NumChannels <= 0) throw new FormatException("Ошибка формата заголовка: количество каналов не может быть меньше, либо равно 0");
            _SampleRate = reader.ReadInt32(); // 24..27 (4)
            _ByteRate = reader.ReadInt32(); // 28..31 (4)
            _BlockAlign = reader.ReadInt16(); // 32..33 (2)
            _BitsPerSample = reader.ReadInt16(); // 34..35 (2)
            _SubChunk2Id = reader.ReadBytes(4); // 36..39 (4)
            if (Encoding.UTF8.GetString(_SubChunk2Id) != "data") throw new FormatException("Ошибка формата - отсутствует сигнатура WAVE в заголовке");
            _SubChunk2Size = reader.ReadInt32(); // 40..43 (4) = file_length - 44
            if (file_length != -1 && _SubChunk2Size != file_length - Length)
                throw new FormatException("Ошибка формата заголовка: неверно указан размер области данных файла");
        }
    }
}
