// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SiliconStudio.Paradox.Effects.Modules;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Images
{
    /// <summary>
    /// Post effect using an <see cref="Effect"/> (either pdxfx or pdxsl).
    /// </summary>
    public class ImageEffectShader : ImageEffectBase
    {
        private readonly ParameterCollection parameters;

        private readonly InternalEffectInstance effectInstance;

        private readonly DynamicEffectCompiler effectCompiler;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageEffectShader"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="effectName">Name of the shader.</param>
        public ImageEffectShader(ImageEffectContext context, string effectName)
            : base(context, effectName)
        {
            if (effectName == null) throw new ArgumentNullException("effectName");

            // Setup this instance parameters
            parameters = new ParameterCollection();
            // As this is used by PostEffectBase, we just setup it here by default
            parameters.Set(TexturingKeys.Sampler, GraphicsDevice.SamplerStates.LinearClamp);

            // Setup the effect compiler
            effectInstance = new InternalEffectInstance(parameters);
            effectCompiler = new DynamicEffectCompiler(context.Services, effectName);
        }

        /// <summary>
        /// Gets the name of the effect.
        /// </summary>
        public string EffectName
        {
            get
            {
                return effectCompiler.EffectName;
            }
        }

        /// <summary>
        /// Gets the effect parameters.
        /// </summary>
        /// <value>The parameters.</value>
        public ParameterCollection Parameters
        {
            get
            {
                return parameters;
            }
        }

        protected override void PreDrawCore(string name)
        {
            base.PreDrawCore(name);

            // Default handler for parameters
            UpdateParameters();
        }

        /// <summary>
        /// Updates the effect <see cref="Parameters"/> from properties defined in this instance. See remarks.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Expecting less than 10 textures in input</exception>
        /// <remarks>
        /// By default, all the input textures will be remapped to <see cref="TexturingKeys.Texture0"/>...etc.
        /// </remarks>
        protected virtual void UpdateParameters()
        {
            // By default, we are copying all input textures to TexturingKeys.Texture#
            var count = InputCount;
            for (int i = 0; i < count; i++)
            {
                var texture = GetInput(i);
                switch (i)
                {
                    case 0:
                        Parameters.Set(TexturingKeys.Texture0, texture);
                        break;
                    case 1:
                        Parameters.Set(TexturingKeys.Texture1, texture);
                        break;
                    case 2:
                        Parameters.Set(TexturingKeys.Texture2, texture);
                        break;
                    case 3:
                        Parameters.Set(TexturingKeys.Texture3, texture);
                        break;
                    case 4:
                        Parameters.Set(TexturingKeys.Texture4, texture);
                        break;
                    case 5:
                        Parameters.Set(TexturingKeys.Texture5, texture);
                        break;
                    case 6:
                        Parameters.Set(TexturingKeys.Texture6, texture);
                        break;
                    case 7:
                        Parameters.Set(TexturingKeys.Texture7, texture);
                        break;
                    case 8:
                        Parameters.Set(TexturingKeys.Texture8, texture);
                        break;
                    case 9:
                        Parameters.Set(TexturingKeys.Texture9, texture);
                        break;
                    default:
                        // TODO: This is not clean
                        throw new InvalidOperationException("Expecting less than 10 textures in input");
                }
            }
        }

        protected override void DrawCore()
        {
            // Dynamically update/compile the effect based on the current parameters.
            effectCompiler.Update(effectInstance);

            // Draw a full screen quad
            GraphicsDevice.DrawQuad(effectInstance.Effect, Parameters);
        }

        /// <summary>
        /// Internal class used for dynamic effect compilation.
        /// </summary>
        private class InternalEffectInstance : DynamicEffectInstance
        {
            private readonly ParameterCollection parameters;

            public InternalEffectInstance(ParameterCollection parameters)
            {
                this.parameters = parameters;
            }

            public override void FillParameterCollections(IList<ParameterCollection> parameterCollections)
            {
                parameterCollections.Add(parameters);
            }
        }
    }
}