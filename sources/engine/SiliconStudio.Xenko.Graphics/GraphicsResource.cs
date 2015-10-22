// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Paradox.Graphics
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
