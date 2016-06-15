using System;
using NUnit.Framework;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Games.Testing;

namespace ConstraintsTest
{
    [TestFixture]
    public class ConstraintsTest
    {
        private const string Path = "samples\\Physics\\Constraints\\Bin\\Windows-Direct3D11\\Debug\\Constraints.exe";

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

                // X:0.8367187 Y:0.9375

                game.TakeScreenshot();

                game.Tap(new Vector2(0.83f, 0.93f), TimeSpan.FromMilliseconds(200));
                game.Wait(TimeSpan.FromMilliseconds(1000));
                game.TakeScreenshot();

                game.Tap(new Vector2(0.83f, 0.93f), TimeSpan.FromMilliseconds(200));
                game.Wait(TimeSpan.FromMilliseconds(1000));
                game.TakeScreenshot();

                game.Tap(new Vector2(0.83f, 0.93f), TimeSpan.FromMilliseconds(200));
                game.Wait(TimeSpan.FromMilliseconds(1000));
                game.TakeScreenshot();

                game.Tap(new Vector2(0.83f, 0.93f), TimeSpan.FromMilliseconds(200));
                game.Wait(TimeSpan.FromMilliseconds(1000));
                game.TakeScreenshot();

                game.Tap(new Vector2(0.83f, 0.93f), TimeSpan.FromMilliseconds(200));
                game.Wait(TimeSpan.FromMilliseconds(1000));
                game.TakeScreenshot();

                game.Wait(TimeSpan.FromMilliseconds(1000));
            }
        }
    }
}
