// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering.Images
{
    public class ColorTransformContext
    {
        private readonly ColorTransformGroup group;

        private readonly RenderContext renderContext;

        private readonly ParameterCollection sharedParameters;

        private readonly ParameterCollection transformParameters;

        private readonly List<Texture> inputs;

        public ColorTransformContext(ColorTransformGroup @group, RenderContext renderContext)
        {
            this.group = group;
            this.renderContext = renderContext;
            inputs = new List<Texture>();
            sharedParameters = group.Parameters;
            transformParameters = new ParameterCollection();
        }

        public ColorTransformGroup Group
        {
            get
            {
                return group;
            }
        }

        public List<Texture> Inputs
        {
            get
            {
                return inputs;
            }
        }

        public ParameterCollection SharedParameters
        {
            get
            {
                return sharedParameters;
            }
        }

        public RenderContext RenderContext
        {
            get
            {
                return renderContext;
            }
        }
    }
}