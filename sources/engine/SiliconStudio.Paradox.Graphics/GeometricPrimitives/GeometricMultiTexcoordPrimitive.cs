// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// A geometric primitive. Use <see cref="Sphere"/> to learn how to use it.
    /// </summary>
    public partial class GeometricMultiTexcoordPrimitive : GeometricPrimitive<VertexPositionNormalTangentMultiTexture>
    {
        public GeometricMultiTexcoordPrimitive(GraphicsDevice graphicsDevice, GeometricMeshData<VertexPositionNormalTangentMultiTexture> geometryMesh)
            : base(graphicsDevice, geometryMesh)
        {
        }
    }
}