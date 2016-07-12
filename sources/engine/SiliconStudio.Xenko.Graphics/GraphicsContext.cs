// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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


        public GraphicsContext(IServiceRegistry services, CommandList commandList)
        {
            CommandList = commandList;
            Allocator = services.GetServiceAs<GraphicsResourceAllocator>() ?? new GraphicsResourceAllocator(services).DisposeBy(CommandList.GraphicsDevice);
            ResourceGroupAllocator = new ResourceGroupAllocator(Allocator, commandList);
        }
    }
}