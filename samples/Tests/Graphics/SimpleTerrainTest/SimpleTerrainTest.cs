using System;
using NUnit.Framework;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games.Testing;

namespace SimpleTerrainTest
{
    [TestFixture]
    public class SimpleTerrainTest
    {
        private const string Path = "samples\\Graphics\\SimpleTerrain\\Bin\\Windows-Direct3D11\\Debug\\SimpleTerrain.exe";

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
                game.TakeScreenshot();
                game.Tap(new Vector2(0.067f, 0.344f), TimeSpan.FromMilliseconds(500));
                game.Wait(TimeSpan.FromMilliseconds(500));
                game.TakeScreenshot();
                game.Tap(new Vector2(0.067f, 0.344f), TimeSpan.FromMilliseconds(500));
                game.Wait(TimeSpan.FromMilliseconds(500));
                game.Tap(new Vector2(0.039f, 0.38f), TimeSpan.FromMilliseconds(500));
                game.Wait(TimeSpan.FromMilliseconds(500));
                game.TakeScreenshot();
                game.Wait(TimeSpan.FromMilliseconds(2000));
            }
        }
    }
}
