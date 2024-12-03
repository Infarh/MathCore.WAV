namespace MathCore.WAV;

public partial class WavFile
{
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
    {
        // Создание объекта WavFileWriter для записи данных в файл
        using var writer = new WavFileWriter(file, ChannelsCount, SampleRate, BitsPerSample);

        // Запись каждого сэмпла в файл
        foreach (var sample in Samples)
            writer.Write(sample);

        // Обновление информации о файле
        file.Refresh();
        return file;
    }
}
