// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Images
{
    /// <summary>
    /// Post effect using an <see cref="Effect"/> (either xkfx or xksl).
    /// </summary>
    [DataContract("ImageEffectShader")]
    public class ImageEffectShader : ImageEffect
    {
        private MutablePipelineState pipelineState;
        private bool pipelineStateDirty = true;
        private BlendStateDescription blendState = BlendStateDescription.Default;
        private EffectBytecode previousBytecode;

        [DataMemberIgnore]
        public BlendStateDescription BlendState
        {
            get { return blendState; }
            set { blendState = value; pipelineStateDirty = true; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageEffectShader" /> class.
        /// </summary>
        public ImageEffectShader(string effectName = null)
        {
            EffectInstance = new DynamicEffectInstance(effectName, Parameters);
        }

        /// <inheritdoc/>
        protected override void InitializeCore()
        {
            base.InitializeCore();

            pipelineState = new MutablePipelineState(Context.GraphicsDevice);
            pipelineState.State.SetDefaults();
            pipelineState.State.InputElements = PrimitiveQuad.VertexDeclaration.CreateInputElements();
            pipelineState.State.PrimitiveType = PrimitiveQuad.PrimitiveType;

            if (EffectName == null) throw new ArgumentNullException("No EffectName specified");

            // Setup the effect compiler
            EffectInstance.Initialize(Context.Services);

            SetDefaultParameters();
        }

        /// <summary>
        /// The current effect instance.
        /// </summary>
        [DataMemberIgnore]
        public DynamicEffectInstance EffectInstance { get; private set; }

        /// <summary>
        /// Effect name.
        /// </summary>
        [DataMemberIgnore]
        public string EffectName
        {
            get { return EffectInstance.EffectName; }
            protected set { EffectInstance.EffectName = value; }
        }

        /// <summary>
        /// Sets the default parameters (called at constructor time and if <see cref="Reset"/> is called)
        /// </summary>
        protected override void SetDefaultParameters()
        {
            // TODO: Do not use slow version
            Parameters.Set(TexturingKeys.Sampler, GraphicsDevice.SamplerStates.LinearClamp);
        }

        protected override void PreDrawCore(RenderDrawContext context)
        {
            base.PreDrawCore(context);

            // Default handler for parameters
            UpdateParameters();
        }

        /// <summary>
        /// Updates the effect <see cref="ImageEffectShader.Parameters" /> from properties defined in this instance. See remarks.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Expecting less than 10 textures in input</exception>
        /// <remarks>By default, all the input textures will be remapped to <see cref="TexturingKeys.Texture0" />...etc.</remarks>
        protected virtual void UpdateParameters()
        {
            // By default, we are copying all input textures to TexturingKeys.Texture#
            var count = InputCount;
            for (int i = 0; i < count; i++)
            {
                var texture = GetInput(i);
                if (i < TexturingKeys.DefaultTextures.Count)
                {
                    var texturingKeys = texture.Dimension == TextureDimension.TextureCube ? TexturingKeys.TextureCubes : TexturingKeys.DefaultTextures;
                    // TODO GRAPHICS REFACTOR Do not use slow version
                    Parameters.Set(texturingKeys[i], texture);
                    Parameters.Set(TexturingKeys.TexturesTexelSize[i], new Vector2(1.0f / texture.ViewWidth, 1.0f / texture.ViewHeight));
                }
                else
                {
                    throw new InvalidOperationException("Expecting less than {0} textures in input".ToFormat(TexturingKeys.DefaultTextures.Count));
                }
            }
        }

        protected override void DrawCore(RenderDrawContext context)
        {
            if (EffectInstance.UpdateEffect(GraphicsDevice) || pipelineStateDirty || previousBytecode != EffectInstance.Effect.Bytecode)
            {
                // The EffectInstance might have been updated from outside
                previousBytecode = EffectInstance.Effect.Bytecode;

                pipelineState.State.RootSignature = EffectInstance.RootSignature;
                pipelineState.State.EffectBytecode = EffectInstance.Effect.Bytecode;
                pipelineState.State.BlendState = blendState;
                pipelineState.State.Output.CaptureState(context.CommandList);
                pipelineState.Update();
                pipelineStateDirty = false;
            }

            context.CommandList.SetPipelineState(pipelineState.CurrentState);

            EffectInstance.Apply(context.GraphicsContext);

            // Draw a full screen quad
            context.GraphicsDevice.PrimitiveQuad.Draw(context.CommandList);
        }
    }
}