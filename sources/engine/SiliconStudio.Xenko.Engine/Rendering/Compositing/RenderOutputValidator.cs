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
        private readonly FastList<RenderTargetDescription> descriptions = new FastList<RenderTargetDescription>();

        private int validatedTargetCount;
        private bool hasChanged;

        public IReadOnlyList<RenderTargetDescription> RenderTargets => descriptions;

        public ShaderSourceCollection ShaderSources { get; private set; }

        //public RenderOutputDescription Output { get; private set; }

        public void Add<T>(PixelFormat format, bool isShaderResource = true)
            where T : IRenderTargetSemantic, new()
        {
            var description = new RenderTargetDescription
            {
                Semantic = new T(),
                Format = format
            };

            int index = validatedTargetCount++;
            if (index < descriptions.Count)
            {
                if (descriptions[index] != description)
                    hasChanged = true;

                descriptions[index] = description;
            }
            else
            {
                descriptions.Add(description);
                hasChanged = true;
            }
        }

        public void BeginValidation()
        {
            validatedTargetCount = 0;
            hasChanged = false;
        }

        public void EndValidation()
        {
            if (validatedTargetCount < descriptions.Count || hasChanged)
            {
                descriptions.Resize(validatedTargetCount, false);

                // Recalculate shader sources
                ShaderSources = new ShaderSourceCollection();
                foreach (var description in descriptions)
                {
                    ShaderSources.Add(new ShaderClassSource(description.Semantic.ShaderClass));
                }
            }
        }

        public int Find(Type semanticType)
        {
            for (int index = 0; index < descriptions.Count; index++)
            {
                if (descriptions[index].Semantic.GetType() == semanticType)
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
