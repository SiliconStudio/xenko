// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects;

namespace SiliconStudio.Paradox.Graphics
{
    public class SimpleEffect : Effect
    {
        private readonly ParameterCollection parameters;

        public SimpleEffect(GraphicsDevice graphicsDevice)
            : base(graphicsDevice, SpriteEffect.Bytecode)
        {
            parameters = new ParameterCollection();
            Color = new Color4(1.0f);
            Sampler = graphicsDevice.SamplerStates.LinearClamp;
            Transform = Matrix.Identity;
        }

        /// <summary>
        /// Gets or sets the color. Default is <see cref="SharpDX.Color.White"/>.
        /// </summary>
        /// <value>The color.</value>
        public Color4 Color
        {
            get
            {
                return parameters.Get(SpriteEffectKeys.Color);
            }

            set
            {
                parameters.Set(SpriteEffectKeys.Color, value);
            }
        }

        public Matrix Transform
        {
            get
            {
                return parameters.Get(SpriteBaseKeys.MatrixTransform);
            }

            set
            {
                parameters.Set(SpriteBaseKeys.MatrixTransform, value);
            }
        }

        public Texture Texture
        {
            get
            {
                return parameters.Get(TexturingKeys.Texture0);
            }

            set
            {
                parameters.Set(TexturingKeys.Texture0, value);
            }
        }

        public SamplerState Sampler
        {
            get
            {
                return parameters.Get(TexturingKeys.Sampler);
            }

            set
            {
                parameters.Set(TexturingKeys.Sampler, value);
            }
        }

        public void Apply()
        {
            Apply(parameters);
        }
    }
}