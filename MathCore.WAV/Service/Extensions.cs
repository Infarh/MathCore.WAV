using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
// ReSharper disable ArgumentsStyleLiteral
// ReSharper disable ArgumentsStyleNamedExpression
// ReSharper disable ArgumentsStyleOther

namespace MathCore.WAV.Service
{
    internal static class Extensions
    {
        public static T ConvertToStructure<T>(this byte[] data, int offset = 0) where T : struct
        {
            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                return (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject() + offset, typeof(T));
            }
            finally
            {
                handle.Free();
            }
        }

        public static T FromStructure<T>(this Stream stream)
        {
            var count = Marshal.SizeOf(typeof(T));
            var buffer = new byte[count];
            stream.Read(buffer, 0, count);
            var handle = GCHandle.Alloc((object)buffer, GCHandleType.Pinned);
            try
            {
                return (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                handle.Free();
            }
        }

        public static TValue Using<T, TValue>(this T obj, Func<T, TValue> Selector)
            where T : IDisposable
        {
            using (obj) return Selector(obj);
        }

        public static async Task WriteAsync(this BinaryWriter Writer, byte[] buffer, CancellationToken Cancel = default)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            await Writer.BaseStream.WriteAsync(buffer, 0, buffer.Length, Cancel);
        }

        public static async Task WriteAsync(this BinaryWriter Writer, int value, CancellationToken Cancel = default) =>
            await Writer.BaseStream.WriteAsync(
                buffer: new[]
                {
                    (byte)value, 
                    (byte)(value >> 8), 
                    (byte)(value >> 16), 
                    (byte)(value >> 24)
                }, 
                offset: 0, 
                count: 4,
                cancellationToken: Cancel);

        public static async Task WriteAsync(this BinaryWriter Writer, short value, CancellationToken Cancel = default) =>
            await Writer.BaseStream.WriteAsync(
                buffer: new[]
                {
                    (byte)value,
                    (byte) ((uint) value >> 8)
                }, 
                offset: 0, 
                count: 2,
                cancellationToken: Cancel);
    }
}
