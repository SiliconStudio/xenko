// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// Contains resources and a constant buffer, that usually change at a given frequency.
    /// </summary>
    public class ResourceGroup
    {
        /// <summary>
        /// Resources.
        /// </summary>
        public DescriptorSet DescriptorSet;

        /// <summary>
        /// Constant buffer.
        /// </summary>
        public BufferPoolAllocationResult ConstantBuffer;
    }
}