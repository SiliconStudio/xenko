// Copyright (c) 2014-2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Xenko.ConnectionRouter;
using SiliconStudio.Xenko.Engine.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using SiliconStudio.Xenko.Games.Testing;
using SiliconStudio.Xenko.Games.Testing.Requests;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.SamplesTestServer
{
    public class SamplesTestServer : RouterServiceServer
    {
        private class TestPair
        {
            public SocketMessageLayer TesterSocket;
            public SocketMessageLayer GameSocket;
        }

        private readonly Dictionary<string, TestPair> processes = new Dictionary<string, TestPair>();

        private readonly Dictionary<SocketMessageLayer, SocketMessageLayer> testerToGame = new Dictionary<SocketMessageLayer, SocketMessageLayer>();
        private readonly Dictionary<SocketMessageLayer, SocketMessageLayer> gameToTester = new Dictionary<SocketMessageLayer, SocketMessageLayer>();

        public SamplesTestServer() : base($"/service/{XenkoVersion.CurrentAsText}/SiliconStudio.Xenko.SamplesTestServer.exe")
        {
        }

        protected override async void HandleClient(SimpleSocket clientSocket, string url)
        {
            await AcceptConnection(clientSocket);

            var socketMessageLayer = new SocketMessageLayer(clientSocket, true);

            socketMessageLayer.AddPacketHandler<TestRegistrationRequest>(request =>
            {
                if (request.Tester)
                {
                    switch (request.Platform)
                    {
                        case (int)PlatformType.Windows:
                            {
                                Process process = null;
                                var debugInfo = "";
                                try
                                {
                                    var start = new ProcessStartInfo
                                    {
                                        WorkingDirectory = Path.GetDirectoryName(request.Cmd),
                                        FileName = request.Cmd,
                                    };
                                    start.EnvironmentVariables["SiliconStudioXenkoDir"] = Environment.GetEnvironmentVariable("SiliconStudioXenkoDir");
                                    start.UseShellExecute = false;

                                    debugInfo = "Starting process " + start.FileName + " with path " + start.WorkingDirectory;
                                    socketMessageLayer.Send(new LogRequest { Message = debugInfo }).Wait();
                                    process = Process.Start(start);
                                }
                                catch (Exception ex)
                                {
                                    socketMessageLayer.Send(new StatusMessageRequest { Error = true, Message = "Launch exception: " + ex.Message }).Wait();
                                }

                                if (process == null)
                                {
                                    socketMessageLayer.Send(new StatusMessageRequest { Error = true, Message = "Failed to start game process. " + debugInfo }).Wait();
                                }
                                else
                                {
                                    processes[request.GameAssembly] = new TestPair { TesterSocket = socketMessageLayer };
                                    socketMessageLayer.Send(new LogRequest { Message = "Process created, id: " + process.Id.ToString() }).Wait();
                                }
                                break;
                            }
                        case (int)PlatformType.Android:
                            {
                                Process process = null;                                
                                var debugInfo = "";
                                try
                                {
                                    process = Process.Start("cmd.exe", $"/C adb shell monkey -p {request.GameAssembly}.{request.GameAssembly} -c android.intent.category.LAUNCHER 1");
                                }
                                catch (Exception ex)
                                {
                                    socketMessageLayer.Send(new StatusMessageRequest { Error = true, Message = "Launch exception: " + ex.Message }).Wait();
                                }

                                if (process == null)
                                {
                                    socketMessageLayer.Send(new StatusMessageRequest { Error = true, Message = "Failed to start game process. " + debugInfo }).Wait();
                                }
                                else
                                {
                                    processes[request.GameAssembly] = new TestPair { TesterSocket = socketMessageLayer };
                                    socketMessageLayer.Send(new LogRequest { Message = "Process created, id: " + process.Id.ToString() }).Wait();
                                }
                                break;
                            }
                        case (int)PlatformType.iOS:
                            {
                                Process process = null;
                                var debugInfo = "";
                                try
                                {
                                    var start = new ProcessStartInfo
                                    {
                                        WorkingDirectory = $"{Environment.GetEnvironmentVariable("SiliconStudioXenkoDir")}\\Bin\\Windows-Direct3D11\\",
                                        FileName = $"{Environment.GetEnvironmentVariable("SiliconStudioXenkoDir")}\\Bin\\Windows-Direct3D11\\idevicedebug.exe",
                                        Arguments = $"run com.your-company.{request.GameAssembly}"
                                    };
                                    start.UseShellExecute = false;
                                    debugInfo = "Starting process " + start.FileName + " with path " + start.WorkingDirectory;
                                    process = Process.Start(start);
                                }
                                catch (Exception ex)
                                {
                                    socketMessageLayer.Send(new StatusMessageRequest { Error = true, Message = $"Launch exception: {ex.Message} info: {debugInfo}" }).Wait();
                                }

                                if (process == null)
                                {
                                    socketMessageLayer.Send(new StatusMessageRequest { Error = true, Message = "Failed to start game process. " + debugInfo }).Wait();
                                }
                                else
                                {
                                    processes[request.GameAssembly] = new TestPair { TesterSocket = socketMessageLayer };
                                    socketMessageLayer.Send(new LogRequest { Message = "Process created, id: " + process.Id.ToString() }).Wait();
                                }
                                break;
                            }
                    }
                }
                else //Game process
                {
                    TestPair pair;
                    if (processes.TryGetValue(request.GameAssembly, out pair))
                    {
                        pair.GameSocket = socketMessageLayer;
                        testerToGame[pair.TesterSocket] = pair.GameSocket;
                        gameToTester[pair.GameSocket] = pair.TesterSocket;
                        pair.TesterSocket.Send(new StatusMessageRequest { Error = false, Message = "Start" }).Wait();
                    }
                }
            });

            socketMessageLayer.AddPacketHandler<KeySimulationRequest>(request =>
            {
                var game = testerToGame[socketMessageLayer];
                game.Send(request).Wait();
            });

            socketMessageLayer.AddPacketHandler<TapSimulationRequest>(request =>
            {
                var game = testerToGame[socketMessageLayer];
                game.Send(request).Wait();
            });

            socketMessageLayer.AddPacketHandler<ScreenshotRequest>(request =>
            {
                var game = testerToGame[socketMessageLayer];
                game.Send(request).Wait();
            });

            socketMessageLayer.AddPacketHandler<TestEndedRequest>(request =>
            {
                var game = testerToGame[socketMessageLayer];
                game.Send(request).Wait();
                testerToGame.Remove(socketMessageLayer);
                ShellHelper.RunProcess("cmd.exe", "/C taskkill /IM idevicedebug.exe /f");
            });

            socketMessageLayer.AddPacketHandler<ScreenShotPayload>(request =>
            {
                var tester = gameToTester[socketMessageLayer];

                var imageData = new TestResultImage();
                var stream = new MemoryStream(request.Data);
                imageData.Read(new BinaryReader(stream));
                stream.Dispose();
                var resultFileStream = File.OpenWrite(request.FileName);
                imageData.Image.Save(resultFileStream, ImageFileType.Png);
                resultFileStream.Dispose();

                tester.Send(new ScreenshotStored()).Wait();
            });

            Task.Run(() => socketMessageLayer.MessageLoop());
        }
    }
}