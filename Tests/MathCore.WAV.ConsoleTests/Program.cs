using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MathCore.WAV.ConsoleTests
{
    internal static class Program
    {
        private const string __TestDataPath = @"..\..\..\..\TestData\";
        private const string __TestDataFile = @"test.wav";
        private const string __TestDataFileSin100 = @"sin100.wav";

        public static void Main(string[] args)
        {
            const double max_time = 5; // sec.
            using (var test_wave = new WavFileWriter("test_data.wav", 1))
            {
                double fd = test_wave.SampleRate;
                var samples_count = (int)(max_time * fd);
                var dt = 1 / fd;

                var f0 = 1e3;
                if (args?.Length >= 1 && double.TryParse(args[0], out var vf0))
                    f0 = vf0;
                var w0 = 2 * Math.PI * f0;
                var a0 = 1000d;
                if (args?.Length >= 2 && double.TryParse(args[1], out var va0))
                    a0 = va0;

                Console.WriteLine("Generating sin wave with\r\n\tf0:{0}Hz\r\n\tA0:{1}", f0, a0);

                for (var i = 0; i < samples_count; i++)
                    test_wave.Write((long)(a0 * Math.Sin(w0 * i * dt)));
            }

            if (args != null && args.Contains("start", StringComparer.OrdinalIgnoreCase))
                Process.Start(new ProcessStartInfo("test_data.wav") { UseShellExecute = true });

            //var test = new WavFile("test_data.wav");
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
}
