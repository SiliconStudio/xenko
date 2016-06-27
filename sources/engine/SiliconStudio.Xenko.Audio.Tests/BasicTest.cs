// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;

using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Input;

namespace SiliconStudio.Xenko.Audio.Tests
{
    public class BasicTest : AudioTestGame
    {
        public BasicTest()
        {
            CurrentVersion = 1;
        }


        private int count;
        private Sound effectA;
        private Sound musicA;
        private Sound effect48kHz;
        private Sound effect11kHz;
        private Sound effect22kHz;
        private Sound effect11kHzStereo;
        private Sound effect22kHzStereo;

        protected override Task LoadContent()
        {
            effect48kHz = Content.Load<Sound>("Effect48000Hz");
            effect11kHz = Content.Load<Sound>("Effect11025Hz");
            effect22kHz = Content.Load<Sound>("Effect22050Hz");
            effect11kHzStereo = Content.Load<Sound>("Effect11025HzStereo");
            effect22kHzStereo = Content.Load<Sound>("Effect22050HzStereo");

            effectA = Content.Load<Sound>("EffectToneA");
            musicA = Content.Load<Sound>("MusicToneA");

            return Task.FromResult(true);
        }
        
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Input.PointerEvents.Count > 0)
            {
                if (Input.PointerEvents.Any(x => x.State == PointerState.Up))
                {
                    if (count % 5 == 0)
                        effect48kHz.CreateInstance(Audio.AudioEngine.DefaultListener).Play();
                    else if (count % 5 == 1)
                        effect11kHz.CreateInstance(Audio.AudioEngine.DefaultListener).Play();
                    else if (count % 5 == 2)
                        effect22kHz.CreateInstance(Audio.AudioEngine.DefaultListener).Play();
                    else if (count % 5 == 3)
                        effect11kHzStereo.CreateInstance(Audio.AudioEngine.DefaultListener).Play();
                    else if (count % 5 == 4)
                        effect22kHzStereo.CreateInstance(Audio.AudioEngine.DefaultListener).Play();

                    count++;
                }
            }
        }

        [Test]
        public void RunBasicGame()
        {
            RunGameTest(new BasicTest());
        }

        public static void Main()
        {
            using (var game = new BasicTest())
                game.Run();
        }
    }
}
