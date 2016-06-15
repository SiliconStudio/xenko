using System;
using NUnit.Framework;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Games.Testing;

namespace SpriteFontsTest
{
    [TestFixture]
    public class SpriteFontsTest
    {
        private const string Path = "samples\\Graphics\\SpriteFonts\\Bin\\Windows-Direct3D11\\Debug\\SpriteFonts.exe";

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
                game.Wait(TimeSpan.FromMilliseconds(1000));
                game.TakeScreenshot();
                game.Wait(TimeSpan.FromMilliseconds(4000));
                game.TakeScreenshot();
                game.Wait(TimeSpan.FromMilliseconds(4000));
                game.TakeScreenshot();
                game.Wait(TimeSpan.FromMilliseconds(5000));
                game.TakeScreenshot();
                game.Wait(TimeSpan.FromMilliseconds(4000));
                game.TakeScreenshot();
                game.Wait(TimeSpan.FromMilliseconds(4000));
                game.TakeScreenshot();
            }
        }
    }
}
