#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
using System;
using System.Threading;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine.Network;
using SiliconStudio.Xenko.Input;

namespace SiliconStudio.Xenko.Testing
{
    public class GameTest : IDisposable
    {
        private readonly SocketMessageLayer socketMessageLayer;
        private readonly string xenkoDir;

        public GameTest(string gamePath, PlatformType platform)
        {
            var url = $"/service/{XenkoVersion.CurrentAsText}/SiliconStudio.Xenko.SamplesTestServer.exe";

            var socketContext = RouterClient.RequestServer(url).Result;

            var success = false;
            var message = "";
            var ev = new AutoResetEvent(false);

            socketMessageLayer = new SocketMessageLayer(socketContext, false);

            socketMessageLayer.AddPacketHandler<StatusMessageRequest>(request =>
            {
                success = !request.Error;
                message = request.Message;
                ev.Set();
            });

            socketMessageLayer.AddPacketHandler<LogRequest>(request =>
            {
                Console.WriteLine(request.Message);
            });

            var runTask = Task.Run(() => socketMessageLayer.MessageLoop());

            xenkoDir = Environment.GetEnvironmentVariable("SiliconStudioXenkoDir");

            socketMessageLayer.Send(new TestRegistrationRequest
            {
                Platform = (int)platform,
                Tester = true,
                Cmd = xenkoDir + "\\" + gamePath
            }).Wait();

            if (!ev.WaitOne(10000))
            {
                throw new Exception("Time out while launching the game");
            }

            if (!success)
            {
                throw new Exception("Failed: " + message);
            }

            Console.WriteLine(@"Game started. (message: " + message + @")");
        }

        public void KeyPress(Keys key, TimeSpan timeDown)
        {
            socketMessageLayer.Send(new KeySimulationRequest { Down = true, Key = key }).Wait();
            Console.WriteLine(@"Simulating key down {0}.", key);

            Thread.Sleep(timeDown);

            socketMessageLayer.Send(new KeySimulationRequest { Down = false, Key = key }).Wait();
            Console.WriteLine(@"Simulating key up {0}.", key);
        }

        public void Tap(Vector2 coords, TimeSpan timeDown)
        {
            socketMessageLayer.Send(new TapSimulationRequest { Down = true, Coords = coords }).Wait();
            Console.WriteLine(@"Simulating tap down {0}.", coords);

            Thread.Sleep(timeDown);

            socketMessageLayer.Send(new TapSimulationRequest { Down = false, Coords = coords }).Wait();
            Console.WriteLine(@"Simulating tap up {0}.", coords);
        }

        public void TakeScreenshot()
        {
            socketMessageLayer.Send(new ScreenshotRequest { Filename = xenkoDir + "\\screenshots\\JumpyJet.png" }).Wait();
            Console.WriteLine(@"Screenshot requested.");
        }

        public void Wait(TimeSpan sleepTime)
        {
            Thread.Sleep(sleepTime);
        }

        public void Dispose()
        {
            socketMessageLayer.Send(new TestEndedRequest()).Wait();
        }
    }
}
#endif