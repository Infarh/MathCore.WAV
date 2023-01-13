namespace MathCore.WAV.Infrastructure.Extensions;

internal static class StreamEx
{
    public static int FeelBuffer(this Stream stream, byte[] buffer)
    {
        var length = buffer.Length;
        var readed = stream.Read(buffer, 0, length);
        if (readed == length)
            return readed;

        while (readed < buffer.Length)
        {
            var r2 = stream.Read(buffer, readed, length - readed);
            if (r2 == 0)
                return readed;

            readed += r2;
        }

        return readed;
    }

    public static async Task<int> FeelBufferAsync(this Stream stream, byte[] buffer, CancellationToken Cancel = default)
    {
        var length = buffer.Length;
        var readed = await stream.ReadAsync(buffer, 0, length, Cancel).ConfigureAwait(false);
        if (readed == length)
            return readed;

        while (readed < buffer.Length)
        {
            var r2 = await stream.ReadAsync(buffer, readed, length - readed, Cancel).ConfigureAwait(false);
            if (r2 == 0)
                return readed;

            readed += r2;
        }

        return readed;
    }
}
