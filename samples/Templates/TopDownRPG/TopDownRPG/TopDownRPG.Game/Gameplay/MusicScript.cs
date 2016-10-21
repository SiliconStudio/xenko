using System.Threading.Tasks;
using SiliconStudio.Xenko.Audio;
using SiliconStudio.Xenko.Engine;

namespace TopDownRPG.Gameplay
{
    /// <summary>
    /// The main script in charge of the sound.
    /// </summary>
    public class MusicScript : AsyncScript
    {
        public Sound SoundMusic { get; set; }

        private SoundInstance music;

        public override async Task Execute()
        {
            music = SoundMusic.CreateInstance();

            if (!IsLiveReloading)
            {
                // start ambient music
                music.IsLooping = true;
                music.Play();
            }

            while (Game.IsRunning)
            {
                // wait for next frame
                await Script.NextFrame();
            }
        }
    }
}
