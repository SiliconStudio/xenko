using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Engine.Network;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Input.Extensions;

namespace SiliconStudio.Xenko.Testing
{
    public class TestClient : GameSystemBase
    {
        protected void SaveTexture(Texture texture, string filename)
        {
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
            using (var image = texture.GetDataAsImage())
            {
                using (var resultFileStream = File.OpenWrite(filename))
                {
                    image.Save(resultFileStream, ImageFileType.Png);
                }
            }
#endif
        }

        public async Task StartClient(Game game)
        {
            game.GameSystems.Add(this);

            var url = $"/service/{XenkoVersion.CurrentAsText}/SiliconStudio.Xenko.SamplesTestServer.exe";

            var socketContext = await RouterClient.RequestServer(url);

            var socketMessageLayer = new SocketMessageLayer(socketContext, false);
            
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

            Task.Run(() => socketMessageLayer.MessageLoop());

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
            await socketMessageLayer.Send(new TestRegistrationRequest { Cmd = AppDomain.CurrentDomain.FriendlyName, Tester = false, Platform = (int)PlatformType.Windows });
#endif
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