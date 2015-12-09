using System;
using System.Runtime.InteropServices;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Internals;
using SiliconStudio.Xenko.Particles.ShapeBuilders;
using SiliconStudio.Xenko.Particles.VertexLayouts;
using SiliconStudio.Xenko.Rendering;


namespace SiliconStudio.Xenko.Particles
{
    public partial class ParticleBatch : BatchBase<ParticleBatch.ParticleDrawInfo>
    {
        private Matrix transformationMatrix;

        private Matrix viewMatrix;
        private Matrix projMatrix;
        private Matrix invViewMatrix;
//        private Vector4 vector4UnitX = Vector4.UnitX;
//        private Vector4 vector4UnitY = -Vector4.UnitY;

        private const int maxQuadCount = 1024 * 16;

        public ParticleBatch(GraphicsDevice device, int bufferElementCount = maxQuadCount, int batchCapacity = 64)
            : base(device, ParticleBatch.Bytecode(ParticleEffectVariation.None), ParticleBatch.Bytecode(ParticleEffectVariation.IsSrgb), StaticQuadBufferInfo.CreateQuadBufferInfo("ParticleBatch.VertexIndexBuffer", true, bufferElementCount, batchCapacity), ParticleVertexLayout.VertexDeclaration)
        {
            SortMode = SpriteSortMode.Immediate;
        }

        protected override void UpdateBufferValuesFromElementInfo(ref ElementInfo elementInfo, IntPtr vertexPointer, IntPtr indexPointer, int vexterStartOffset)
        {
            // TODO Setup material - here is also ok

            var emitter = elementInfo.DrawInfo.Emitter;
            var context = elementInfo.DrawInfo.Context;
            var color = elementInfo.DrawInfo.Color;

            


            emitter.Setup(GraphicsDevice, context, viewMatrix, projMatrix, color);
            


            var unitX = new Vector3(invViewMatrix.M11, invViewMatrix.M12, invViewMatrix.M13);
            var unitY = new Vector3(invViewMatrix.M21, invViewMatrix.M22, invViewMatrix.M23);

            var remainingCapacity = maxQuadCount;
            emitter.BuildVertexBuffer(vertexPointer, unitX, unitY, ref remainingCapacity);
        }

        public void Draw(ParticleEmitter emitter, RenderContext context, Color4 color)
        {
            var drawInfo = new ParticleDrawInfo
            {
                Emitter = emitter,
                Context = context,
                Color = color,
            };

            // TODO Sort by depth
            float depthSprite = 1f;

            var requiredQuads = emitter.GetRequiredQuadCount();

            var elementInfo = new ElementInfo(
                StaticQuadBufferInfo.VertexByElement  * requiredQuads, 
                StaticQuadBufferInfo.IndicesByElement * requiredQuads, 
                ref drawInfo, 
                depthSprite);

            // TODO Setup material - here is ok

            Draw(null, ref elementInfo);

        }


        protected override void PrepareForRendering()
        {
            // TODO Setup my uniforms here

            // Setup the Transformation matrix of the shader
            Parameters.Set(ParticleBaseKeys.MatrixTransform, transformationMatrix);

            /*
            Parameters.Set(ParticleBaseKeys.ViewMatrix, viewMatrix);
            Parameters.Set(ParticleBaseKeys.ProjectionMatrix, projMatrix);

            Parameters.Set(ParticleBaseKeys.InvViewX, new Vector4(invViewMatrix.M11, invViewMatrix.M12, invViewMatrix.M13, 0));
            Parameters.Set(ParticleBaseKeys.InvViewY, new Vector4(invViewMatrix.M21, invViewMatrix.M22, invViewMatrix.M23, 0));
            //*/

            base.PrepareForRendering();
        }

        public void Begin(Matrix viewMat, Matrix projMat, Matrix viewInv, BlendState blendState = null, SamplerState samplerState = null, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, Effect effect = null, EffectParameterCollectionGroup parameterCollectionGroup = null, int stencilValue = 0)
        {
            CheckEndHasBeenCalled("begin");

            transformationMatrix = viewMat * projMat;
            viewMatrix = viewMat;
            projMatrix = projMat;
            invViewMatrix = viewInv;

            Begin(effect, parameterCollectionGroup, SpriteSortMode.Immediate, blendState, samplerState, depthStencilState, rasterizerState, stencilValue);
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct ParticleDrawInfo
        {
            public ParticleEmitter Emitter;
            public RenderContext Context;
            public Color4 Color;
        }
    }
}
