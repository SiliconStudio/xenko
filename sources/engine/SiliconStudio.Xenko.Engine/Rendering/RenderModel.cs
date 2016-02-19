using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Rendering
{
    public struct ActiveRenderStage
    {
        public bool Active { get { return EffectName != null; } }

        public string EffectName;

        public ActiveRenderStage(string effectName)
        {
            EffectName = effectName;
        }
    }

    /// <summary>
    /// Describes something that can be rendered by a <see cref="RootRenderFeature"/>.
    /// </summary>
    public abstract class RenderObject
    {
        // Kept in cache to quickly know if RenderPerFrameNode was already generated
        public RootRenderFeature RenderFeature;
        public ObjectNodeReference ObjectNode;

        public StaticObjectNodeReference StaticObjectNode;

        public ActiveRenderStage[] ActiveRenderStages;

        // TODO: Switch to a "StaticPropertyContainer" that will be optimized by assembly processor
        //public PropertyContainer Tags;
    }

    /// <summary>
    /// Used by <see cref="MeshRenderFeature"/> to render a <see cref="Mesh"/>.
    /// </summary>
    public class RenderMesh : RenderObject
    {
        public RenderModel RenderModel;

        /// <summary>
        /// Underlying mesh, can be accessed only during <see cref="RenderFeature.Extract"/> phase.
        /// </summary>
        public Mesh Mesh;

        // Material
        // TODO: Extract with MaterialRenderFeature
        public MaterialInstance Material;
        
        // TODO: Skinning
        
        public Matrix World = Matrix.Identity;
    }

    public class RenderModel
    {
        public readonly ModelComponent ModelComponent;
        public Model Model;
        public RenderMesh[] Meshes;

        public RenderModel(ModelComponent modelComponent)
        {
            ModelComponent = modelComponent;
        }
    }
}