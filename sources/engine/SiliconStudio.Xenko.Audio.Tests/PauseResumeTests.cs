// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Threading.Tasks;
using NUnit.Framework;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Input;

namespace SiliconStudio.Xenko.Audio.Tests
{
    public class PauseResumeTest : AudioTestGame
    {
        private SoundMusic music;
        private SoundEffect effect;
        
        public PauseResumeTest()
        {
            CurrentVersion = 1;
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            music = Content.Load<SoundMusic>("MusicFishLampMp3");
            effect = Content.Load<SoundEffect>("EffectBip");
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
                if (pointerEvent.State == PointerState.Up)
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