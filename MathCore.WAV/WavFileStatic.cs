namespace MathCore.WAV;

public partial class WavFile
{
    private static FileInfo WriteCore<TSample>(
        FileInfo file,
        IEnumerable<TSample> Samples,
        short ChannelsCount,
        int SampleRate,
        short BitsPerSample,
        Action<WavFileWriter, TSample> WriteSample)
    {
        if (file is null)
            throw new ArgumentNullException(nameof(file));

        if (Samples is null)
            throw new ArgumentNullException(nameof(Samples));

        if (WriteSample is null)
            throw new ArgumentNullException(nameof(WriteSample));

        using var writer = new WavFileWriter(file, ChannelsCount, SampleRate, BitsPerSample);
        foreach (var sample in Samples)
            WriteSample(writer, sample);

        file.Refresh();
        return file;
    }

    /// <summary>
    /// Записывает WAV файл по указанному пути.
    /// </summary>
    /// <param name="FilePath">Путь к файлу.</param>
    /// <param name="Samples">Коллекция сэмплов.</param>
    /// <param name="ChannelsCount">Количество каналов.</param>
    /// <param name="SampleRate">Частота дискретизации.</param>
    /// <param name="BitsPerSample">Количество бит на сэмпл.</param>
    /// <returns>Информация о файле.</returns>
    public static FileInfo Write(
        string FilePath,
        IEnumerable<short> Samples,
        short ChannelsCount = 1,
        int SampleRate = 44100,
        short BitsPerSample = 16)
        => Write(new FileInfo(FilePath), Samples, ChannelsCount, SampleRate, BitsPerSample);

    /// <summary>
    /// Записывает WAV файл по указанному объекту FileInfo.
    /// </summary>
    /// <param name="file">Объект FileInfo, представляющий файл.</param>
    /// <param name="Samples">Коллекция сэмплов.</param>
    /// <param name="ChannelsCount">Количество каналов.</param>
    /// <param name="SampleRate">Частота дискретизации.</param>
    /// <param name="BitsPerSample">Количество бит на сэмпл.</param>
    /// <returns>Информация о файле.</returns>
    public static FileInfo Write(
        FileInfo file,
        IEnumerable<short> Samples,
        short ChannelsCount = 1,
        int SampleRate = 44100,
        short BitsPerSample = 16)
        => WriteCore(file, Samples, ChannelsCount, SampleRate, BitsPerSample, static (writer, sample) => writer.Write(sample));

    /// <summary>
    /// Записывает WAV файл по указанному пути из массива сэмплов.
    /// </summary>
    /// <param name="FilePath">Путь к файлу.</param>
    /// <param name="Samples">Массив сэмплов.</param>
    /// <param name="ChannelsCount">Количество каналов.</param>
    /// <param name="SampleRate">Частота дискретизации.</param>
    /// <param name="BitsPerSample">Количество бит на сэмпл.</param>
    /// <returns>Информация о файле.</returns>
    public static FileInfo Write(
        string FilePath,
        short[] Samples,
        short ChannelsCount = 1,
        int SampleRate = 44100,
        short BitsPerSample = 16)
        => Write(new FileInfo(FilePath), Samples, ChannelsCount, SampleRate, BitsPerSample);

    /// <summary>
    /// Записывает WAV файл по указанному объекту FileInfo из массива сэмплов.
    /// </summary>
    /// <param name="file">Объект FileInfo, представляющий файл.</param>
    /// <param name="Samples">Массив сэмплов.</param>
    /// <param name="ChannelsCount">Количество каналов.</param>
    /// <param name="SampleRate">Частота дискретизации.</param>
    /// <param name="BitsPerSample">Количество бит на сэмпл.</param>
    /// <returns>Информация о файле.</returns>
    public static FileInfo Write(
        FileInfo file,
        short[] Samples,
        short ChannelsCount = 1,
        int SampleRate = 44100,
        short BitsPerSample = 16)
        => WriteCore(file, Samples, ChannelsCount, SampleRate, BitsPerSample, static (writer, sample) => writer.Write(sample));

    /// <summary>
    /// Записывает WAV файл по указанному пути из массива сэмплов int.
    /// </summary>
    /// <param name="FilePath">Путь к файлу.</param>
    /// <param name="Samples">Массив сэмплов.</param>
    /// <param name="ChannelsCount">Количество каналов.</param>
    /// <param name="SampleRate">Частота дискретизации.</param>
    /// <param name="BitsPerSample">Количество бит на сэмпл.</param>
    /// <returns>Информация о файле.</returns>
    public static FileInfo Write(
        string FilePath,
        int[] Samples,
        short ChannelsCount = 1,
        int SampleRate = 44100,
        short BitsPerSample = 16)
        => Write(new FileInfo(FilePath), Samples, ChannelsCount, SampleRate, BitsPerSample);

    /// <summary>
    /// Записывает WAV файл по указанному объекту FileInfo из массива сэмплов int.
    /// </summary>
    /// <param name="file">Объект FileInfo, представляющий файл.</param>
    /// <param name="Samples">Массив сэмплов.</param>
    /// <param name="ChannelsCount">Количество каналов.</param>
    /// <param name="SampleRate">Частота дискретизации.</param>
    /// <param name="BitsPerSample">Количество бит на сэмпл.</param>
    /// <returns>Информация о файле.</returns>
    public static FileInfo Write(
        FileInfo file,
        int[] Samples,
        short ChannelsCount = 1,
        int SampleRate = 44100,
        short BitsPerSample = 16)
        => WriteCore(file, Samples, ChannelsCount, SampleRate, BitsPerSample, static (writer, sample) => writer.Write(sample));

    /// <summary>
    /// Записывает WAV файл по указанному пути из массива сэмплов long.
    /// </summary>
    /// <param name="FilePath">Путь к файлу.</param>
    /// <param name="Samples">Массив сэмплов.</param>
    /// <param name="ChannelsCount">Количество каналов.</param>
    /// <param name="SampleRate">Частота дискретизации.</param>
    /// <param name="BitsPerSample">Количество бит на сэмпл.</param>
    /// <returns>Информация о файле.</returns>
    public static FileInfo Write(
        string FilePath,
        long[] Samples,
        short ChannelsCount = 1,
        int SampleRate = 44100,
        short BitsPerSample = 16)
        => Write(new FileInfo(FilePath), Samples, ChannelsCount, SampleRate, BitsPerSample);

    /// <summary>
    /// Записывает WAV файл по указанному объекту FileInfo из массива сэмплов long.
    /// </summary>
    /// <param name="file">Объект FileInfo, представляющий файл.</param>
    /// <param name="Samples">Массив сэмплов.</param>
    /// <param name="ChannelsCount">Количество каналов.</param>
    /// <param name="SampleRate">Частота дискретизации.</param>
    /// <param name="BitsPerSample">Количество бит на сэмпл.</param>
    /// <returns>Информация о файле.</returns>
    public static FileInfo Write(
        FileInfo file,
        long[] Samples,
        short ChannelsCount = 1,
        int SampleRate = 44100,
        short BitsPerSample = 16)
        => WriteCore(file, Samples, ChannelsCount, SampleRate, BitsPerSample, static (writer, sample) => writer.Write(sample));
}
