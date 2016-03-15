// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using NUnit.Framework;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Audio.Tests.Engine
{
    /// <summary>
    /// Test that <see cref="SoundEffect"/> and <see cref="SoundMusic"/> can be loaded without problem with the asset loader.
    /// </summary>
    [TestFixture]
    public class TestAssetLoading
    {
        /// <summary>
        /// Test loading and playing resulting <see cref="SoundEffect"/> 
        /// </summary>
        [Test]
        public void TestSoundEffectLoading()
        {
            TestUtilities.ExecuteScriptInUpdateLoop(TestSoundEffectLoadingImpl, TestUtilities.ExitGameAfterSleep(1000));
        }

        private static void TestSoundEffectLoadingImpl(Game game)
        {
            SoundEffect sound = null;
            Assert.DoesNotThrow(() => sound = game.Content.Load<SoundEffect>("EffectBip"), "Failed to load the soundEffect.");
            Assert.IsNotNull(sound, "The soundEffect loaded is null.");
            sound.Play();
            // Should hear the sound here.
        }

        /// <summary>
        /// Test loading and playing resulting <see cref="SoundMusic"/>
        /// </summary>
        [Test]
        public void TestSoundMusicLoading()
        {
            TestUtilities.ExecuteScriptInUpdateLoop(TestSoundMusicLoadingImpl, TestUtilities.ExitGameAfterSleep(2000));
        }

        private static void TestSoundMusicLoadingImpl(Game game)
        {
            SoundMusic sound = null;
            Assert.DoesNotThrow(() => sound = game.Content.Load<SoundMusic>("EffectBip"), "Failed to load the SoundMusic.");
            Assert.IsNotNull(sound, "The SoundMusic loaded is null.");
            sound.Play();
            // Should hear the sound here.
        }
    }
}
