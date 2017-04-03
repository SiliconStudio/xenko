// Copyright(c) 2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Collections;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Compositing
{
    /// <summary>
    /// Represents how we setup the graphics pipeline output targets.
    /// </summary>
    public sealed class RenderOutputValidator
    {
        private readonly FastList<RenderTargetDescription> renderTargets = new FastList<RenderTargetDescription>();
        private readonly RenderStage renderStage;

        private int validatedTargetCount;
        private bool hasChanged;
        private MSAALevel multiSampleLevel;
        private PixelFormat depthStencilFormat;

        public IReadOnlyList<RenderTargetDescription> RenderTargets => renderTargets;

        public ShaderMixinSource ShaderSource { get; private set; }

        //public RenderOutputDescription Output { get; private set; }

        internal RenderOutputValidator(RenderStage renderStage)
        {
            this.renderStage = renderStage;
        }

        public void Add<T>(PixelFormat format, bool isShaderResource = true)
            where T : IRenderTargetSemantic, new()
        {
            var description = new RenderTargetDescription
            {
                Semantic = new T(),
                Format = format
            };

            int index = validatedTargetCount++;
            if (index < renderTargets.Count)
            {
                if (renderTargets[index] != description)
                    hasChanged = true;

                renderTargets[index] = description;
            }
            else
            {
                renderTargets.Add(description);
                hasChanged = true;
            }
        }

        public void BeginCustomValidation(PixelFormat depthStencilFormat, MSAALevel multiSampleLevel = MSAALevel.None)
        {
            validatedTargetCount = 0;
            hasChanged = false;

            if (this.depthStencilFormat != depthStencilFormat)
            {
                hasChanged = true;
                this.depthStencilFormat = depthStencilFormat;
            }
            if (this.multiSampleLevel != multiSampleLevel)
            {
                hasChanged = true;
                this.multiSampleLevel = multiSampleLevel;
            }
        }

        public unsafe void EndCustomValidation()
        {
            if (validatedTargetCount < renderTargets.Count || hasChanged)
            {
                renderTargets.Resize(validatedTargetCount, false);

                // Recalculate shader sources
                ShaderSource = new ShaderMixinSource();
                ShaderSource.Macros.Add(new ShaderMacro("SILICON_STUDIO_RENDER_TARGET_COUNT", renderTargets.Count));
                for (var index = 0; index < renderTargets.Count; index++)
                {
                    var renderTarget = renderTargets[index];
                    if (index > 0)
                        ShaderSource.Compositions.Add($"ShadingColor{index}", renderTarget.Semantic.ShaderClass);
                }

                ShaderSource.Macros.Add(new ShaderMacro("SILICON_STUDIO_MULTISAMPLE_COUNT", (int)multiSampleLevel));
            }

            renderStage.Output.RenderTargetCount = renderTargets.Count;
            renderStage.Output.MultiSampleLevel = multiSampleLevel;
            renderStage.Output.DepthStencilFormat = depthStencilFormat;

            fixed (PixelFormat* formats = &renderStage.Output.RenderTargetFormat0)
            {
                for (int i = 0; i < renderTargets.Count; ++i)
                {
                    formats[i] = renderTargets[i].Format;
                }
            }
        }

        public void Validate(ref RenderOutputDescription renderOutput)
        {
            hasChanged = false;
            if (multiSampleLevel != renderOutput.MultiSampleLevel)
            {
                hasChanged = true;
                multiSampleLevel = renderOutput.MultiSampleLevel;
            }

            if (hasChanged)
            {
                // Recalculate shader sources
                ShaderSource = new ShaderMixinSource();
                ShaderSource.Macros.Add(new ShaderMacro("SILICON_STUDIO_MULTISAMPLE_COUNT", (int)multiSampleLevel));
            }

            renderStage.Output = renderOutput;
        }

        public int Find(Type semanticType)
        {
            for (int index = 0; index < renderTargets.Count; index++)
            {
                if (renderTargets[index].Semantic.GetType() == semanticType)
                    return index;
            }

            return -1;
        }

        public int Find<T>()
            where T : IRenderTargetSemantic
        {
            return Find(typeof(T));
        }
    }
}
