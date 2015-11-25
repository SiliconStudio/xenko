using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Internals;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Particles.Materials
{
    [DataContract("ParticleMaterialTexture")]
    [Display("StaticTexture")]
    public class ParticleMaterialTexture : ParticleMaterialBase
    {
        [DataMember(100)]
        [Display("Texture")]
        public Texture Texture;

        private static Effect effect = null;
        private static Effect effectSRgb = null;

        protected static Effect GetEffect(GraphicsDevice device)
        {
            return effect ?? (effect = new Effect(device, ParticleBatch.Bytecode));
        }

        protected static Effect GetEffectSRgb(GraphicsDevice device)
        {
            return effectSRgb ?? (effectSRgb = new Effect(device, ParticleBatch.BytecodeSRgb));
        }

        protected EffectParameterCollectionGroup ParameterCollectionGroup { get; private set; }

        public override void Setup(GraphicsDevice graphicsDevice, Matrix viewMatrix, Matrix projMatrix)
        {
            var effect = (graphicsDevice.ColorSpace == ColorSpace.Linear ? GetEffectSRgb(graphicsDevice) : GetEffect(graphicsDevice));

            // This is a textured material, so if we don't have a texture parameter we can't draw it
            if (!effect.HasParameter(TexturingKeys.Texture0))
                return;

            // Get or create parameter collection
            if (ParameterCollectionGroup == null || ParameterCollectionGroup.Effect != effect)
            {
                // If ParameterCollectionGroup is not specified (using default one), let's make sure it is updated to matches effect
                // It is quite inefficient if user is often switching effect without providing a matching ParameterCollectionGroup
                ParameterCollectionGroup = new EffectParameterCollectionGroup(graphicsDevice, effect, new[] { Parameters });
            }

            SetupBase(graphicsDevice);

            // This should be CB0 - view/proj matrices don't change per material
            Parameters.Set(ParticleBaseKeys.MatrixTransform, viewMatrix * projMatrix);


            effect.Apply(graphicsDevice, ParameterCollectionGroup, applyEffectStates: false);

            // TODO Is it ok if EffectParameterResourceBinding is not internal anymore?
            var textureUpdater = effect.GetParameterFastUpdater(TexturingKeys.Texture0);
            textureUpdater.ApplyParameter(graphicsDevice, Texture);


        }

    }
}
