using System;
using NUnit.Framework;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Games.Testing;

namespace UIParticlesTest
{
    [TestFixture]
    public class UIParticlesTest
    {
        private const string Path = "samples\\UI\\UIParticles\\Bin\\Windows\\Debug\\UIParticles.exe";

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

		// TODO Simulate taps

                game.Tap(new Vector2(179f / 600f, 235f / 600f), TimeSpan.FromMilliseconds(150));
                game.Wait(TimeSpan.FromMilliseconds(250));
                game.TakeScreenshot();

                game.Tap(new Vector2(360f / 600f, 328f / 600f), TimeSpan.FromMilliseconds(150));
                game.Wait(TimeSpan.FromMilliseconds(1250));
                game.TakeScreenshot();

                game.Tap(new Vector2(179f / 600f, 235f / 600f), TimeSpan.FromMilliseconds(150));
                game.Wait(TimeSpan.FromMilliseconds(250));
                game.TakeScreenshot();

                game.Wait(TimeSpan.FromMilliseconds(2000));
            }
        }
    }
}
