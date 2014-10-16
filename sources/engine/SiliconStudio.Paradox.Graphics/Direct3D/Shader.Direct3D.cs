// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_DIRECT3D
using System;
using SharpDX.Direct3D11;

using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Graphics
{
    public partial class Shader
    {
        internal byte[] NativeInputSignature;

        private Shader(GraphicsDevice device, ShaderStage shaderStage, byte[] shaderBytecode)
            : base(device)
        {
            this.stage = shaderStage;

            switch (shaderStage)
            {
                case ShaderStage.Vertex:
                    NativeDeviceChild = new VertexShader(device.NativeDevice, shaderBytecode);
                    NativeInputSignature = shaderBytecode;
                    break;
                case ShaderStage.Hull:
                    NativeDeviceChild = new HullShader(device.NativeDevice, shaderBytecode);
                    break;
                case ShaderStage.Domain:
                    NativeDeviceChild = new DomainShader(device.NativeDevice, shaderBytecode);
                    break;
                case ShaderStage.Geometry:
                    NativeDeviceChild = new GeometryShader(device.NativeDevice, shaderBytecode);
                    break;
                case ShaderStage.Pixel:
                    NativeDeviceChild = new PixelShader(device.NativeDevice, shaderBytecode);
                    break;
                case ShaderStage.Compute:
                    NativeDeviceChild = new ComputeShader(device.NativeDevice, shaderBytecode);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("shaderStage");
            }
        }
    }
} 
#endif
