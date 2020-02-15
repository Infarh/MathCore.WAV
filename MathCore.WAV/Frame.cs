using System;
using System.Runtime.InteropServices;
// ReSharper disable UnusedMember.Global
// ReSharper disable ConvertToAutoProperty
// ReSharper disable ConvertToAutoPropertyWhenPossible

namespace MathCore.WAV
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct Frame
    {
        private readonly double _Time;
        private readonly int _ChannelsCount;
        private readonly byte[] _Data;
        private readonly int _BytesPerChannel;

        public double Time => _Time;
        public int ChannelsCount => _ChannelsCount;

        public long this[int i]
        {
            get
            {
                switch (_BytesPerChannel)
                {
                    case 1:
                        return _Data[i];
                    case 2:
                        return BitConverter.ToInt16(_Data, i * _BytesPerChannel);
                    case 4:
                        return BitConverter.ToInt32(_Data, i * _BytesPerChannel);
                    case 8:
                        return BitConverter.ToInt64(_Data, i * _BytesPerChannel);
                    default:
                        throw new NotSupportedException($"Размерность отсчёта {_BytesPerChannel} байт на канал не поддерживается");
                }
            }
        }

        public Frame(double Time, int ChannelsCount, byte[] data)
        {
            _Time = Time;
            _ChannelsCount = ChannelsCount;
            _Data = data;
            _BytesPerChannel = _Data.Length / _ChannelsCount;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var data = new long[_ChannelsCount];
            for (var i = 0; i < _ChannelsCount; i++)
                data[i] = this[i];

            return $"{TimeSpan.FromSeconds(Time)}:{string.Join("|", data)}";
        }
    }
}