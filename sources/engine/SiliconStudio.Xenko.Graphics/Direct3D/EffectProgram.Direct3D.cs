// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D 
using System.Collections.Generic;
using SharpDX.Direct3D11;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Graphics
{
    internal partial class EffectProgram
    {
        private readonly EffectBytecode effectBytecode;
        private VertexShader vertexShader;
        private GeometryShader geometryShader;
        private PixelShader pixelShader;
        private HullShader hullShader;
        private DomainShader domainShader;
        private ComputeShader computeShader;
        private EffectInputSignature inputSignature;

        private EffectProgram(GraphicsDevice device, EffectBytecode bytecode)
            : base(device)
        {
            effectBytecode = bytecode;
            Reflection = effectBytecode.Reflection;
            CreateShaders();
        }

        public EffectInputSignature InputSignature
        {
            get
            {
                return inputSignature;
            }
        }

        private void CreateShaders()
        {
            foreach (var shaderBytecode in effectBytecode.Stages)
            {
                var bytecodeRaw = shaderBytecode.Data;
                var reflection = effectBytecode.Reflection;

                // TODO CACHE Shaders with a bytecode hash
                switch (shaderBytecode.Stage)
                {
                    case ShaderStage.Vertex:
                        vertexShader = new VertexShader(GraphicsDevice.NativeDevice, bytecodeRaw);
                        // Note: input signature can be reused when reseting device since it only stores non-GPU data,
                        // so just keep it if it has already been created before.
                        if (inputSignature == null)
                            inputSignature = EffectInputSignature.GetOrCreateLayout(new EffectInputSignature(shaderBytecode.Id, bytecodeRaw));
                        break;
                    case ShaderStage.Domain:
                        domainShader = new DomainShader(GraphicsDevice.NativeDevice, bytecodeRaw);
                        break;
                    case ShaderStage.Hull:
                        hullShader = new HullShader(GraphicsDevice.NativeDevice, bytecodeRaw);
                        break;
                    case ShaderStage.Geometry:
                        if (reflection.ShaderStreamOutputDeclarations != null && reflection.ShaderStreamOutputDeclarations.Count > 0)
                        {
                            // Calculate the strides
                            var soStrides = new List<int>();
                            foreach (var streamOutputElement in reflection.ShaderStreamOutputDeclarations)
                            {
                                for (int i = soStrides.Count; i < (streamOutputElement.Stream + 1); i++)
                                {
                                    soStrides.Add(0);
                                }

                                soStrides[streamOutputElement.Stream] += streamOutputElement.ComponentCount * sizeof(float);
                            }
                            var soElements = new StreamOutputElement[0]; // TODO CREATE StreamOutputElement from bytecode.Reflection.ShaderStreamOutputDeclarations
                            geometryShader = new GeometryShader(GraphicsDevice.NativeDevice, bytecodeRaw, soElements, soStrides.ToArray(), reflection.StreamOutputRasterizedStream);
                        }
                        else
                        {
                            geometryShader = new GeometryShader(GraphicsDevice.NativeDevice, bytecodeRaw);
                        }
                        break;
                    case ShaderStage.Pixel:
                        pixelShader = new PixelShader(GraphicsDevice.NativeDevice, bytecodeRaw);
                        break;
                    case ShaderStage.Compute:
                        computeShader = new ComputeShader(GraphicsDevice.NativeDevice, bytecodeRaw);
                        break;
                }
            }
        }

        protected override void DestroyImpl()
        {
            Utilities.Dispose(ref pixelShader);
            Utilities.Dispose(ref vertexShader);
            Utilities.Dispose(ref geometryShader);
            Utilities.Dispose(ref hullShader);
            Utilities.Dispose(ref domainShader);
            Utilities.Dispose(ref computeShader);
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
            base.OnRecreate();
            CreateShaders();
            return true;
        }

        internal void Apply(GraphicsDevice device)
        {
            // Not sure weither setting everything to NULL is important if no program is specified?
            var nativeDeviceContext = device.NativeDeviceContext;
            if (computeShader != null)
            {
                nativeDeviceContext.ComputeShader.Set(computeShader);
            }
            else
            {
                nativeDeviceContext.VertexShader.Set(vertexShader);
                nativeDeviceContext.PixelShader.Set(pixelShader);
                nativeDeviceContext.HullShader.Set(hullShader);
                nativeDeviceContext.DomainShader.Set(domainShader);
                nativeDeviceContext.GeometryShader.Set(geometryShader);
            }
        }
    }
}
 
#endif 
