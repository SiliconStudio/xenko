using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Rendering.Editor;
using SiliconStudio.Xenko.Rendering.Images;

namespace SiliconStudio.Xenko.Rendering
{
    [DataContract("CameraRendererModePicking")]
    [NonInstantiable]
    public class CameraRendererModePicking : CameraRenderModeBase
    {
        private const int PickingTargetSize = 512;

        private Int3 pickingResult;
        private readonly Dictionary<int, Entity> idToEntity = new Dictionary<int, Entity>();
        private ImageReadback<Vector4> pickingReadback;
        private Texture pickingTexture;

        [DataMemberIgnore]
        public RenderStage PickingRenderStage { get; set; }

        protected override void InitializeCore()
        {
            base.InitializeCore();

            // Create optional render stages that don't exist yet
            if (PickingRenderStage == null)
                PickingRenderStage = EntityComponentRendererBase.GetOrCreateRenderStage(RenderSystem, "Picking", "Picking", new RenderOutputDescription(PixelFormat.R32G32B32A32_Float, PixelFormat.D24_UNorm_S8_UInt));

            if (PickingRenderStage != null)
            {
                MainRenderView.RenderStages.Add(PickingRenderStage);
            }
        }

        public override void BeforeExtract(RenderContext context)
        {
            base.BeforeExtract(context);

            if (RenderSystem.GetPipelinePlugin<MeshPipelinePlugin>(false) != null)
            {
                // If MeshPipelinePlugin exists and we have picking, let's enable PickingMeshPipelinePlugin
                RenderSystem.GetPipelinePlugin<PickingMeshPipelinePlugin>(true);
            }
        }
        protected override void DrawCore(RenderDrawContext context)
        {
            if (pickingReadback == null)
            {
                pickingReadback = ToLoadAndUnload(new ImageReadback<Vector4> { FrameDelayCount = 4 });
                pickingTexture = Texture.New2D(GraphicsDevice, 1, 1, PickingRenderStage.Output.RenderTargetFormat0, TextureFlags.None, 1, GraphicsResourceUsage.Staging).DisposeBy(this);
            }
            var inputManager = Context.Services.GetServiceAs<InputManager>();

            // TODO: Use RenderFrame
            var pickingRenderTarget = PushScopedResource(Context.Allocator.GetTemporaryTexture2D(PickingTargetSize, PickingTargetSize, PickingRenderStage.Output.RenderTargetFormat0));
            var pickingDepthStencil = PushScopedResource(Context.Allocator.GetTemporaryTexture2D(PickingTargetSize, PickingTargetSize, PickingRenderStage.Output.DepthStencilFormat, TextureFlags.DepthStencil));

            // Render the picking stage using the current view
            context.PushRenderTargets();
            {
                context.CommandList.Clear(pickingRenderTarget, Color.Transparent);
                context.CommandList.Clear(pickingDepthStencil, DepthStencilClearOptions.DepthBuffer);

                context.CommandList.SetDepthAndRenderTarget(pickingDepthStencil, pickingRenderTarget);
                RenderSystem.Draw(context, MainRenderView, PickingRenderStage);
            }
            context.PopRenderTargets();

            // Copy the correct texel and read it back
            // TODO: We could just render the scene to the single texel being picked
            CopyPicking(context, pickingRenderTarget, inputManager.MousePosition);
            pickingReadback.SetInput(pickingTexture);
            pickingReadback.Draw(context);

            // Result should be used during extract phase
            if (pickingReadback.IsResultAvailable)
            {
                var encodedResult = pickingReadback.Result[0];
                unsafe
                {
                    pickingResult = *(Int3*)&encodedResult;
                }
            }
        }

        private void CopyPicking(RenderDrawContext context, Texture pickingRenderTarget, Vector2 mousePosition)
        {
            var renderTargetSize = new Vector2(pickingRenderTarget.Width, pickingRenderTarget.Height);
            var positionInTexture = Vector2.Modulate(renderTargetSize, mousePosition);
            var region = new ResourceRegion(
                Math.Max(0, Math.Min((int)renderTargetSize.X - 1, (int)positionInTexture.X)),
                Math.Max(0, Math.Min((int)renderTargetSize.Y - 1, (int)positionInTexture.Y)),
                0,
                Math.Max(0, Math.Min((int)renderTargetSize.X - 1, (int)positionInTexture.X + 1)),
                Math.Max(0, Math.Min((int)renderTargetSize.Y - 1, (int)positionInTexture.Y + 1)),
                1);

            // Copy results to 1x1 target
            context.CommandList.CopyRegion(pickingRenderTarget, 0, region, pickingTexture, 0);
        }

        /// <summary>
        /// Cache all the components ids in the <see cref="idToEntity"/> dictionary.
        /// </summary>
        /// <param name="componentBase">the component to tag recursively.</param>
        /// <param name="isRecursive">indicate if cache should be built recursively</param>
        public void CacheComponentsId(ComponentBase componentBase, bool isRecursive)
        {
            var scene = componentBase as Scene;
            if (scene != null && isRecursive)
            {
                foreach (var entity in scene.Entities)
                    CacheComponentsId(entity, true);
            }
            else
            {
                var entity = componentBase as Entity;
                if (entity != null)
                {
                    foreach (var component in entity.Components)
                        idToEntity[RuntimeIdHelper.ToRuntimeId(component)] = entity;

                    if (isRecursive)
                    {
                        foreach (var child in entity.Transform.Children)
                            CacheComponentsId(child.Entity, true);
                    }
                }
            }
        }

        /// <summary>
        /// Uncache all the components ids in the <see cref="idToEntity"/> dictionary.
        /// </summary>
        /// <param name="entity">the entity to tag recursively.</param>
        /// <param name="isReccursive">indicate if cache should be built recursively</param>
        public void UncacheComponentsId(Entity entity, bool isReccursive)
        {
            foreach (var component in entity.Components)
            {
                var runtimeId = RuntimeIdHelper.ToRuntimeId(component);
                if (idToEntity.ContainsKey(runtimeId))
                    idToEntity.Remove(runtimeId);
            }

            if (isReccursive)
            {
                foreach (var child in entity.Transform.Children)
                    UncacheComponentsId(child.Entity, true);
            }
        }
        /// <summary>
        /// Gets the entity at the provided screen position
        /// </summary>
        /// <returns></returns>
        public EntityPickingResult Pick()
        {
            var result = new EntityPickingResult
            {
                ComponentId = pickingResult.X,
                MeshNodeIndex = pickingResult.Y,
                MaterialIndex = pickingResult.Z
            };
            result.Entity = idToEntity.ContainsKey(result.ComponentId) ? idToEntity[result.ComponentId] : null;
            return result;
        }
    }
}