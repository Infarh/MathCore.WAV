namespace MathCore.WAV.Infrastructure.Extensions;

internal static class StreamEx
{
    public static int FeelBuffer(this Stream stream, byte[] buffer)
    {
        var length = buffer.Length;
        var bytes_count = stream.Read(buffer, 0, length);
        if (bytes_count == length)
            return bytes_count;

        while (bytes_count < buffer.Length)
        {
            var count = stream.Read(buffer, bytes_count, length - bytes_count);
            if (count == 0)
                return bytes_count;

            bytes_count += count;
        }

        return bytes_count;
    }

    public static async Task<int> FeelBufferAsync(this Stream stream, byte[] buffer, CancellationToken Cancel = default)
    {
        var length = buffer.Length;
        var bytes_count = await stream.ReadAsync(buffer, 0, length, Cancel).ConfigureAwait(false);
        if (bytes_count == length)
            return bytes_count;

        while (bytes_count < buffer.Length)
        {
            var count = await stream.ReadAsync(buffer, bytes_count, length - bytes_count, Cancel).ConfigureAwait(false);
            if (count == 0)
                return bytes_count;

            bytes_count += count;
        }

        return bytes_count;
    }
}
