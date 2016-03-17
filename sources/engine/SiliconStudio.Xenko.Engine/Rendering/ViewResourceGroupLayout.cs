// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Implementation of <see cref="ResourceGroupLayout"/> specifically for PerView cbuffer of <see cref="RenderSystem"/>.
    /// </summary>
    public class ViewResourceGroupLayout : RenderSystemResourceGroupLayout
    {
        public ResourceGroupEntry[] Entries;
    }
}