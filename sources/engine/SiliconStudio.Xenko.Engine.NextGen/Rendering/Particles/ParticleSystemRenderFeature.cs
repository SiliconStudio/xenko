// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering.Particles
{
    public class ParticleSystemRenderFeature : RootEffectRenderFeature
    {
        public override bool SupportsRenderObject(RenderObject renderObject)
        {
            return renderObject is RenderParticleSystem;
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Extract()
        {
            base.Extract();
        }

        public override void Prepare(RenderContext context)
        {
            base.Prepare(context);
        }

        public override void PrepareEffectPermutations()
        {
            base.PrepareEffectPermutations();
        }

        public override void Draw(RenderDrawContext context, RenderView renderView, RenderViewStage renderViewStage, int startIndex, int endIndex)
        {
            base.Draw(context, renderView, renderViewStage, startIndex, endIndex);

            Matrix viewInverse;
            Matrix.Invert(ref renderView.View, out viewInverse);

            for (var index = startIndex; index <= endIndex; index++)
            {
                var renderNodeReference = renderViewStage.RenderNodes[index].RenderNode;
                var renderNode = GetRenderNode(renderNodeReference);

                var renderParticleSystem = (RenderParticleSystem)renderNode.RenderObject;

                // Get effect
                var renderEffect = renderNode.RenderEffect;

                // TODO GRAPHICS REFACTOR: Extract data
                var particleSystemComponent = renderParticleSystem.ParticleSystemComponent;
                var particleSystem = particleSystemComponent.ParticleSystem;

                particleSystemComponent.ParticleSystem.Draw(context, ref renderView.View, ref renderView.Projection, ref viewInverse, particleSystemComponent.Color);
            }
        }
        
    }
}