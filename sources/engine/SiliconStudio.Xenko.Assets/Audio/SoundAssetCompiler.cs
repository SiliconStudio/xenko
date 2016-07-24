// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Xenko.Audio;
using SiliconStudio.Xenko.Native;

namespace SiliconStudio.Xenko.Assets.Audio
{
    public class SoundAssetCompiler : AssetCompilerBase<SoundAsset>
    {
        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, SoundAsset asset, AssetCompilerResult result)
        {
            if (!EnsureSourcesExist(result, asset, assetAbsolutePath))
                return;

            result.BuildSteps = new AssetBuildStep(AssetItem)
            {
                new DecodeSoundFileCommand(urlInStorage, asset)
            };
        }

        private class DecodeSoundFileCommand : AssetCommand<SoundAsset>
        {
            private readonly TagSymbol disableCompressionSymbol;

            public DecodeSoundFileCommand(string url, SoundAsset asset) : base(url, asset)
            {
                disableCompressionSymbol = RegisterTag(Builder.DoNotCompressTag, () => Builder.DoNotCompressTag);
            }

            private static int RunProcessAndGetOutput(string command, string parameters)
            {
                using (var process = Process.Start(
                    new ProcessStartInfo(command, parameters)
                    {
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }))
                {
                    if (process != null)
                    {
                        var @err = process.StandardError;
                        var @out = process.StandardOutput;
                        process.WaitForExit();

                        var error = err.ReadToEnd();
                        var output = @out.ReadToEnd();

                        return process.ExitCode;
                    }
                }

                return -1;
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                var assetManager = new ContentManager();

                // Get absolute path of asset source on disk
                var assetDirectory = AssetParameters.Source.GetParent();
                var assetSource = UPath.Combine(assetDirectory, AssetParameters.Source);

                var installationDir = DirectoryHelper.GetPackageDirectory("Xenko");
                var binDir = UPath.Combine(installationDir, new UDirectory("Bin"));
                binDir = UPath.Combine(binDir, new UDirectory("Windows-Direct3D11"));
                var ffmpeg = UPath.Combine(binDir, new UFile("ffmpeg.exe"));
                if (!File.Exists(ffmpeg))
                {
                    throw new AssetException("Failed to compile a sound asset, ffmpeg was not found.");
                }

                var channels = AssetParameters.Spatialized ? 1 : 2;
                var tempPcmFile = Path.GetTempFileName();
                var ret = RunProcessAndGetOutput(ffmpeg, $"-i \"{assetSource}\" -f f32le -acodec pcm_f32le -ac {channels} -ar {AssetParameters.SampleRate} -y \"{tempPcmFile}\"");
                if (ret != 0)
                {
                    File.Delete(tempPcmFile);
                    throw new AssetException($"Failed to compile a sound asset, ffmpeg failed to convert {assetSource}");
                }

                var encoder = new Celt(AssetParameters.SampleRate, CompressedSoundSource.SamplesPerFrame, channels, false);

                var uncompressed = CompressedSoundSource.SamplesPerFrame * channels * sizeof(short); //compare with int16 for CD quality comparison.. but remember we are dealing with 32 bit floats for encoding!!
                var target = (int)Math.Floor(uncompressed / (float)AssetParameters.CompressionRatio);

                var dataUrl = Url + "_Data";
                var newSound = new Sound
                {
                    CompressedDataUrl = dataUrl,
                    Channels = channels,
                    SampleRate = AssetParameters.SampleRate,
                    StreamFromDisk = AssetParameters.StreamFromDisk,
                    Spatialized = AssetParameters.Spatialized,
                };

                //make sure we don't compress celt data
                commandContext.AddTag(new ObjectUrl(UrlType.ContentLink, dataUrl), disableCompressionSymbol);

                var frameSize = CompressedSoundSource.SamplesPerFrame* channels;
                using (var reader = new BinaryReader(new FileStream(tempPcmFile, FileMode.Open, FileAccess.Read)))
                using (var outputStream = ContentManager.FileProvider.OpenStream(dataUrl, VirtualFileMode.Create, VirtualFileAccess.Write, VirtualFileShare.Read, StreamFlags.Seekable))
                {
                    var writer = new BinarySerializationWriter(outputStream);

                    var outputBuffer = new byte[target];
                    var buffer = new float[frameSize];
                    var count = 0;
                    for (;;)
                    {
                        if (count == frameSize) //flush
                        {
                            var len = encoder.Encode(buffer, outputBuffer);
                            writer.Write((short)len);
                            outputStream.Write(outputBuffer, 0, len);

                            count = 0;
                            Array.Clear(buffer, 0, frameSize);

                            newSound.NumberOfPackets++;
                            newSound.MaxPacketLength = Math.Max(newSound.MaxPacketLength, len);
                        }

                        buffer[count] = reader.ReadSingle();
                        count++;

                        if (reader.BaseStream.Position == reader.BaseStream.Length) break;
                    }

                    if (count > 0) //flush
                    {
                        var len = encoder.Encode(buffer, outputBuffer);
                        writer.Write((short)len);
                        outputStream.Write(outputBuffer, 0, len);

                        newSound.NumberOfPackets++;
                        newSound.MaxPacketLength = Math.Max(newSound.MaxPacketLength, len);
                    }
                }

                File.Delete(tempPcmFile);

                assetManager.Save(Url, newSound);

                return Task.FromResult(ResultStatus.Successful);
            }
        }
    }
}
