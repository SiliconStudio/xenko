using System;
using System.Runtime.InteropServices;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Internals;
using SiliconStudio.Xenko.Rendering;


namespace SiliconStudio.Xenko.Particles
{
    public partial class ParticleBatch : BatchBase<ParticleBatch.ParticleDrawInfo>
    {
        private Matrix viewMatrix;
        private Matrix projMatrix;
        private Matrix invViewMatrix;
        private Vector4 vector4UnitX = Vector4.UnitX;
        private Vector4 vector4UnitY = -Vector4.UnitY;

        public ParticleBatch(GraphicsDevice device, int bufferElementCount = 1024, int batchCapacity = 64)
            : base(device, ParticleBatch.Bytecode, ParticleBatch.BytecodeSRgb, StaticQuadBufferInfo.CreateQuadBufferInfo("ParticleBatch.VertexIndexBuffer", true, bufferElementCount, batchCapacity), ParticleVertex.Layout)
        {
        }

        // TODO Implement
        protected unsafe override void UpdateBufferValuesFromElementInfo(ref ElementInfo elementInfo, IntPtr vertexPointer, IntPtr indexPointer, int vexterStartOffset)
        {
            var vertex = (ParticleVertex*)vertexPointer;

            var emitter = elementInfo.DrawInfo.Emitter;

            // TODO Ivnerse view
            var unitX = new Vector3(invViewMatrix.M11, invViewMatrix.M12, invViewMatrix.M13);
            var unitY = new Vector3(invViewMatrix.M21, invViewMatrix.M22, invViewMatrix.M23);
            //var unitX = new Vector3(0, 0, 0);
            //var unitY = new Vector3(0, 0, 0);

            var remainingCapacity = 2000;
            emitter.BuildVertexBuffer(vertexPointer, unitX, unitY, ref remainingCapacity);

            
        }

        public void Draw(Texture texture, ParticleEmitter emitter)
        {
            var drawInfo = new ParticleDrawInfo
            {
                Emitter = emitter,
            };

            // TODO Sort by depth
            float depthSprite = 1f;

            var totalParticles = emitter.pool.LivingParticles;

            var elementInfo = new ElementInfo(
                StaticQuadBufferInfo.VertexByElement * totalParticles, 
                StaticQuadBufferInfo.IndicesByElement * totalParticles, 
                ref drawInfo, 
                depthSprite);

            Draw(texture, ref elementInfo);

        }


        protected override void PrepareForRendering()
        {
            // TODO Setup my uniforms here

            // Setup the Transformation matrix of the shader
            //Parameters.Set(ParticleBaseKeys.MatrixTransform, transformationMatrix);

            Parameters.Set(ParticleBaseKeys.ViewMatrix, viewMatrix);
            Parameters.Set(ParticleBaseKeys.ProjectionMatrix, projMatrix);

            Parameters.Set(ParticleBaseKeys.InvViewX, new Vector4(invViewMatrix.M11, invViewMatrix.M12, invViewMatrix.M13, 0));
            Parameters.Set(ParticleBaseKeys.InvViewY, new Vector4(invViewMatrix.M21, invViewMatrix.M22, invViewMatrix.M23, 0));

            base.PrepareForRendering();
        }

        /// <summary>
        /// Begins a 3D sprite batch rendering using the specified sorting mode and blend state, sampler, depth stencil, rasterizer state objects, plus a custom effect and a view-projection matrix. 
        /// Passing null for any of the state objects selects the default default state objects (BlendState.AlphaBlend, DepthStencilState.Default, RasterizerState.CullCounterClockwise, SamplerState.LinearClamp). 
        /// Passing a null effect selects the default SpriteBatch Class shader. 
        /// </summary>
        /// <param name="sortMode">The sprite drawing order to use for the batch session</param>
        /// <param name="effect">The effect to use for the batch session</param>
        /// <param name="blendState">The blending state to use for the batch session</param>
        /// <param name="samplerState">The sampling state to use for the batch session</param>
        /// <param name="depthStencilState">The depth stencil state to use for the batch session</param>
        /// <param name="rasterizerState">The rasterizer state to use for the batch session</param>
        /// <param name="stencilValue">The value of the stencil buffer to take as reference for the batch session</param>
        /// <param name="viewProjection">The view-projection matrix to use for the batch session</param>
        public void Begin(Matrix viewMat, Matrix projMat, Matrix viewInv, SpriteSortMode sortMode = SpriteSortMode.Deferred, BlendState blendState = null, SamplerState samplerState = null, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, Effect effect = null, EffectParameterCollectionGroup parameterCollectionGroup = null, int stencilValue = 0)
        {
            CheckEndHasBeenCalled("begin");

            viewMatrix = viewMat;
            projMatrix = projMat;
            invViewMatrix = viewInv;

            Begin(effect, parameterCollectionGroup, sortMode, blendState, samplerState, depthStencilState, rasterizerState, stencilValue);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ParticleDrawInfo
        {
            public ParticleEmitter Emitter;
        }
    }
}
