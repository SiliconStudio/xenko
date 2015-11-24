#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
using System;
using System.Diagnostics;
using System.IO;
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
        private readonly string gameName;
        private int screenShots;

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

            gameName = Path.GetFileNameWithoutExtension(gamePath);

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
            socketMessageLayer.Send(new TapSimulationRequest { State = PointerState.Down, Coords = coords }).Wait();
            Console.WriteLine(@"Simulating tap down {0}.", coords);

            Thread.Sleep(timeDown);

            socketMessageLayer.Send(new TapSimulationRequest { State = PointerState.Up, Coords = coords, Delta = timeDown }).Wait();
            Console.WriteLine(@"Simulating tap up {0}.", coords);
        }

        public void Drag(Vector2 from, Vector2 target, TimeSpan timeToTarget, TimeSpan timeDown)
        {
            socketMessageLayer.Send(new TapSimulationRequest { State = PointerState.Down, Coords = from }).Wait();
            Console.WriteLine(@"Simulating tap down {0}.", from);

            //send 15 events per second?
            var sleepTime = TimeSpan.FromMilliseconds(1000/15.0);
            var watch = Stopwatch.StartNew();
            var start = watch.Elapsed;
            var end = watch.Elapsed + timeToTarget;
            Vector2 prev = from;
            while (true)
            {
                if (watch.Elapsed > timeToTarget)
                {
                    break;
                }

                float factor = (watch.Elapsed.Ticks - start.Ticks) / (float)(end.Ticks - start.Ticks);

                var current = Vector2.Lerp(from, target, factor);

                var delta = current - prev;

                socketMessageLayer.Send(new TapSimulationRequest { State = PointerState.Move, Coords = current, Delta = sleepTime, CoordsDelta = delta }).Wait();
                Console.WriteLine(@"Simulating tap update {0}.", current);

                prev = current;

                Thread.Sleep(sleepTime);
            }

            Thread.Sleep(timeDown);

            socketMessageLayer.Send(new TapSimulationRequest { State = PointerState.Up, Coords = target, Delta = watch.Elapsed, CoordsDelta = target - from }).Wait();
            Console.WriteLine(@"Simulating tap up {0}.", target);
        }

        public void TakeScreenshot()
        {
            socketMessageLayer.Send(new ScreenshotRequest { Filename = xenkoDir + "\\screenshots\\" + gameName + screenShots + ".png" }).Wait();
            Console.WriteLine(@"Screenshot requested.");
            screenShots++;
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