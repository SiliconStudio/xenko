// Copyright (c) 2014-2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Engine.Network;
using SiliconStudio.Xenko.Games.Testing.Requests;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Input.Extensions;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SiliconStudio.Xenko.Graphics.Regression;

namespace SiliconStudio.Xenko.Games.Testing
{
    /// <summary>
    /// This game system will be automatically injected by the Module initialized when included in the build processing via msbuild
    /// The purpose is to simulate events within the game process and report errors and such to the GameTestingClient
    /// </summary>
    internal class GameTestingSystem : GameSystemBase
    {
        public static bool Initialized;

        private readonly ConcurrentQueue<Action> drawActions = new ConcurrentQueue<Action>();
        private SocketMessageLayer socketMessageLayer;

        public GameTestingSystem(IServiceRegistry registry) : base(registry)
        {
            DrawOrder = int.MaxValue;
            Enabled = true;
            Visible = true;
        }

        public override async void Initialize()
        {
            var game = (Game)Game;

            var url = $"/service/{XenkoVersion.CurrentAsText}/SiliconStudio.Xenko.SamplesTestServer.exe";

            var socketContext = await RouterClient.RequestServer(url);

            socketMessageLayer = new SocketMessageLayer(socketContext, false);

            socketMessageLayer.AddPacketHandler<KeySimulationRequest>(request =>
            {
                if (request.Down)
                {
                    game.Input.SimulateKeyDown(request.Key);
                }
                else
                {
                    game.Input.SimulateKeyUp(request.Key);
                }
            });

            socketMessageLayer.AddPacketHandler<TapSimulationRequest>(request =>
            {
                switch (request.State)
                {
                    case PointerState.Down:
                        game.Input.SimulateTapDown(request.Coords);
                        break;

                    case PointerState.Up:
                        game.Input.SimulateTapUp(request.Coords, request.CoordsDelta, request.Delta);
                        break;

                    case PointerState.Move:
                        game.Input.SimulateTapMove(request.Coords, request.CoordsDelta, request.Delta);
                        break;
                }
            });

            socketMessageLayer.AddPacketHandler<ScreenshotRequest>(request =>
            {
                drawActions.Enqueue(() =>
                {
                    SaveTexture(game.GraphicsDevice.Presenter.BackBuffer, request.Filename);
                });
            });

            socketMessageLayer.AddPacketHandler<TestEndedRequest>(request =>
            {
                socketMessageLayer.Context.Dispose();
                game.Exit();
                Quit();
            });

            var t = Task.Run(() => socketMessageLayer.MessageLoop());

            drawActions.Enqueue(async () =>
            {
                await socketMessageLayer.Send(new TestRegistrationRequest { GameAssembly = game.Settings.PackageName, Tester = false, Platform = (int)Platform.Type });
            });

            Initialized = true;

#if SILICONSTUDIO_PLATFORM_IOS || SILICONSTUDIO_PLATFORM_ANDROID || SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
            Console.WriteLine(@"Test initialized, waiting to start...");
#endif
        }

        public override void Draw(GameTime gameTime)
        {
            Action action;
            if (drawActions.TryDequeue(out action))
            {
                action();
            }
        }

        private void SaveTexture(Texture texture, string filename)
        {
            using (var image = texture.GetDataAsImage(Game.GraphicsContext.CommandList))
            {
                //Send to server and store to disk
                var imageData = new TestResultImage { CurrentVersion = "1.0", Frame = "0", Image = image, TestName = "" };
                var payload = new ScreenShotPayload { FileName = filename };
                var resultFileStream = new MemoryStream();
                var writer = new BinaryWriter(resultFileStream);
                imageData.Write(writer);

                Task.Run(() =>
                {
                    payload.Data = resultFileStream.ToArray();
                    payload.Size = payload.Data.Length;
                    socketMessageLayer.Send(payload).Wait();
                    resultFileStream.Dispose();
                });
            }
        }

#if SILICONSTUDIO_PLATFORM_IOS
        [DllImport("__Internal", EntryPoint = "exit")]
        public static extern void exit(int status);
#endif

        public static void Quit()
        {
#if SILICONSTUDIO_PLATFORM_ANDROID
            global::Android.OS.Process.KillProcess(global::Android.OS.Process.MyPid());
#elif SILICONSTUDIO_PLATFORM_IOS
            exit(0);
#endif
        }
    }
}
