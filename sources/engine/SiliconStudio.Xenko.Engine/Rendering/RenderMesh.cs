// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Rendering.Materials;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Used by <see cref="MeshRenderFeature"/> to render a <see cref="Rendering.Mesh"/>.
    /// </summary>
    public class RenderMesh : RenderObject
    {
        public MeshDraw ActiveMeshDraw;

        public RenderModel RenderModel;

        /// <summary>
        /// Underlying mesh, can be accessed only during <see cref="RenderFeature.Extract"/> phase.
        /// </summary>
        public Mesh Mesh;

        // Material
        // TODO: Extract with MaterialRenderFeature
        public MaterialPass MaterialPass;

        // TODO GRAPHICS REFACTOR store that in RenderData (StaticObjectNode?)
        internal MaterialRenderFeature.MaterialInfo MaterialInfo;

        public bool IsShadowCaster;

        public bool IsScalingNegative;

        public bool IsPreviousScalingNegative;

        public Matrix World = Matrix.Identity;

        public Matrix[] BlendMatrices;
    }
}
