// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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
