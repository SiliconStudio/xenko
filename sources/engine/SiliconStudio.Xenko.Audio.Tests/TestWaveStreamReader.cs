// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.IO;

using NUnit.Framework;

using SiliconStudio.Xenko.Audio.Wave;

namespace SiliconStudio.Xenko.Audio.Tests
{
    /// <summary>
    /// Tests for <see cref="SoundEffect"/> and <see cref="SoundEffectInstance"/>.
    /// </summary>
    [TestFixture]
    public class TestWaveStreamReader
    {
        [TestFixtureSetUp]
        public void Initialize()
        {
        }

        readonly byte[] riffHeader =
            {   
                0x52, 0x49, 0x46, 0x46, // RIFFF
                0x26, 0x00, 0x00, 0x00, // size
                0x57, 0x41, 0x56, 0x45  // WAVE
            };

        private readonly byte[] waveFmt =
            {
                0x66, 0x6D, 0x74, 0x20, // "fmt "
                0x12, 0x00, 0x00, 0x00, // size = 18
                0x01, 0x00, // audio format = 1 (PCM)
                0x02, 0x00, // nb channels =  2 (Stereo)
                0x80, 0xBB, 0x00, 0x00, // sample rate = 48000
                0x00, 0xEE, 0x02, 0x00, // byte rate = 192000
                0x02, 0x00, // block align = 2
                0x10, 0x00, //bits per sample = 16
                0x00, 0x00 // not used
            };

        private readonly byte[] emptyData =
            {
                0x64, 0x61, 0x74, 0x61, // data
                0x00, 0x00, 0x00, 0x00  // size = 0
            };

        /// <summary>
        /// Test that wave header is read correctly
        /// </summary>
        [Test]
        public void WaveHeaderTest()
        {
            const uint TotalSize = 38;
            riffHeader[4] = (byte)((TotalSize & 0x000000FF) >> 0);
            riffHeader[5] = (byte)((TotalSize & 0x0000FF00) >> 8);
            riffHeader[6] = (byte)((TotalSize & 0x00FF0000) >> 16);
            riffHeader[7] = (byte)((TotalSize & 0xFF000000) >> 24);

            using (var handMadeWaveStream = new MemoryStream())
            {
                handMadeWaveStream.Write(riffHeader, 0, riffHeader.Length);
                handMadeWaveStream.Write(waveFmt, 0, waveFmt.Length);
                handMadeWaveStream.Write(emptyData, 0, emptyData.Length);
                handMadeWaveStream.Seek(0, SeekOrigin.Begin);

                SoundStream waveStreamReader = null;
                Assert.DoesNotThrow(() => waveStreamReader = new SoundStream(handMadeWaveStream), "An error happened while reading of the wave file header.");

                var waveFormat = waveStreamReader.Format;
                Assert.AreEqual(waveFmt[8] + (waveFmt[9]<<8), (int)waveFormat.Encoding, "Audio formats do not match");
                Assert.AreEqual(waveFmt[10] + (waveFmt[11] << 8), waveFormat.Channels, "Channel numbers do not match");
                Assert.AreEqual(waveFmt[12] + (waveFmt[13] << 8) + (waveFmt[14] << 16) + (waveFmt[15] << 24), waveFormat.SampleRate, "Sample rates do not match");
                Assert.AreEqual(waveFmt[16] + (waveFmt[17] << 8) + (waveFmt[18] << 16) + (waveFmt[19] << 24), waveFormat.AverageBytesPerSecond, "Byte rates do not match");
                Assert.AreEqual(waveFmt[20] + (waveFmt[21] << 8), waveFormat.BlockAlign, "Block aligns do not match");
                Assert.AreEqual(waveFmt[22] + (waveFmt[23] << 8), waveFormat.BitsPerSample, "Bits per samples do not match");
            }
        }

        /// <summary>
        /// Test that wave data is read correctly
        /// </summary>
        [Test]
        public void WaveDataTest()
        {
            const uint DataSize = 4 * 512;
            const uint TotalSize = DataSize + 38;

            riffHeader[4] = (byte)((TotalSize & 0x000000FF) >> 0);
            riffHeader[5] = (byte)((TotalSize & 0x0000FF00) >> 8);
            riffHeader[6] = (byte)((TotalSize & 0x00FF0000) >> 16);
            riffHeader[7] = (byte)((TotalSize & 0xFF000000) >> 24);

            using (var handMadeWaveStream = new MemoryStream())
            {
                handMadeWaveStream.Write(riffHeader, 0, riffHeader.Length);
                handMadeWaveStream.Write(waveFmt, 0, waveFmt.Length);

                // generate the data header
                var dataHeader = new byte[]
                    {
                        0x64, 0x61, 0x74, 0x61, // data
                        (byte)((DataSize & 0x000000FF) >> 0), (byte)((DataSize & 0x0000FF00) >> 8), (byte)((DataSize & 0x00FF0000) >> 16), (byte)((DataSize & 0xFF000000) >> 24)
                    };
                handMadeWaveStream.Write(dataHeader, 0, dataHeader.Length);

                // generate the data
                var data = new byte[DataSize];
                for (var i = 0; i < data.Length; ++i)
                    data[i] = (byte)i;
                handMadeWaveStream.Write(data, 0, data.Length);


                handMadeWaveStream.Seek(0, SeekOrigin.Begin);

                SoundStream waveStreamReader = null;
                Assert.DoesNotThrow(() => waveStreamReader = new SoundStream(handMadeWaveStream), "An error happened while reading of the wave file.");

                var readData = new byte[waveStreamReader.Length];
                waveStreamReader.Read(readData, 0, (int)waveStreamReader.Length);

                Assert.AreEqual(data, readData, "Expected data and read data are not the same.");
            }
        }
    }
}
