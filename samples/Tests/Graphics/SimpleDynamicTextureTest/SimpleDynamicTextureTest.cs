using System;
using NUnit.Framework;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games.Testing;

namespace SimpleDynamicTextureTest
{
    [TestFixture]
    public class SimpleDynamicTextureTest
    {
        private const string Path = "samples\\Graphics\\SimpleDynamicTexture\\Bin\\Windows-Direct3D11\\Debug\\SimpleDynamicTexture.exe";

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

                game.Tap(new Vector2(0.5f, 0.7f), TimeSpan.FromMilliseconds(500));
                game.Tap(new Vector2(0.6f, 0.2f), TimeSpan.FromMilliseconds(500));
                game.Tap(new Vector2(0.7f, 0.3f), TimeSpan.FromMilliseconds(500));
                game.Tap(new Vector2(0.8f, 0.4f), TimeSpan.FromMilliseconds(500));
                game.Tap(new Vector2(0.9f, 0.5f), TimeSpan.FromMilliseconds(500));
                game.Tap(new Vector2(0.5f, 0.6f), TimeSpan.FromMilliseconds(500));

                game.TakeScreenshot();

                game.Wait(TimeSpan.FromMilliseconds(2000));
            }
        }
    }
}
