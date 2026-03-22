using System.Runtime.CompilerServices;

namespace MathCore.WAV.Tests;

[TestClass]
public class WavFileTests
{
    private static string GetTempFilePath([CallerMemberName] string? TestName = null) =>
        Path.Combine(Path.GetTempPath(), $"MathCore.WAV.{TestName}.{Guid.NewGuid():N}.wav");

    [TestMethod]
    public void Indexer_ReadsLastFrame()
    {
        var file_name = GetTempFilePath();
        using (var writer = new WavFileWriter(file_name, BitsPerSample: 16))
        {
            writer.Write(1L);
            writer.Write(2L);
        }

        var wav = new WavFile(file_name);
        Assert.AreEqual(2L, wav[1][0]);
    }

    [TestMethod]
    public void ChannelAmplitude_For32Bit_IsCorrect()
    {
        var file_name = GetTempFilePath();
        using var writer = new WavFileWriter(file_name, BitsPerSample: 32);

        Assert.AreEqual(int.MaxValue, writer.ChannelAmplitude);
    }

    [TestMethod]
    public async Task WriteAsync_Double_ConvertsUsingAmplitudeResolution()
    {
        var file_name = GetTempFilePath();
        await using (var writer = new WavFileWriter(file_name, BitsPerSample: 16))
        {
            writer.Amplitude = 1;
            await writer.WriteAsync(0.5d);
        }

        var wav = new WavFile(file_name) { Amplitude = 1 };
        Assert.AreEqual(16384L, wav.GetChannel(0)[0]);
    }

    [TestMethod]
    public void WavStream_ReadsFromMemoryStream()
    {
        var file_name = GetTempFilePath();
        using (var writer = new WavFileWriter(file_name, BitsPerSample: 16))
            writer.Write(123L);

        var bytes = File.ReadAllBytes(file_name);
        using var memory_stream = new MemoryStream(bytes);
        using var wav_stream = new WavStream(memory_stream, LeaveOpen: true);

        Assert.AreEqual(123L, wav_stream.GetChannel(0)[0]);
    }
}