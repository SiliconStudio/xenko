using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Xenko.ConnectionRouter;
using SiliconStudio.Xenko.Engine.Network;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Testing;

namespace SiliconStudio.Xenko.SamplesTestServer
{
    public class SamplesTestServer : RouterServiceServer
    {
        private class TestProcess
        {
            public Process Process;
            public SocketMessageLayer TesterSocket;
            public SocketMessageLayer GameSocket;
            public string Filename;
        }

        private readonly Dictionary<string, TestProcess> processes = new Dictionary<string, TestProcess>(); 

        private readonly Dictionary<SocketMessageLayer, SocketMessageLayer> testerToGame = new Dictionary<SocketMessageLayer, SocketMessageLayer>(); 

        public SamplesTestServer() : base($"/service/{XenkoVersion.CurrentAsText}/SiliconStudio.Xenko.SamplesTestServer.exe")
        {
        }

        Stopwatch watch = new Stopwatch();

        protected override async void HandleClient(SimpleSocket clientSocket, string url)
        {
            await AcceptConnection(clientSocket);

            var socketMessageLayer = new SocketMessageLayer(clientSocket, true);

            socketMessageLayer.AddPacketHandler<TestRegistrationRequest>(request =>
            {
                if (request.Cmd == null) return;

                var filename = Path.GetFileName(request.Cmd);

                if (request.Tester)
                {
                    if (request.Platform == (int)PlatformType.Windows)
                    {                        
                        Process process = null;
                        try
                        {
                            var start = new ProcessStartInfo
                            {
                                WorkingDirectory = Path.GetDirectoryName(request.Cmd),
                                FileName = filename
                            };
                            process = Process.Start(start);
                        }
                        catch (Exception ex)
                        {
                            socketMessageLayer.Send(new StatusMessageRequest { Error = true, Message = ex.Message }).Wait();
                        }              
                        
                        if (process == null)
                        {
                            socketMessageLayer.Send(new StatusMessageRequest { Error = true, Message = "Failed to start game process." }).Wait();
                        }
                        else
                        {
                            processes[filename] = new TestProcess { Process = process, TesterSocket = socketMessageLayer, Filename = filename };
                        }
                    }
                }
                else
                {
                    TestProcess process;
                    if (!processes.TryGetValue(request.Cmd, out process))
                    {
                        socketMessageLayer.Send(new StatusMessageRequest { Error = true, Message = "Failed to find test process." }).Wait();
                    }
                    else
                    {
                        process.GameSocket = socketMessageLayer;
                        process.TesterSocket.Send(new StatusMessageRequest { Error = false, Message = "Start" }).Wait();

                        testerToGame[process.TesterSocket] = process.GameSocket;
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
                var proc = processes.First(x => x.Value.TesterSocket == socketMessageLayer);
                proc.Value.Process.Kill();
                processes.Remove(proc.Key);
                testerToGame.Remove(proc.Value.TesterSocket);
            });

            Task.Run(() => socketMessageLayer.MessageLoop());
        }
    }
}