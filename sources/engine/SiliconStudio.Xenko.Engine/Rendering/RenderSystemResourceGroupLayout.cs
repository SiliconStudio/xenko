// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Implementation of <see cref="ResourceGroupLayout"/> specifically for <see cref="RenderSystem"/> use (contains some extra information).
    /// </summary>
    public class RenderSystemResourceGroupLayout : ResourceGroupLayout
    {
        internal int[] ConstantBufferOffsets;
        internal int[] ResourceIndices;

        public int GetConstantBufferOffset(ConstantBufferOffsetReference offsetReference)
        {
            return ConstantBufferOffsets[offsetReference.Index];
        }
    }
}