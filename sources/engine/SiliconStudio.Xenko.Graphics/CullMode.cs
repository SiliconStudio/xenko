// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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