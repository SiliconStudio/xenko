// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// Defines an entry point for mesh instantiation and recursive rendering.
    /// </summary>
    public class RenderPipeline : RenderPass
    {

        public RenderPipeline(string name)
            : base(name)
        {
        }

        public RenderPipeline() : this(null)
        {
        }
    }
}