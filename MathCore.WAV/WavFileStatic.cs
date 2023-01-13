namespace MathCore.WAV;

public partial class WavFile
{
    public static FileInfo Write(
        string FilePath,
        IEnumerable<short> Samples,
        short ChannelsCount = 1,
        int SampleRate = 44100,
        short BitsPerSample = 16)
        => Write(new FileInfo(FilePath), Samples, ChannelsCount, SampleRate, BitsPerSample);

    public static FileInfo Write(
        FileInfo file,
        IEnumerable<short> Samples,
        short ChannelsCount = 1,
        int SampleRate = 44100,
        short BitsPerSample = 16)
    {
        using var writer = new WavFileWriter(file, ChannelsCount, SampleRate, BitsPerSample);

        foreach (var sample in Samples)
            writer.Write(sample);

        file.Refresh();
        return file;
    }
}
