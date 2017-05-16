// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.IO;

namespace Microsoft.DotNet.Archive
{
    static class CompressionUtility
    {
        enum MeasureBy
        {
            Input,
            Output
        }

        private const int CopyBufferSize = 81920;

        private static readonly string LzmaLocation = Path.Combine(Path.GetDirectoryName(typeof(CompressionUtility).Assembly.Location), "lzma.exe");

        private class LzmaProgress
        {
            private IProgress<ProgressReport> progress;
            private long totalSize;
            private string phase;
            private MeasureBy measureBy;

            public LzmaProgress(IProgress<ProgressReport> progress, string phase, long totalSize, MeasureBy measureBy)
            {
                this.progress = progress;
                this.totalSize = totalSize;
                this.phase = phase;
                this.measureBy = measureBy;
            }

            public void SetProgress(long inSize, long outSize)
            {
                progress.Report(phase, measureBy == MeasureBy.Input ? inSize : outSize, totalSize);
            }
        }

        public static void Compress(Stream inStream, Stream outStream, IProgress<ProgressReport> progress)
        {
            var processStartInfo = new ProcessStartInfo(LzmaLocation, "e -si -so");

            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardInput = true;

            var process = Process.Start(processStartInfo);
            if (process == null)
                throw new NotImplementedException($"Error starting {processStartInfo.FileName}");

            // Output header (5 bytes)
            for (int i = 0; i < 5; ++i)
            {
                var @byte = (byte)process.StandardOutput.BaseStream.ReadByte();
                outStream.WriteByte(@byte);
            }

            // Replace decompressed size (lzma doesn't know it since it is processing a stream)
            var inSize = inStream.CanSeek ? (inStream.Length - inStream.Position) : -1;
            for (int i = 0; i < 8; i++)
            {
                // Discarded
                process.StandardOutput.BaseStream.ReadByte();
                outStream.WriteByte((Byte)(inSize >> (8 * i)));
            }

            var lzmaProgress = inStream.CanSeek ? new LzmaProgress(progress, "Compressing", inSize, MeasureBy.Input) : null;
            lzmaProgress?.SetProgress(0, 0);

            // Encode
            inStream.CopyToAsync(process.StandardInput.BaseStream).ContinueWith(_ => process.StandardInput.Close());

            var copyBuffer = new byte[CopyBufferSize];
            int read;
            long totalRead = 0;
            while ((read = process.StandardOutput.BaseStream.Read(copyBuffer, 0, copyBuffer.Length)) != 0)
            {
                outStream.Write(copyBuffer, 0, read);
                lzmaProgress?.SetProgress(inSize - (inStream.Length - inStream.Position), outStream.Length);
            }

            process.WaitForExit();
            if (process.ExitCode != 0)
                throw new InvalidOperationException($"Error {process.ExitCode} when compressing using LZMA");

            lzmaProgress?.SetProgress(inSize, outStream.Length);
        }

        public static void Decompress(Stream inStream, Stream outStream, IProgress<ProgressReport> progress)
        {
            var processStartInfo = new ProcessStartInfo(LzmaLocation, "d -si -so");

            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardInput = true;

            var process = Process.Start(processStartInfo);
            if (process == null)
                throw new NotImplementedException($"Error starting {processStartInfo.FileName}");

            long compressedSize = inStream.CanSeek ? (inStream.Length - inStream.Position) : -1;
            var lzmaProgress = inStream.CanSeek ? new LzmaProgress(progress, "Decompressing", compressedSize, MeasureBy.Input) : null;
            lzmaProgress?.SetProgress(0, 0);

            // Decode
            inStream.CopyToAsync(process.StandardInput.BaseStream).ContinueWith(_ => process.StandardInput.Close());

            var copyBuffer = new byte[CopyBufferSize];
            int read;
            long totalRead = 0;
            while ((read = process.StandardOutput.BaseStream.Read(copyBuffer, 0, copyBuffer.Length)) != 0)
            {
                outStream.Write(copyBuffer, 0, read);
                totalRead += read;
                lzmaProgress?.SetProgress(compressedSize - (inStream.Length - inStream.Position), outStream.Length);
            }

            process.WaitForExit();
            if (process.ExitCode != 0)
                throw new InvalidOperationException($"Error {process.ExitCode} when decompressing using LZMA");

            lzmaProgress?.SetProgress(compressedSize, outStream.Length);
        }
    }
}
