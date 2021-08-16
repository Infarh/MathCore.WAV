using System;
using System.Runtime.InteropServices;

using MathCore.WAV.Service;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable ConvertToAutoProperty
// ReSharper disable ConvertToAutoPropertyWhenPossible
namespace MathCore.WAV
{
    /// <summary>Фрейм, представляющий массив байт данных в единицу времени файла</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct Frame : IEquatable<Frame>
    {
        /* ------------------------------------------------------------------------------------- */

        /// <summary>Значение времени отсчёта</summary>
        private readonly double _Time;

        /// <summary>Число каналов</summary>
        private readonly int _ChannelsCount;

        /// <summary>Массив байт данных фрейма</summary>
        private readonly byte[] _Data;

        /// <summary>Число байт на один канал</summary>
        private readonly int _BytesPerChannel;

        /* ------------------------------------------------------------------------------------- */

        /// <summary>Значение времени отсчёта</summary>
        public double Time => _Time;

        /// <summary>Число каналов</summary>
        public int ChannelsCount => _ChannelsCount;

        /// <summary>Получение значения канала по его индексу в фрейме</summary>
        /// <param name="channel">Индекс канала</param>
        /// <exception cref="NotSupportedException">Если длина канала не равна 8, 16, 32, 64 бита</exception>
        public long this[int channel] =>
            _BytesPerChannel switch
            {
                1 => _Data[channel],
                2 => BitConverter.ToInt16(_Data, channel * _BytesPerChannel),
                4 => BitConverter.ToInt32(_Data, channel * _BytesPerChannel),
                8 => BitConverter.ToInt64(_Data, channel * _BytesPerChannel),
                _ => throw new NotSupportedException($"Размерность отсчёта {_BytesPerChannel} байт на канал не поддерживается"),
            };

        /* ------------------------------------------------------------------------------------- */

        /// <summary>Инициализация нового экземпляра <see cref="Frame"/></summary>
        /// <param name="Time">Значение времени отсчёта</param>
        /// <param name="ChannelsCount">Число каналов</param>
        /// <param name="data">Массив байт данных фрейма</param>
        public Frame(double Time, int ChannelsCount, byte[] data)
        {
            _Time = Time;
            _ChannelsCount = ChannelsCount;
            _Data = data;
            _BytesPerChannel = _Data.Length / _ChannelsCount;
        }

        /* ------------------------------------------------------------------------------------- */

        /// <inheritdoc />
        public override string ToString()
        {
            var data = new long[_ChannelsCount];
            for (var i = 0; i < _ChannelsCount; i++)
                data[i] = this[i];

            return $"{TimeSpan.FromSeconds(Time)}#{string.Join("|", data)}";
        }

        /// <inheritdoc />
        public bool Equals(Frame other) =>
            _Time.Equals(other._Time)
            && _ChannelsCount == other._ChannelsCount
            && Equals(_Data, other._Data)
            && _BytesPerChannel == other._BytesPerChannel;

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is Frame other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hash_code = _Time.GetHashCode();
                hash_code = (hash_code * 397) ^ _ChannelsCount;
                hash_code = (hash_code * 397) ^ _Data.GetItemsHashCode();
                hash_code = (hash_code * 397) ^ _BytesPerChannel;
                return hash_code;
            }
        }

        /* ------------------------------------------------------------------------------------- */

        /// <summary>Оператор проверки равенства двух фреймов</summary>
        public static bool operator ==(Frame left, Frame right) => left.Equals(right);

        /// <summary>Оператор проверки неравенства двух фреймов</summary>
        public static bool operator !=(Frame left, Frame right) => !left.Equals(right);

        /* ------------------------------------------------------------------------------------- */
    }
}