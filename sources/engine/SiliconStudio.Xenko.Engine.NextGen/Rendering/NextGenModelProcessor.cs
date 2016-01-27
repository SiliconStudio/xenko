using System.Collections.Generic;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Rendering
{
    public class NextGenModelProcessor : EntityProcessor<ModelComponent, RenderModel>
    {
        private NextGenRenderSystem renderSystem;

        public Dictionary<ModelComponent, RenderModel> RenderModels => ComponentDatas;

        public NextGenModelProcessor(NextGenRenderSystem renderSystem) : base(typeof(TransformComponent))
        {
            this.renderSystem = renderSystem;
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

            foreach (var renderMesh in renderModel.Meshes)
            {
                var mesh = renderMesh.Mesh;

                // Copy world matrix
                var nodeIndex = mesh.NodeIndex;
                renderMesh.World = nodeTransformations[nodeIndex].WorldMatrix;
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
                    Material = modelComponent.Materials[materialIndex]  // Check ModelComponent.Materials first
                                     ?? model.Materials[materialIndex], // Otherwise, fallback to Model.Materials
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