using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Rendering
{
    public class ModelRenderProcessor : EntityProcessor<ModelComponent, RenderModel>
    {
        private NextGenRenderSystem renderSystem;

        public Dictionary<ModelComponent, RenderModel> RenderModels => ComponentDatas;

        public ModelRenderProcessor() : base(typeof(TransformComponent))
        {
        }

        protected internal override void OnSystemAdd()
        {
            renderSystem = Services.GetSafeServiceAs<NextGenRenderSystem>();
        }

        protected override RenderModel GenerateComponentData(Entity entity, ModelComponent component)
        {
            var modelComponent = entity.Get<ModelComponent>();
            var renderModel = new RenderModel(modelComponent);

            return renderModel;
        }

        public override void Draw(RenderContext context)
        {
            base.Draw(context);

            // Note: we are rebuilding RenderMeshes every frame
            // TODO: check if it wouldn't be better to add/remove directly in CheckMeshes()?
            foreach (var entity in ComponentDatas)
            {
                var renderModel = entity.Value;

                CheckMeshes(renderModel);
                UpdateRenderModel(renderModel);
            }
        }

        private void UpdateRenderModel(RenderModel renderModel)
        {
            var modelComponent = renderModel.ModelComponent;
            var modelViewHierarchy = modelComponent.Skeleton;
            var nodeTransformations = modelViewHierarchy.NodeTransformations;

            // TODO GRAPHICS REFACTOR compute bounding box either by Mesh, or switch to future VisibilityObject system to deal with complete models)
            var boundingBox = new BoundingBoxExt(modelComponent.BoundingBox);

            foreach (var renderMesh in renderModel.Meshes)
            {
                var mesh = renderMesh.Mesh;

                renderMesh.Enabled = modelComponent.Enabled;

                if (renderMesh.Enabled)
                {
                    // Copy world matrix
                    var nodeIndex = mesh.NodeIndex;
                    renderMesh.World = nodeTransformations[nodeIndex].WorldMatrix;
                    renderMesh.BoundingBox = boundingBox;
                }
            }
        }

        private void CheckMeshes(RenderModel renderModel)
        {
            // Check if model changed
            var model = renderModel.ModelComponent.Model;
            if (renderModel.Model == model)
                return;

            // Remove old meshes
            if (renderModel.Meshes != null)
            {
                foreach (var renderMesh in renderModel.Meshes)
                {
                    // Unregister from render system
                    renderSystem.RenderObjects.Remove(renderMesh);
                }
            }

            // Create render meshes
            var renderMeshes = new RenderMesh[model.Meshes.Count];
            var modelComponent = renderModel.ModelComponent;
            for (int index = 0; index < model.Meshes.Count; index++)
            {
                var mesh = model.Meshes[index];

                // Update material
                // TODO: Somehow, if material changed we might need to remove/add object in render system again (to evaluate new render stage subscription)
                var materialIndex = mesh.MaterialIndex;
                renderMeshes[index] = new RenderMesh
                {
                    RenderModel = renderModel,
                    Mesh = mesh,
                    Material = modelComponent.Materials.GetItemOrNull(materialIndex)  // Check ModelComponent.Materials first
                                     ?? model.Materials.GetItemOrNull(materialIndex), // Otherwise, fallback to Model.Materials
                };
            }

            renderModel.Model = model;
            renderModel.Meshes = renderMeshes;

            // Update and register with render system
            foreach (var renderMesh in renderMeshes)
            {
                renderSystem.RenderObjects.Add(renderMesh);
            }
        }
    }
}