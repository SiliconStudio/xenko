using System;
using System.Threading.Tasks;

namespace SiliconStudio.Xenko.Audio
{
    public sealed class DynamicSoundSource : SoundSource
    {
        public DynamicSoundSource(int channels) : base(channels)
        {
        }

        protected override Task Reader()
        {
            throw new NotImplementedException();
        }
    }
}