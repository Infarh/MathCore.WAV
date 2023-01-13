using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using System.Text;

namespace MathCore.WAV.ConsoleTests.Extensions;

internal static class BinaryReaderEx
{
    public static IEnumerable<short> EnumerateShort(this BinaryReader reader)
    {
        var base_stream = reader.BaseStream;
        if(base_stream.CanSeek)
        {
            var length = base_stream.Length;
            while (base_stream.Position < length)
                yield return reader.ReadInt16();
        }
        else
        {
            var buffer = new byte[2];
            while(true)
                switch (reader.Read(buffer))
                {
                    case 2:
                        yield return MemoryMarshal.Read<short>(buffer);
                        break;
                    case 1:
                        if (reader.Read(buffer[1..]) != 1)
                            yield break;
                        yield return MemoryMarshal.Read<short>(buffer);
                        break;
                    default:
                        yield break;
                }
        }
    }

    public static BinaryReader CreateReaderBinary(this Stream stream, Encoding encoding = null, bool LeaveOpen = false) =>
        new(stream, encoding ?? Encoding.UTF8, LeaveOpen);
}
