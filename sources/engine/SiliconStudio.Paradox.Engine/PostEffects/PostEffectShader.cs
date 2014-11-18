// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Shaders.Compiler;

namespace SiliconStudio.Paradox.PostEffects
{
    /// <summary>
    /// Post effect using a single shader.
    /// </summary>
    public class PostEffectShader : PostEffectBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostEffectShader" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public PostEffectShader(PostEffectContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostEffectShader" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="effect">The effect.</param>
        /// <param name="parameters">The parameters.</param>
        /// <exception cref="System.ArgumentNullException">effect</exception>
        public PostEffectShader(PostEffectContext context, Effect effect, ParameterCollection parameters = null) : base(context)
        {
            if (effect == null) throw new ArgumentNullException("effect");
            Effect = effect;
            Name = Effect.Name;
            Parameters = parameters ?? new ParameterCollection();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostEffectShader"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="effectName">Name of the shader.</param>
        /// <param name="parameters">The compiler parameters.</param>
        public PostEffectShader(PostEffectContext context, string effectName, ParameterCollection parameters = null)
            : this(context, SafeLoadEffect(context, effectName, parameters), parameters)
        {
        }

        /// <summary>
        /// Gets the effect associated to this post effect.
        /// </summary>
        /// <value>The effect.</value>
        public Effect Effect { get; private set; }

        /// <summary>
        /// Gets the effect parameters.
        /// </summary>
        /// <value>The parameters.</value>
        public ParameterCollection Parameters { get; private set; }

        protected override void DrawCore()
        {
            GraphicsDevice.DrawQuad(Effect, Parameters);
        }

        private static Effect SafeLoadEffect(PostEffectContext context, string effectName, ParameterCollection compilerParameters)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (effectName == null) throw new ArgumentNullException("effectName");

            return context.LoadEffect(effectName, new CompilerParameters());
        }
    }
}