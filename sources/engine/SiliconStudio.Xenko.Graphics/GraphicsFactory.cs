// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Graphics
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
