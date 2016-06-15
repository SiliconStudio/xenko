using System;
using NUnit.Framework;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games.Testing;

namespace ForwardLightingTest
{
    [TestFixture]
    public class ForwardLightingTest
    {
        private const string Path = "samples\\Graphics\\ForwardLighting\\Bin\\Windows-Direct3D11\\Debug\\ForwardLighting.exe";

#if TEST_ANDROID
        private const PlatformType TestPlatform = PlatformType.Android;
#elif TEST_IOS
        private const PlatformType TestPlatform = PlatformType.iOS;
#else
        private const PlatformType TestPlatform = PlatformType.Windows;
#endif

        [Test]
        public void TestLaunch()
        {
            using (var game = new GameTestingClient(Path, TestPlatform))
            {
                game.Wait(TimeSpan.FromMilliseconds(2000));
            }
        }

        [Test]
        public void TestInputs()
        {
            using (var game = new GameTestingClient(Path, TestPlatform))
            {
                game.Wait(TimeSpan.FromMilliseconds(2000));

                //Turn on all shadows first
                game.Tap(new Vector2(0.07f, 0.09f), TimeSpan.FromMilliseconds(500));
                game.Tap(new Vector2(0.07f, 0.2f), TimeSpan.FromMilliseconds(500));
                game.Tap(new Vector2(0.07f, 0.4f), TimeSpan.FromMilliseconds(500));
                game.Wait(TimeSpan.FromMilliseconds(1000));
                game.TakeScreenshot();
                game.Wait(TimeSpan.FromMilliseconds(500));

                //Turn off some lights
                game.Tap(new Vector2(0.07f, 0.15f), TimeSpan.FromMilliseconds(500));
                game.Tap(new Vector2(0.07f, 0.27f), TimeSpan.FromMilliseconds(500));
                game.Tap(new Vector2(0.07f, 0.32f), TimeSpan.FromMilliseconds(500));
                game.Wait(TimeSpan.FromMilliseconds(1000));
                game.TakeScreenshot();
                game.Wait(TimeSpan.FromMilliseconds(500));

                //Turn off some lights
                game.Tap(new Vector2(0.07f, 0.03f), TimeSpan.FromMilliseconds(500)); //tuns off top
                game.Tap(new Vector2(0.07f, 0.15f), TimeSpan.FromMilliseconds(500)); //turns on
                game.Wait(TimeSpan.FromMilliseconds(1000));
                game.TakeScreenshot();
                game.Wait(TimeSpan.FromMilliseconds(500));

                //Turn off some lights
                game.Tap(new Vector2(0.07f, 0.15f), TimeSpan.FromMilliseconds(500)); //tuns off top
                game.Tap(new Vector2(0.07f, 0.27f), TimeSpan.FromMilliseconds(500)); //turns on
                game.Wait(TimeSpan.FromMilliseconds(1000));
                game.TakeScreenshot();
                game.Wait(TimeSpan.FromMilliseconds(500));

                //Turn off some lights
                game.Tap(new Vector2(0.07f, 0.27f), TimeSpan.FromMilliseconds(500)); //tuns off top
                game.Tap(new Vector2(0.07f, 0.32f), TimeSpan.FromMilliseconds(500)); //turns on
                game.Wait(TimeSpan.FromMilliseconds(1000));
                game.TakeScreenshot();
                game.Wait(TimeSpan.FromMilliseconds(500));
            }
        }
    }
}
