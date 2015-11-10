using System;
using System.Threading.Tasks;
using SiliconStudio.Xenko.ConnectionRouter;
using SiliconStudio.Xenko.Engine.Network;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Testing;

namespace SiliconStudio.Xenko.SamplesTestServer
{
    public class SamplesTestServer : RouterServiceServer
    {
        public SamplesTestServer() : base($"/service/{XenkoVersion.CurrentAsText}/SiliconStudio.Xenko.SamplesTestServer.exe")
        {
        }

        protected override async void HandleClient(SimpleSocket clientSocket, string url)
        {
            await AcceptConnection(clientSocket);

            var socketMessageLayer = new SocketMessageLayer(clientSocket, true);

            while (true)
            {
                await socketMessageLayer.Send(new KeySimulationRequest { Key = Keys.Space, Down = true });
                await Task.Delay(1000);
                await socketMessageLayer.Send(new KeySimulationRequest { Key = Keys.Space, Down = false });
                await Task.Delay(1000);
            }
        }
    }
}