// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Rendering.Images
{
    public class ColorTransformContext
    {
        private readonly ColorTransformGroup group;

        private readonly ParameterCollection sharedParameters;

        private readonly ParameterCollection transformParameters;

        private readonly List<Texture> inputs;

        public ColorTransformContext(ColorTransformGroup @group)
        {
            this.group = group;
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
    }
}