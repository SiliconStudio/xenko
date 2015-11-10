using System;
using System.Diagnostics;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Engine.Network;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Input.SimulatorExtensions;

namespace SiliconStudio.Xenko.Testing
{
    [DataContract]
    public class KeySimulationRequest
    {
        public Keys Key;
        public bool Down;
    }

    public class TestClient : IDisposable
    {
        private Game currentGame;

        public async Task StartClient(Game game)
        {
            currentGame = game;

            var url = $"/service/{XenkoVersion.CurrentAsText}/SiliconStudio.Xenko.SamplesTestServer.exe";

            var socketContext = await RouterClient.RequestServer(url);

            var socketMessageLayer = new SocketMessageLayer(socketContext, false);

            socketMessageLayer.AddPacketHandler<KeySimulationRequest>(request =>
            {
                if (request.Down)
                {
                    currentGame.Input.SimulateKeyDown(request.Key);
                }
                else
                {
                    currentGame.Input.SimulateKeyUp(request.Key);
                }
            });

            Task.Run(() => socketMessageLayer.MessageLoop());
        }

        public void Dispose()
        {
            
        }
    }
}
