// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Paradox.Graphics
{
    public static class VertexElementUsage
    {
        /// <summary>
        /// Vertex data contains diffuse or specular color.
        /// </summary>
        public static readonly string Color = "COLOR";

        /// <summary>
        /// Vertex normal data.
        /// </summary>
        public static readonly string Normal = "NORMAL";

        /// <summary>
        /// Position data.
        /// </summary>
        public static readonly string Position = "POSITION";

        /// <summary>
        /// Position transformed data.
        /// </summary>
        public static readonly string PositionTransformed = "SV_POSITION";

        /// <summary>
        /// Vertex tangent data.
        /// </summary>
        public static readonly string Tangent = "TANGENT";

        /// <summary>
        /// Vertex Bitangent data.
        /// </summary>
        public static readonly string BiTangent = "BITANGENT";

        /// <summary>
        /// Texture coordinate data.
        /// </summary>
        public static readonly string TextureCoordinate = "TEXCOORD";         
    }
}
