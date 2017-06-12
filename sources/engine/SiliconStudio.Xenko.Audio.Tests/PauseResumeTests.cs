// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Threading.Tasks;
using NUnit.Framework;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Input;

namespace SiliconStudio.Xenko.Audio.Tests
{
    public class PauseResumeTest : AudioTestGame
    {
        private SoundInstance music;
        private SoundInstance effect;
        
        public PauseResumeTest()
        {
            CurrentVersion = 1;
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            music = Content.Load<Sound>("MusicFishLampMp3").CreateInstance(Audio.AudioEngine.DefaultListener);
            effect = Content.Load<Sound>("EffectBip").CreateInstance(Audio.AudioEngine.DefaultListener);
            music.IsLooped = true;
            effect.IsLooped = true;
            music.Play();
            effect.Play();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            foreach (var pointerEvent in Input.PointerEvents)
            {
                if (pointerEvent.EventType == PointerEventType.Released)
                {
                    music.Stop();
                    music.Play();
                    //effect.Stop();
                    //effect.Play();
                }
            }
        }

        [Test]
        public void RunPauseGame()
        {
            RunGameTest(new PauseResumeTest());
        }

        public static void Main()
        {
            using (var game = new PauseResumeTest())
                game.Run();
        }
    }
}
