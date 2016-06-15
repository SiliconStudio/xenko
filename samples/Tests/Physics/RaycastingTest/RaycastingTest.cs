using System;
using NUnit.Framework;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games.Testing;

namespace RaycastingTest
{
    [TestFixture]
    public class RaycastingTest
    {
        private const string Path = "samples\\Physics\\Raycasting\\Bin\\Windows-Direct3D11\\Debug\\Raycasting.exe";

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

                /*
                [Raycasting.RaycastingScript]: Info: X:0.3304687 Y:0.6055555
                [Raycasting.RaycastingScript]: Info: X:0.496875 Y:0.4125
                [Raycasting.RaycastingScript]: Info: X:0.659375 Y:0.5319445
                */

                game.Tap(new Vector2(0.3304687f, 0.6055555f), TimeSpan.FromMilliseconds(200));
                game.TakeScreenshot();

                game.Tap(new Vector2(0.496875f, 0.4125f), TimeSpan.FromMilliseconds(200));
                game.TakeScreenshot();

                game.Tap(new Vector2(0.659375f, 0.5319445f), TimeSpan.FromMilliseconds(200));
                game.TakeScreenshot();

                game.Wait(TimeSpan.FromMilliseconds(1000));
            }
        }
    }
}
