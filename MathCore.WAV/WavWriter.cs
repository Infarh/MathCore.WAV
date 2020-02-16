using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
// ReSharper disable UnusedMember.Global
// ReSharper disable ConvertToAutoPropertyWhenPossible

namespace MathCore.WAV
{
    public class WavFileWriter : IDisposable
    {
        private readonly Stream _DataStream;

        /// <summary>Число каналов</summary>
        private readonly short _ChannelsCount;

        /// <summary>Частота дискретизации</summary>
        private readonly int _SampleRate;

        /// <summary>Количество байт на один фрейм (один отсчёт по всем каналам)</summary>
        private readonly short _BlockAlign;

        /// <summary>Число бит на канал</summary>
        private readonly short _BitsPerSample;

        /// <summary>Число каналов</summary>
        public int ChannelsCount => _ChannelsCount;

        /// <summary>Частота дискретизации</summary>
        public int SampleRate => _SampleRate;

        /// <summary>Период дискретизации</summary>
#pragma warning disable IDE1006 // Стили именования
        public double dt => 1d / _SampleRate;
#pragma warning restore IDE1006 // Стили именования

        /// <summary>Инициализация нового экземпляра <see cref="WavFileWriter"/></summary>
        /// <param name="FileName">Имя файла для записи данных</param>
        /// <param name="ChannelsCount">Число каналов (по умолчанию 1)</param>
        /// <param name="SampleRate">Частота дискретизации в Гц (по умолчанию 44'100 Гц, или 44,1кГц)</param>
        /// <param name="BitsPerSample">Число бит на канал (по умолчанию 16 бит на канал)</param>
        public WavFileWriter(string FileName, short ChannelsCount = 1, int SampleRate = 44100, short BitsPerSample = 16)
        {
            if (string.IsNullOrWhiteSpace(FileName))
                throw new ArgumentException($"Некорректное имя файла: {FileName ?? "<null>"}", nameof(FileName));
            if (ChannelsCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(ChannelsCount), "Число каналов должно быть больше 0");
            if (BitsPerSample <= 0)
                throw new ArgumentOutOfRangeException(nameof(BitsPerSample), "Число бит на канал должно быть больше 0");
            switch (BitsPerSample)
            {
                case 8:
                case 16:
                case 32:
                case 64:
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(BitsPerSample), BitsPerSample, "Поддерживается 8, 16, 32 и 64 бит на канал");
            }

            _ChannelsCount = ChannelsCount;
            _SampleRate = SampleRate;
            _BlockAlign = (short) (ChannelsCount * (BitsPerSample >> 3));
            _BitsPerSample = BitsPerSample;
            _WriteBuffer = new byte[_BlockAlign];
            _ChannelValues = new long[_ChannelsCount];

            _DataStream = File.Create(FileName);
            var header_bytes = new byte[Header.Length];
            _DataStream.Write(header_bytes, 0, header_bytes.Length);
        }

        private readonly long[] _ChannelValues;
        public double Write(params double[] Values)
        {
            for (var i = 0; i < _ChannelsCount; i++)
                _ChannelValues[i] = (long)Values[i];
            return Write(_ChannelValues);
        }

        private readonly byte[] _WriteBuffer;
        public double Write(params long[] Values)
        {
            if (Values is null) throw new ArgumentNullException(nameof(Values));
            if(Values.Length < _ChannelsCount) 
                throw new ArgumentException($"Число каналов в файле задано равным {_ChannelsCount}, а для записи передано {Values.Length} значений");

            var byte_per_sample = _BitsPerSample >> 3;
            if (byte_per_sample == 1 || byte_per_sample == 2 || byte_per_sample == 4 || byte_per_sample == 8)
                for (var channel = 0; channel < _ChannelsCount; channel++)
                    Buffer.BlockCopy(Values, channel << 3, _WriteBuffer, channel * byte_per_sample, byte_per_sample);
            else
                throw new ArgumentOutOfRangeException(nameof(_BitsPerSample), _BitsPerSample, "Поддерживается 8, 16, 32 и 64 бит на канал");

            _DataStream.Write(_WriteBuffer, 0, _BlockAlign);

            var pos = _DataStream.Length - Header.Length;
            return (double)pos / _BlockAlign / _SampleRate;
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected bool _Disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (_Disposed || !disposing) return;
            _Disposed = true;

            var header = new Header(
                (int)_DataStream.Length,
                _ChannelsCount,
                _SampleRate,
                _BlockAlign,
                _BitsPerSample,
                (int)_DataStream.Length - Header.Length);

            _DataStream.Seek(0, SeekOrigin.Begin);
            using (_DataStream)
            {
                using var writer = new BinaryWriter(_DataStream);
                header.WriteTo(writer);
            }
        }

        #endregion
    }
}
