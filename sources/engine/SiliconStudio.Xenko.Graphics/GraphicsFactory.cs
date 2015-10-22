// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// Root factory for all Graphics components.
    /// </summary>
    public class GraphicsFactory : ComponentBase
    {
        /// <summary>
        /// GraphicsApi key.
        /// TODO not the best place to store this identifier. Move it to GraphicsFactory?
        /// </summary>
        public const string GraphicsApi = "GraphicsApi";
    }
}
