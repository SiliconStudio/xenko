// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Xenko.Graphics
{ 
    /// <summary>
    /// GraphicsResource abstract class
    /// </summary>
    public abstract partial class GraphicsResource : GraphicsResourceBase
    {
        protected GraphicsResource()
        {
        }

        protected GraphicsResource(GraphicsDevice device) : base(device)
        {
        }

        protected GraphicsResource(GraphicsDevice device, string name) : base(device, name)
        {
        }
    }
}
