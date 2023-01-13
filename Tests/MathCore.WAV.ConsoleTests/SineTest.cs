namespace MathCore.WAV.ConsoleTests;

public static class SineTest
{
    private const string __TestDataPath = @"..\..\..\..\TestData\";
    private const string __TestDataFile = @"test.wav";
    private const string __TestDataFileSin100 = @"sin100.wav";

    private static void Load(string[] args, int index, ref double value)
    {
        if (args?.Length > index && double.TryParse(args[index], out var v))
            value = v;
    }

    public static void Run(string[] args)
    {
        var max_time = 5d; // sec.
        var a0 = 5d;
        var max_a0 = 5d;
        Load(args, 2, ref max_time);
        Load(args, 3, ref max_a0);

        double[] input_data;
        using (var test_wave = new WavFileWriter("test_data.wav") { Amplitude = max_a0 })
        {
            double fd = test_wave.SampleRate;
            var samples_count = (int)(max_time * fd);
            input_data = new double[samples_count];
            var dt = 1 / fd;

            var f0 = 1e3;
            Load(args, 0, ref f0);
            var w0 = 2 * Math.PI * f0;
            Load(args, 1, ref a0);

            Console.WriteLine("""
                Generating sin wave with
                      f0 : {0}Hz
                      A0 : {1}
                    A0max: {2}
                     time: {3}sec.
                """, 
                f0, a0, max_a0, max_time);

            for (var i = 0; i < samples_count; i++)
                test_wave.Write(input_data[i] = a0 * Math.Cos(w0 * i * dt));
        }

        if (args != null && args.Contains("start", StringComparer.OrdinalIgnoreCase))
            Process.Start(new ProcessStartInfo("test_data.wav") { UseShellExecute = true });

        var test = new WavFile("test_data.wav") { Amplitude = max_a0 };
        var test_data = test.GetChannelDouble(0);

        var error = input_data.Zip(test_data, (x, y) => x - y).ToArray();

        var e2 = error.Sum(e => e * e);

        Console.ReadLine();

        //var td = test.GetChannel(0);

        //var sin = new WavFile(__TestDataPath + __TestDataFileSin100);
        //var dat = new WavFile(__TestDataPath + __TestDataFile);

        ////var channel0 = wav.GetChannel(0);
        //var data = dat.EnumerateSamples().ToArray();

        ////var frames = new Frame[100];
        ////var ch0 = new long[100];
        ////var ch1 = new long[100];
        ////for (var i = 0; i < frames.Length; i++)
        ////{
        ////    frames[i] = wav[i];
        ////    ch0[i] = frames[i][0];
        ////    ch1[i] = frames[i][1];
        ////}

        //Console.WriteLine("Hello World!");
    }
}
