using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Particles
{
    [Flags]
    public enum ParticleEffectVariation
    {
        None        = 0x00,
        IsSrgb      = 0x01,
        HasTex0     = 0x02,
        HasColor    = 0x04,
    }

    public partial class ParticleBatch
    {
        private const int MaxEffectVariations = 
            (int)(ParticleEffectVariation.IsSrgb | 
                  ParticleEffectVariation.HasTex0 | 
                  ParticleEffectVariation.HasColor) + 1;

        private static Effect[] effect = new Effect[MaxEffectVariations];
        private static EffectBytecode[] effectBytecode = new EffectBytecode[MaxEffectVariations];

        private static EffectBytecode Bytecode(ParticleEffectVariation variation)
        {
            switch (variation)
            {
                case ParticleEffectVariation.IsSrgb:
                    return effectBytecode[(int)ParticleEffectVariation.IsSrgb] ?? 
                          (effectBytecode[(int)ParticleEffectVariation.IsSrgb] = EffectBytecode.FromBytes(binaryBytecodeSRgb));

                case ParticleEffectVariation.HasTex0:
                    return effectBytecode[(int)ParticleEffectVariation.HasTex0] ?? 
                          (effectBytecode[(int)ParticleEffectVariation.HasTex0] = EffectBytecode.FromBytes(binaryBytecodeTex0));

                case ParticleEffectVariation.IsSrgb | ParticleEffectVariation.HasTex0:
                    return effectBytecode[(int)(ParticleEffectVariation.IsSrgb | ParticleEffectVariation.HasTex0)] ?? 
                          (effectBytecode[(int)(ParticleEffectVariation.IsSrgb | ParticleEffectVariation.HasTex0)] = EffectBytecode.FromBytes(binaryBytecodeSRgbTex0));

                default:
                    return effectBytecode[(int)ParticleEffectVariation.None] ?? 
                          (effectBytecode[(int)ParticleEffectVariation.None] = EffectBytecode.FromBytes(binaryBytecode));
            }
        }

        public static Effect GetEffect(GraphicsDevice device, ParticleEffectVariation variation)
        {
            return effect[(int)variation] ?? (effect[(int)variation] = new Effect(device, Bytecode(variation)));
        }

    }
}
