using System;
using System.Collections.Generic;

using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.EntityModel;

namespace SiliconStudio.Paradox.Engine.Graphics
{
    public class RenderModel
    {
        public RenderModel(Entity entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            Entity = entity;
            ModelComponent = entity.Get<ModelComponent>();
            Parameters = ModelComponent.Parameters;
            TransformComponent = entity.Transform;
            RenderMeshesList = new List<RenderMeshCollection>(4);
            Update();
        }

        public readonly Entity Entity;

        public readonly ModelComponent ModelComponent;

        public ParameterCollection Parameters { get; private set; }

        public Model Model { get; private set; }

        public EntityGroup Group { get; private set; }

        public bool IsGroupUpdated { get; private set; }

        internal void Update()
        {
            IsGroupUpdated = Entity.Group != Group;
            Group = Entity.Group;
            var previousModel = Model;
            Model = ModelComponent.Model;
            if (previousModel != Model)
            {
                // When changing the model, we need to regenerate the render meshes
                foreach (var renderMeshes in RenderMeshesList)
                {
                    if (renderMeshes != null)
                    {
                        // TODO: Should we dispose something here?
                        renderMeshes.Clear();
                        renderMeshes.TransformUpdated = false;
                    }
                }
            }
            else
            {
                // When changing the model, we need to regenerate the render meshes
                foreach (var renderMeshes in RenderMeshesList)
                {
                    if (renderMeshes != null)
                    {
                        renderMeshes.TransformUpdated = false;
                    }
                }
            }
        }

        public readonly TransformComponent TransformComponent;

        internal readonly List<RenderMeshCollection> RenderMeshesList;

        public List<ModelProcessor.EntityLink> Links;

        public Material GetMaterial(int materialIndex)
        {
            // TBD, but for now, -1 means null material
            if (materialIndex == -1)
                return null;

            // Try to get material first from model instance, then model
            return GetMaterialHelper(ModelComponent.Materials, materialIndex)
                   ?? GetMaterialHelper(Model.Materials, materialIndex);
        }

        private static Material GetMaterialHelper(List<Material> materials, int index)
        {
            if (materials != null && index < materials.Count)
            {
                return materials[index];
            }

            return null;
        }
    }

    internal class RenderMeshCollection : List<RenderMesh>
    {
        public bool TransformUpdated { get; set; }
    }

}