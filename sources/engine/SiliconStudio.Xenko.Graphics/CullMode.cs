// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>	
    /// Indicates triangles facing a particular direction are not drawn.	
    /// </summary>	
    /// <remarks>	
    /// This enumeration is part of a rasterizer-state object description (see <see cref="RasterizerState"/>). 	
    /// </remarks>
    [DataContract]
    public enum CullMode 
    {
        /// <summary>	
        /// Always draw all triangles. 	
        /// </summary>	
        None = 1,

        /// <summary>	
        /// Do not draw triangles that are front-facing. 	
        /// </summary>	
        Front = 2,

        /// <summary>	
        /// Do not draw triangles that are back-facing. 	
        /// </summary>	
        Back = 3,
    }
}
