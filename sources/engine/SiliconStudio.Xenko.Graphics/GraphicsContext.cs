// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// A graphics command context. You should usually stick to one per rendering thread.
    /// </summary>
    public class GraphicsContext
    {
        /// <summary>
        /// Gets the current command list.
        /// </summary>
        public CommandList CommandList { get; set; }

        /// <summary>
        /// Gets the current resource group allocator.
        /// </summary>
        public ResourceGroupAllocator ResourceGroupAllocator { get; private set; }

        public GraphicsResourceAllocator Allocator { get; private set; }

        public GraphicsContext(GraphicsDevice graphicsDevice, GraphicsResourceAllocator allocator = null, CommandList commandList = null)
        {
            CommandList = commandList ?? graphicsDevice.InternalMainCommandList ?? CommandList.New(graphicsDevice);
            Allocator = allocator ?? new GraphicsResourceAllocator(graphicsDevice).DisposeBy(graphicsDevice);
            ResourceGroupAllocator = new ResourceGroupAllocator(Allocator, CommandList).DisposeBy(graphicsDevice);
        }
    }
}
