using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MathCore.WAV.Tests
{
    [TestClass]
    public class WavFileTests
    {
        private const string __TestDataPath = @"..\..\..\..\TestData\";
        private const string __TestDataFile = @"test.wav";
        private const string __TestDataFileSin100 = @"test.wav";


        [TestMethod]
        public void ReadFileTest()
        {
            using var wav = new WavFile();
            wav.LoadFrom(__TestDataPath + __TestDataFile);

        }
    }
}
