using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiliconStudio.Paradox.Effects.Images
{
    [DataContract("Vignetting")]
    public sealed class Vignetting : ImageEffect
    {

        private ImageEffect vignettingEffect;

        /// <summary>
        /// Initializes a new instance of the <see cref="Vignetting"/> class.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="context">The context.</param>
        public Vignetting()
        {
            Amount = 0.8f;
            Radius = 0.7f;
            Color = new Color3(0f);
        }

        protected override void InitializeCore()
        {
            base.InitializeCore();
            vignettingEffect = ToLoadAndUnload(new ImageEffectShader("VignettingShader"));
        }

        /// <summary>
        /// Amount of vignetting (alpha of the halo).
        /// </summary>
        [DataMember(10)]
        [DefaultValue(0.8f)]
        [DataMemberRange(0f, 1f, 0.01f, 0.1f)]
        public float Amount { get; set; }

        /// <summary>
        /// Radius from the center, from which vignetting begins. 
        /// </summary>
        [DataMember(20)]
        [DefaultValue(0.7f)]
        [DataMemberRange(0f, 1f, 0.01f, 0.1f)]
        public float Radius { get; set; }

        /// <summary>
        /// Color of the vignetting halo.
        /// </summary>
        [DataMember(30)]
        public Color3 Color { get; set; }

        protected override void DrawCore(RenderContext context)
        {
            var input = GetSafeInput(0);
            var output = GetSafeOutput(0);

            // If input == output, than copy the input to a temporary texture
            if (input == output)
            {
                var newInput = NewScopedRenderTarget2D(input.Width, input.Height, input.Format);
                GraphicsDevice.Copy(input, newInput);
                input = newInput;
            }

            vignettingEffect.Parameters.Set(VignettingShaderKeys.Amount, Amount);
            vignettingEffect.Parameters.Set(VignettingShaderKeys.RadiusBegin, Radius);
            vignettingEffect.Parameters.Set(VignettingShaderKeys.Color, Color);

            vignettingEffect.SetInput(0, input);
            vignettingEffect.SetOutput(output);
            vignettingEffect.Draw(context, "Vignetting");
        }

    }
}
