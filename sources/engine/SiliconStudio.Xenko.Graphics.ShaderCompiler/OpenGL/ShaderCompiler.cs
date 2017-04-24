// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Xenko.Shaders;
using SiliconStudio.Shaders.Ast.Hlsl;
using LayoutQualifier = SiliconStudio.Shaders.Ast.Glsl.LayoutQualifier;
using ParameterQualifier = SiliconStudio.Shaders.Ast.ParameterQualifier;
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
using System;
using System.IO;
using System.Linq;
using System.Text;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Shaders.Ast;
using SiliconStudio.Shaders.Ast.Glsl;
using SiliconStudio.Shaders.Convertor;
using SiliconStudio.Shaders.Writer.Hlsl;

namespace SiliconStudio.Xenko.Graphics.ShaderCompiler.OpenGL
{
    public class ShaderCompiler : IShaderCompilerOld
    {
        private bool isOpenGLES;

        public ShaderCompiler(bool isOpenGLES = false)
        {
            this.isOpenGLES = isOpenGLES;
        }

        /// <inheritdoc/>
        public CompilationResult Compile(string shaderSource, string entryPoint, string profile, string sourceFileName = "unknown")
        {
            string realShaderSource;

            if (profile == "glsl")
            {
                // Compile directly as GLSL
                realShaderSource = shaderSource;
            }
            else
            {
                // Convert HLSL to GLSL
                PipelineStage stage;
                var profileParts = profile.Split('_');
                switch (profileParts[0])
                {
                    case "vs":
                        stage = PipelineStage.Vertex;
                        break;
                    case "ps":
                        stage = PipelineStage.Pixel;
                        break;
                    case "gs":
                    case "hs":
                    case "ds":
                    case "cs":
                        throw new NotImplementedException("This shader stage can't be converted to OpenGL.");
                    default:
                        throw new NotSupportedException("Unknown shader profile.");
                }

                // Convert from HLSL to GLSL
                // Note that for now we parse from shader as a string, but we could simply clone effectPass.Shader to avoid multiple parsing.
                var glslConvertor = new ShaderConverter(isOpenGLES);
                var glslShader = glslConvertor.Convert(shaderSource, entryPoint, stage);

                // Add std140 layout
                foreach (var constantBuffer in glslShader.Declarations.OfType<ConstantBuffer>())
                {
                    constantBuffer.Qualifiers |= new LayoutQualifier(new LayoutKeyValue("std140"));
                }

                // Output the result
                var glslShaderWriter = new HlslToGlslWriter();

                if (isOpenGLES)
                {
                    glslShaderWriter.TrimFloatSuffix = true;
                    glslShaderWriter.GenerateUniformBlocks = false;
                    foreach (var variable in glslShader.Declarations.OfType<Variable>())
                    {
                        if (variable.Qualifiers.Contains(ParameterQualifier.In))
                        {
                            variable.Qualifiers.Values.Remove(ParameterQualifier.In);
                            // "in" becomes "attribute" in VS, "varying" in other stages
                            variable.Qualifiers.Values.Add(
                                stage == PipelineStage.Vertex
                                    ? global::SiliconStudio.Shaders.Ast.Glsl.ParameterQualifier.Attribute
                                    : global::SiliconStudio.Shaders.Ast.Glsl.ParameterQualifier.Varying);
                        }
                        if (variable.Qualifiers.Contains(ParameterQualifier.Out))
                        {
                            variable.Qualifiers.Values.Remove(ParameterQualifier.Out);
                            variable.Qualifiers.Values.Add(global::SiliconStudio.Shaders.Ast.Glsl.ParameterQualifier.Varying);
                        }
                    }
                }

                // Write shader
                glslShaderWriter.Visit(glslShader);

                // Build shader source
                var glslShaderCode = new StringBuilder();

                // Append some header depending on target
                if (!isOpenGLES)
                {
                    glslShaderCode
                        .AppendLine("#version 410")
                        .AppendLine();

                    if (stage == PipelineStage.Pixel)
                        glslShaderCode
                            .AppendLine("out vec4 gl_FragData[1];")
                            .AppendLine();
                }

                if (isOpenGLES)
                {
                    if (stage == PipelineStage.Pixel)
                        glslShaderCode
                            .AppendLine("precision mediump float;")
                            .AppendLine();
                }

                glslShaderCode.Append(glslShaderWriter.Text);

                realShaderSource = glslShaderCode.ToString();
            }

            var shaderBytecodeData = new OpenGLShaderBytecodeData
                {
                    IsBinary = false,
                    EntryPoint = entryPoint,
                    Profile = profile,
                    Source = realShaderSource,
                };

            // Encode shader source to a byte array (no universal StageBytecode format for OpenGL)
            var memoryStream = new MemoryStream();
            var binarySerializationWriter = new BinarySerializationWriter(memoryStream);
            shaderBytecodeData.Serialize(binarySerializationWriter, ArchiveMode.Serialize);

            return new CompilationResult(new ShaderBytecode(memoryStream.ToArray()), false, null);
        }

        /// <inheritdoc/>
        public ShaderBytecode StripReflection(ShaderBytecode shaderBytecode)
        {
            // Doesn't exist in OpenGL.
            return shaderBytecode;
        }

        /// <inheritdoc/>
        public ShaderReflectionOld GetReflection(ShaderBytecode shaderBytecode)
        {
            // TODO: Implement this for separate shaders?
            // Otherwise it is only available at runtime.
            return null;
        }
    }
}
#endif
