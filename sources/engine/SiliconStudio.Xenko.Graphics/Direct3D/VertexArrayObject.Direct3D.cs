// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_DIRECT3D
using System;
using System.Linq;
using SharpDX;
using SharpDX.Direct3D11;
using SiliconStudio.Core.ReferenceCounting;

namespace SiliconStudio.Paradox.Graphics
{
    public partial class VertexArrayObject
    {
        private readonly SharpDX.DXGI.Format indexFormat;
        private readonly int indexBufferOffset;
        internal readonly EffectInputSignature EffectInputSignature;
        private SharpDX.Direct3D11.VertexBufferBinding[] nativeVertexBufferBindings;
        private SharpDX.Direct3D11.Buffer nativeIndexBuffer;
        internal InputLayout InputLayout;

        // Cache previous use
        internal EffectInputSignature LastEffectInputSignature;
        internal InputLayout LastInputLayout;

        internal VertexArrayLayout Layout { get; private set; }

        private VertexArrayObject(GraphicsDevice graphicsDevice, EffectInputSignature shaderSignature, IndexBufferBinding indexBufferBinding, VertexBufferBinding[] vertexBufferBindings)
            : base(graphicsDevice)
        {
            this.vertexBufferBindings = vertexBufferBindings;
            this.indexBufferBinding = indexBufferBinding;
            this.EffectInputSignature = shaderSignature;

            // Calculate Direct3D11 InputElement
            int inputElementCount = vertexBufferBindings.Sum(t => t.Declaration.VertexElements.Length);
            var inputElements = new InputElement[inputElementCount];

            int j = 0;
            for (int i = 0; i < vertexBufferBindings.Length; i++)
            {
                var declaration = vertexBufferBindings[i].Declaration;
                vertexBufferBindings[i].Buffer.AddReferenceInternal();
                foreach (var vertexElementWithOffset in declaration.EnumerateWithOffsets())
                {
                    var vertexElement = vertexElementWithOffset.VertexElement;
                    inputElements[j++] = new InputElement
                        {
                            Slot = i,
                            SemanticName = vertexElement.SemanticName,
                            SemanticIndex = vertexElement.SemanticIndex,
                            AlignedByteOffset = vertexElementWithOffset.Offset,
                            Format = (SharpDX.DXGI.Format)vertexElement.Format,
                        };
                }
            }

            Layout = VertexArrayLayout.GetOrCreateLayout(new VertexArrayLayout(inputElements));

            if (indexBufferBinding != null)
            {
                indexBufferBinding.Buffer.AddReferenceInternal();
                indexBufferOffset = indexBufferBinding.Offset;
                indexFormat = (indexBufferBinding.Is32Bit ? SharpDX.DXGI.Format.R32_UInt : SharpDX.DXGI.Format.R16_UInt);
            }

            CreateResources();
        }

        void CreateResources()
        {
            // If we have a shader signature, we can store the input layout directly
            if (EffectInputSignature != null)
            {
                InputLayout = GraphicsDevice.InputLayoutManager.GetInputLayout(EffectInputSignature, Layout);
            }

            nativeVertexBufferBindings = vertexBufferBindings.Select(x => new SharpDX.Direct3D11.VertexBufferBinding(x.Buffer.NativeBuffer, x.Stride, x.Offset)).ToArray();

            if (indexBufferBinding != null)
            {
                nativeIndexBuffer = indexBufferBinding.Buffer.NativeBuffer;
            }
        }

        protected override void DestroyImpl()
        {
            if (InputLayout != null)
            {
                ((IUnknown)InputLayout).Release();
                InputLayout = null;
            }

            nativeVertexBufferBindings = null;
            nativeIndexBuffer = null;
        }

        /// <inheritdoc/>
        protected internal override void OnDestroyed()
        {
            base.OnDestroyed();
            DestroyImpl();
        }

        /// <inheritdoc/>
        protected internal override bool OnRecreate()
        {
            // Dependency: wait for underlying buffers to be recreated first
            foreach (var vertexBufferBinding in vertexBufferBindings)
            {
                if (vertexBufferBinding.Buffer.LifetimeState != GraphicsResourceLifetimeState.Active)
                    return false;
            }

            if (indexBufferBinding != null && indexBufferBinding.Buffer.LifetimeState != GraphicsResourceLifetimeState.Active)
                return false;

            CreateResources();
            return true;
        }

        internal void Apply(InputAssemblerStage inputAssemblerStage)
        {
            inputAssemblerStage.SetVertexBuffers(0, nativeVertexBufferBindings);
            inputAssemblerStage.SetIndexBuffer(nativeIndexBuffer, indexFormat, indexBufferOffset);
        }
    }
} 
#endif 
