using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Engine.Network;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Input.Extensions;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace SiliconStudio.Xenko.Testing
{
    public class TestClient : GameSystemBase
    {
        protected void SaveTexture(Texture texture, string filename)
        {
            using (var image = texture.GetDataAsImage())
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

        private static void Quit(Game game)
        {
            game.Exit();

#if SILICONSTUDIO_PLATFORM_ANDROID
            Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
#endif
        }

        private SocketMessageLayer socketMessageLayer;

        public async Task StartClient(Game game, string gameName)
        {
            game.GameSystems.Add(this);

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
                    SaveTexture(game.GraphicsDevice.BackBuffer, request.Filename);
                });
            });

            socketMessageLayer.AddPacketHandler<TestEndedRequest>(request =>
            {
                Quit(game);
            });

            Task.Run(() => socketMessageLayer.MessageLoop());

            await socketMessageLayer.Send(new TestRegistrationRequest { GameAssembly = gameName, Tester = false, Platform = (int)Platform.Type });

            //Quit after 1 minute anyway!
            Task.Run(async () =>
            {
                await Task.Delay(60000);
                Quit(game);
            });
        }

        private readonly ConcurrentQueue<Action> drawActions = new ConcurrentQueue<Action>();

        public TestClient(IServiceRegistry registry) : base(registry)
        {
            DrawOrder = int.MaxValue;
            Enabled = true;
            Visible = true;
        }

        public override void Draw(GameTime gameTime)
        {
            Action action;
            if (drawActions.TryDequeue(out action))
            {
                action();
            }
        }
    }
}