// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Xenko.Audio;
using SiliconStudio.Xenko.Native;

namespace SiliconStudio.Xenko.Assets.Audio
{
    public class SoundAssetCompiler : AssetCompilerBase<SoundAsset>
    {
        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, SoundAsset asset, AssetCompilerResult result)
        {
            if (!EnsureSourceExists(result, asset, assetAbsolutePath))
                return;

            result.BuildSteps = new AssetBuildStep(AssetItem)
            {
                new DecodeSoundFileCommand(urlInStorage, asset)
            };
        }

        private class DecodeSoundFileCommand : AssetCommand<SoundAsset>
        {
            public DecodeSoundFileCommand(string url, SoundAsset asset) : base(url, asset)
            {             
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

                var installationDir = DirectoryHelper.GetInstallationDirectory("Xenko");
                var binDir = UPath.Combine(installationDir, new UDirectory("Bin"));
                binDir = UPath.Combine(binDir, new UDirectory("Windows-Direct3D11"));
                var ffmpeg = UPath.Combine(binDir, new UFile("ffmpeg.exe"));
                if (!File.Exists(ffmpeg))
                {
                    throw new AssetException("Failed to compile a sound asset, ffmpeg was not found.");
                }

                var tempPcmFile = Path.GetTempFileName();
                //todo add samplerate control and maybe channels too?
                var ret = RunProcessAndGetOutput(ffmpeg, $"-i \"{assetSource}\" -f f32le -acodec pcm_f32le -y \"{tempPcmFile}\"");
                if (ret != 0)
                {
                    File.Delete(tempPcmFile);
                    throw new AssetException($"Failed to compile a sound asset, ffmpeg failed to convert {assetSource}");
                }

                var sr = 44100;
                var bsize = 1024;
                var channels = 2;
                var encoder = new Celt(sr, bsize, channels, false);

                var uncompressed = bsize * channels * sizeof(short);
                var target = (int)Math.Floor(uncompressed / (float)AssetParameters.CompressionRatio);

                var newSound = new SoundEffect();
                newSound.Packets = new List<CompressedSoundPacket>();

                using (var reader = new BinaryReader(new FileStream(tempPcmFile, FileMode.Open, FileAccess.Read)))
                {
                    var outputBuffer = new byte[target];
                    var buffer = new float[2048];
                    var count = 0;
                    for (;;)
                    {
                        if (count == 2048) //flush
                        {
                            var len = encoder.Encode(buffer, outputBuffer);
                            var packet = new CompressedSoundPacket { Data = new byte[len], Length = len };
                            Array.Copy(outputBuffer, packet.Data, packet.Length);
                            newSound.Packets.Add(packet);
                            Array.Clear(buffer, 0, 2048);
                            count = 0;
                        }

                        try
                        {
                            buffer[count] = reader.ReadSingle();
                        }
                        catch (EndOfStreamException)
                        {
                            break;
                        }
                        count++;
                    }

                    if (count > 0) //flush
                    {
                        var len = encoder.Encode(buffer, outputBuffer);
                        var packet = new CompressedSoundPacket { Data = new byte[len], Length = len };
                        Array.Copy(outputBuffer, packet.Data, packet.Length);
                        newSound.Packets.Add(packet);
                    }
                }

                File.Delete(tempPcmFile);

                assetManager.Save(Url, newSound);

                return Task.FromResult(ResultStatus.Successful);
            }
        }
    }
}
