using System;
using System.IO;
using System.Runtime.InteropServices;

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
    }
}
