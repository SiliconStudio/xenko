using System;
using SiliconStudio.Xenko.Rendering.Images;

namespace SiliconStudio.Xenko.Rendering
{
    public class ImageEffectShader : ImageEffect
    {
        private readonly EffectInstance dynamicEffectInstance;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageEffectShader" /> class.
        /// </summary>
        public ImageEffectShader(string effectName = null)
        {
            dynamicEffectInstance = new DynamicEffectInstance(effectName);
        }

        protected override void DrawCore(RenderContext context)
        {
            throw new NotImplementedException();
            //dynamicEffectInstance.UpdateEffect(GraphicsDevice, EffectSystem);
            dynamicEffectInstance.Apply(GraphicsDevice);
        }
    }
}