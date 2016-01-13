// Copyright (c) 2014-2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine.Network;
using SiliconStudio.Xenko.Games.Testing.Requests;
using SiliconStudio.Xenko.Input;

namespace SiliconStudio.Xenko.Games.Testing
{
    /// <summary>
    /// This class is to be consumed by Unit tests, see samples/Tests/Tests.sln
    /// It will send requests to the router which in turn will route them to the running game
    /// </summary>
    public class GameTestingClient : IDisposable
    {
        private readonly SocketMessageLayer socketMessageLayer;
        private readonly string xenkoDir;
        private readonly string gameName;
        private readonly string platformName;
        private int screenShots;

        private readonly AutoResetEvent screenshotEvent = new AutoResetEvent(false);

        public GameTestingClient(string gamePath, PlatformType platform)
        {
            GameTestingSystem.Initialized = true; //prevent time-outs from test side!!

            if (gamePath == null) throw new ArgumentNullException(nameof(gamePath));

            xenkoDir = Environment.GetEnvironmentVariable("SiliconStudioXenkoDir");
            if(xenkoDir.IsNullOrEmpty()) throw new NullReferenceException("Could not find SiliconStudioXenkoDir, make sure the environment variable is set.");

            gameName = Path.GetFileNameWithoutExtension(gamePath);
            switch (platform)
            {
                case PlatformType.Windows:
                    platformName = "Windows";
                    break;
                case PlatformType.WindowsPhone:
                    platformName = "WindowsPhone";
                    break;
                case PlatformType.WindowsStore:
                    platformName = "WindowsStore";
                    break;
                case PlatformType.Android:
                    platformName = "Android";
                    break;
                case PlatformType.iOS:
                    platformName = "iOS";
                    break;
                case PlatformType.Windows10:
                    platformName = "Windows10";
                    break;
                default:
                    platformName = "";
                    break;
            }

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

            socketMessageLayer.AddPacketHandler<LogRequest>(request => { Console.WriteLine(request.Message); });

            socketMessageLayer.AddPacketHandler<ScreenshotStored>(request =>
            {
                screenshotEvent.Set();
            });

            var runTask = Task.Run(() => socketMessageLayer.MessageLoop());

            var cmd = platform == PlatformType.Windows ? xenkoDir + "\\" + gamePath : "";

            socketMessageLayer.Send(new TestRegistrationRequest
            {
                Platform = (int)platform, Tester = true, Cmd = cmd, GameAssembly = gameName
            }).Wait();

            var waitMs = 10000;
            switch (platform)
            {
                case PlatformType.Android:
                    waitMs = 20000;
                    break;
                case PlatformType.iOS:
                    waitMs = 40000;
                    break;
            }

            if (!ev.WaitOne(waitMs))
            {
                socketMessageLayer.Send(new TestAbortedRequest()).Wait();
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

                float factor = (watch.Elapsed.Ticks - start.Ticks)/(float)(end.Ticks - start.Ticks);

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
            socketMessageLayer.Send(new ScreenshotRequest { Filename = xenkoDir + "\\screenshots\\" + gameName + "_" + platformName + "_" + screenShots + ".png" }).Wait();
            Console.WriteLine(@"Screenshot requested.");
            screenShots++;
            if (!screenshotEvent.WaitOne(10000))
            {
                throw new Exception(@"Failed to store screenshot.");
            }
        }

        public void Wait(TimeSpan sleepTime)
        {
            Thread.Sleep(sleepTime);
        }

        public void Dispose()
        {
            Console.WriteLine(@"Ending the test.");
            socketMessageLayer.Send(new TestEndedRequest()).Wait();
        }
    }
}

#endif