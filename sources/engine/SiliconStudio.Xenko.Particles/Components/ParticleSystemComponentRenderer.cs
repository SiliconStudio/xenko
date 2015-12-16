// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Net;
using System.Windows.Data;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Sprites;
using SiliconStudio.Xenko.Shaders.Compiler;
using System.Runtime.CompilerServices;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Particles.Components
{
    /// <summary>
    /// This <see cref="ParticleSystemComponentRenderer"/> is responsible for preparing and rendering the particles for a specific pass.
    /// </summary>

    class ParticleSystemComponentRenderer : EntityComponentRendererBase
    {
        // TODO For now try to render particle systems as Sprites, later move on to a proper particle representation

     private ParticleSystemProcessor particleSystemProcessor;

        protected override void InitializeCore()
        {
            base.InitializeCore();
        }

        protected override void Unload()
        {
//            particleBatch.Dispose();

            base.Unload();
        }

        protected override void PrepareCore(RenderContext context, RenderItemCollection opaqueList, RenderItemCollection transparentList)
        {
            // Early out if particle system processor doesn't exist
            particleSystemProcessor = SceneInstance.GetProcessor<ParticleSystemProcessor>();
            if (particleSystemProcessor == null)
            {
                return;
            }

            // Early out if camera doesn't exist
            var camera = context.GetCurrentCamera();
            if (camera == null)
            {
                return;
            }

            var viewProjectionMatrix = camera.ViewProjectionMatrix;

            foreach (var particleSystemState in particleSystemProcessor.ParticleSystems)
            {
                // Perform culling on group and accept
                // TODO Should culling be performed on a per-sprite basis or batched?
                if (!CurrentCullingMask.Contains(particleSystemState.ParticleSystemComponent.Entity.Group))
                    continue;

                // Project the position to find depth for sorting
                var worldPosition = new Vector4(particleSystemState.TransformComponent.WorldMatrix.TranslationVector, 1.0f);
                Vector4 projectedPosition;
                Vector4.Transform(ref worldPosition, ref viewProjectionMatrix, out projectedPosition);
                var projectedZ = projectedPosition.Z / projectedPosition.W;

                var list = true ? transparentList : opaqueList;

                // TODO Sort value based on custom key
                list.Add(new RenderItem(this, particleSystemState, projectedZ));
            }
        }

        protected override void DrawCore(RenderContext context, RenderItemCollection renderItems, int fromIndex, int toIndex)
        {
            var viewParameters = context.Parameters;
            var device = context.GraphicsDevice;

            // var viewProjection = viewParameters.Get(TransformationKeys.ViewProjection);
            var viewMat = viewParameters.Get(TransformationKeys.View);
            var projMat = viewParameters.Get(TransformationKeys.Projection);

            Matrix viewInv;
            Matrix.Invert(ref viewMat, out viewInv);

            for (var i = fromIndex; i <= toIndex; i++)
            {
                var renderItem = renderItems[i];
                var particleSystemState = (ParticleSystemProcessor.ParticleSystemComponentState)renderItem.DrawContext;
                var particleSystemComponent = particleSystemState.ParticleSystemComponent;

                particleSystemComponent.ParticleSystem.Draw(device, context, ref viewMat, ref projMat, ref viewInv, particleSystemComponent.Color);
            }
        }


        protected override void PreDrawCore(RenderContext context)
        {
            base.PreDrawCore(context);
            // Custom pre draw code
        }

        protected override void PostDrawCore(RenderContext context)
        {
            base.PostDrawCore(context);
            // Custom post draw code
        }
    }
}
