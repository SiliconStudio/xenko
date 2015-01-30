// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// Interface to create a geometric primitive model.
    /// </summary>
    public interface IGeometricPrimitiveModel
    {
        GeometricMeshData<VertexPositionNormalTexture> Create();
    }
}