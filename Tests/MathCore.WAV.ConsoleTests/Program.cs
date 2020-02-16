using System;

namespace MathCore.WAV.ConsoleTests
{
    internal static class Program
    {
        private const string __TestDataPath = @"..\..\..\..\TestData\";
        private const string __TestDataFile = @"test.wav";
        private const string __TestDataFileSin100 = @"sin100.wav";

        public static void Main(string[] args)
        {
            using var wav = new WavFile();
            //var header = wav.LoadFrom(__TestDataPath + __TestDataFileSin100);
            var header = wav.LoadFrom(__TestDataPath + __TestDataFile);

            //var channel0 = wav.GetChannel(0);
            var data = wav.GetChannels();

            //var frames = new Frame[100];
            //var ch0 = new long[100];
            //var ch1 = new long[100];
            //for (var i = 0; i < frames.Length; i++)
            //{
            //    frames[i] = wav[i];
            //    ch0[i] = frames[i][0];
            //    ch1[i] = frames[i][1];
            //}

            Console.WriteLine("Hello World!");
        }
    }
}
