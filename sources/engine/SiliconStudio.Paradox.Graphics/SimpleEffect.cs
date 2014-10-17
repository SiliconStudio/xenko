// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects.Modules;

namespace SiliconStudio.Paradox.Graphics
{
    public class SimpleEffect : Effect
    {
        public SimpleEffect(GraphicsDevice graphicsDevice)
            : base(graphicsDevice, SpriteEffect.Bytecode)
        {
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
                return Parameters.Get(SpriteEffectKeys.Color);
            }

            set
            {
                Parameters.Set(SpriteEffectKeys.Color, value);
            }
        }

        public Matrix Transform
        {
            get
            {
                return Parameters.Get(SpriteBaseKeys.MatrixTransform);
            }

            set
            {
                Parameters.Set(SpriteBaseKeys.MatrixTransform, value);
            }
        }

        public Texture Texture
        {
            get
            {
                return Parameters.Get(TexturingKeys.Texture0);
            }

            set
            {
                Parameters.Set(TexturingKeys.Texture0, value);
            }
        }

        public SamplerState Sampler
        {
            get
            {
                return Parameters.Get(TexturingKeys.Sampler);
            }

            set
            {
                Parameters.Set(TexturingKeys.Sampler, value);
            }
        }
    }
}