// Copyright (c) 2011 Silicon Studio

using System.Collections.Generic;
using SiliconStudio.Xenko.DataModel;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Rendering
{
    public class PostEffectPlugin : RenderPassPlugin
    {
        public PostEffectPlugin() : this(null)
        {
        }

        public PostEffectPlugin(string name)
            : base(name)
        {
            PreferredFormat = PixelFormat.R16G16B16A16_Float;
        }

        public PixelFormat PreferredFormat { get; set; }

        public Texture2D RenderSource { get; set; }

        public RenderTarget RenderTarget { get; set; }
    }
}