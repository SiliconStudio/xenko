// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Rendering.Materials;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Used by <see cref="MeshRenderFeature"/> to render a <see cref="Rendering.Mesh"/>.
    /// </summary>
    [DefaultPipelinePlugin(typeof(MeshPipelinePlugin))]
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
        public Material Material;
        // TODO GRAPHICS REFACTOR store that in RenderData (StaticObjectNode?)
        internal MaterialRenderFeature.MaterialInfo MaterialInfo;

        public bool IsShadowCaster;
        public bool IsShadowReceiver;

        public Matrix World = Matrix.Identity;
    }
}