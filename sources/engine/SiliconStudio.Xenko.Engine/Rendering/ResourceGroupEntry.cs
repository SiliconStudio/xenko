// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Threading;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering
{
    public struct ResourceGroupEntry
    {
        public int LastFrameUsed;
        public ResourceGroup Resources;

        /// <summary>
        /// Mark resource group as used during this frame.
        /// </summary>
        /// <returns>True if state changed (object was not mark as used during this frame until now), otherwise false.</returns>
        public bool MarkAsUsed(RenderSystem renderSystem)
        {
            return Interlocked.Exchange(ref LastFrameUsed, renderSystem.FrameCounter) != renderSystem.FrameCounter;
        }
    }
}
