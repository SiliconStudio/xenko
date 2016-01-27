// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL
using System;
using System.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Xenko.Shaders;
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
using OpenTK.Graphics.ES30;
using BinaryFormat = OpenTK.Graphics.ES30.ShaderBinaryFormat;
#else
using OpenTK.Graphics.OpenGL;
#endif

namespace SiliconStudio.Xenko.Graphics
{
    public partial class Shader
    {
        private Shader(GraphicsDevice device, ShaderStage shaderStage, byte[] shaderStageBytecode)
            : base(device)
        {
            this.stage = shaderStage;

            var shaderStageGl = ConvertShaderStage(shaderStage);

            // Decode shader StageBytecode
            var binarySerializationReader = new BinarySerializationReader(new MemoryStream(shaderStageBytecode));
            var shaderBytecodeData = new OpenGLShaderBytecodeData();
            shaderBytecodeData.Serialize(binarySerializationReader, ArchiveMode.Deserialize);

            using (GraphicsDevice.UseOpenGLCreationContext())
            {
                resourceId = GL.CreateShader(shaderStageGl);

                if (shaderBytecodeData.IsBinary)
                {
                    GL.ShaderBinary(1, ref resourceId, (BinaryFormat)shaderBytecodeData.BinaryFormat, shaderBytecodeData.Binary, shaderBytecodeData.Binary.Length);
                }
                else
                {
                    GL.ShaderSource(resourceId, shaderBytecodeData.Source);
                    GL.CompileShader(resourceId);

                    var log = GL.GetShaderInfoLog(resourceId);

                    int compileStatus;
                    GL.GetShader(resourceId, ShaderParameter.CompileStatus, out compileStatus);

                    if (compileStatus != 1)
                        throw new InvalidOperationException(string.Format("Error while compiling GLSL shader: {0}", log));
                }
            }
        }

        /// <inheritdoc/>
        protected override void Destroy()
        {
            using (GraphicsDevice.UseOpenGLCreationContext())
            {
                GL.DeleteShader(resourceId);
            }

            resourceId = 0;

            base.Destroy();
        }

        private static ShaderType ConvertShaderStage(ShaderStage shaderStage)
        {
            switch (shaderStage)
            {
                case ShaderStage.Pixel:
                    return ShaderType.FragmentShader;
                case ShaderStage.Vertex:
                    return ShaderType.VertexShader;
#if !SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                case ShaderStage.Geometry:
                    return ShaderType.GeometryShader;
#endif
                default:
                    throw new NotSupportedException();
            }
        }
    }
} 
#endif
