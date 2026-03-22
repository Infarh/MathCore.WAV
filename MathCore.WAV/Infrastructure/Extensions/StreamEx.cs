namespace MathCore.WAV.Infrastructure.Extensions;

/// <summary>
/// Предоставляет методы расширения для Stream.
/// </summary>
internal static class StreamEx
{
    /// <summary>
    /// Заполняет буфер данными из потока
    /// </summary>
    /// <param name="stream">Поток, из которого читаются данные</param>
    /// <param name="buffer">Буфер, который заполняется данными</param>
    /// <returns>Количество байтов, считанных в буфер</returns>
    public static int FillBuffer(this Stream stream, byte[] buffer)
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

    /// <summary>
    /// Асинхронно заполняет буфер данными из потока
    /// </summary>
    /// <param name="stream">Поток, из которого читаются данные</param>
    /// <param name="buffer">Буфер, который заполняется данными</param>
    /// <param name="Cancel">Токен отмены для отмены операции</param>
    /// <returns>Задача, представляющая асинхронную операцию чтения</returns>
    public static async Task<int> FillBufferAsync(this Stream stream, byte[] buffer, CancellationToken Cancel = default)
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

    [Obsolete("Используйте FillBuffer")]
    public static int FeelBuffer(this Stream stream, byte[] buffer) => FillBuffer(stream, buffer);

    [Obsolete("Используйте FillBufferAsync")]
    public static Task<int> FeelBufferAsync(this Stream stream, byte[] buffer, CancellationToken Cancel = default) => FillBufferAsync(stream, buffer, Cancel);
}
