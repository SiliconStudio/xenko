// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using SiliconStudio.Core.Storage;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Shaders.Ast;
using SiliconStudio.Shaders.Ast.Glsl;
using SiliconStudio.Shaders.Convertor;
using SiliconStudio.Shaders.Writer.Hlsl;
using ConstantBuffer = SiliconStudio.Shaders.Ast.Hlsl.ConstantBuffer;
using LayoutQualifier = SiliconStudio.Shaders.Ast.LayoutQualifier;
using ParameterQualifier = SiliconStudio.Shaders.Ast.Hlsl.ParameterQualifier;

namespace SiliconStudio.Paradox.Shaders.Compiler.OpenGL
{
    internal partial class ShaderCompiler : IShaderCompiler
    {
        static ShaderCompiler()
        {
            // Preload proper glsl optimizer native library (depending on CPU type)
            Core.NativeLibrary.PreloadLibrary("glsl_optimizer.dll");
        }

        /// <summary>
        /// Converts the hlsl code into glsl and stores the result as plain text
        /// </summary>
        /// <param name="shaderSource">the hlsl shader</param>
        /// <param name="entryPoint">the entrypoint function name</param>
        /// <param name="stage">the shader pipeline stage</param>
        /// <param name="compilerParameters"></param>
        /// <param name="reflection">the reflection gathered from the hlsl analysis</param>
        /// <param name="sourceFilename">the name of the source file</param>
        /// <returns></returns>
        public ShaderBytecodeResult Compile(string shaderSource, string entryPoint, ShaderStage stage, ShaderMixinParameters compilerParameters, EffectReflection reflection, string sourceFilename = null)
        {
            var isOpenGLES = compilerParameters.Get(CompilerParameters.GraphicsPlatformKey) == GraphicsPlatform.OpenGLES;
            var shaderBytecodeResult = new ShaderBytecodeResult();


            PipelineStage pipelineStage = PipelineStage.None;
            switch (stage)
            {
                case ShaderStage.Vertex:
                    pipelineStage = PipelineStage.Vertex;
                    break;
                case ShaderStage.Pixel:
                    pipelineStage = PipelineStage.Pixel;
                    break;
                case ShaderStage.Geometry:
                    shaderBytecodeResult.Error("Geometry stage can't be converted to OpenGL. Only Vertex and Pixel shaders are supported");
                    break;
                case ShaderStage.Hull:
                    shaderBytecodeResult.Error("Hull stage can't be converted to OpenGL. Only Vertex and Pixel shaders are supported");
                    break;
                case ShaderStage.Domain:
                    shaderBytecodeResult.Error("Domain stage can't be converted to OpenGL. Only Vertex and Pixel shaders are supported");
                    break;
                case ShaderStage.Compute:
                    shaderBytecodeResult.Error("Compute stage can't be converted to OpenGL. Only Vertex and Pixel shaders are supported");
                    break;
                default:
                    shaderBytecodeResult.Error("Unknown shader profile.");
                    break;
            }

            if (shaderBytecodeResult.HasErrors)
                return shaderBytecodeResult;
            
            // Convert from HLSL to GLSL
            // Note that for now we parse from shader as a string, but we could simply clone effectPass.Shader to avoid multiple parsing.
            var glslConvertor = new ShaderConverter(isOpenGLES);
            var glslShader = glslConvertor.Convert(shaderSource, entryPoint, pipelineStage, sourceFilename, shaderBytecodeResult);

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
                            pipelineStage == PipelineStage.Vertex
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
                    .AppendLine("#version 420")
                    .AppendLine();

                if (pipelineStage == PipelineStage.Pixel)
                    glslShaderCode
                        .AppendLine("out vec4 gl_FragData[1];")
                        .AppendLine();
            }

            if (isOpenGLES)
            {
                if (pipelineStage == PipelineStage.Pixel)
                    glslShaderCode
                        .AppendLine("precision highp float;")
                        .AppendLine();
            }

            glslShaderCode.Append(glslShaderWriter.Text);

            var realShaderSource = glslShaderCode.ToString();

            // optimize shader
            var optShaderSource = RunOptimizer(realShaderSource, isOpenGLES, false, pipelineStage == PipelineStage.Vertex);
            if (!String.IsNullOrEmpty(optShaderSource))
                realShaderSource = optShaderSource;

            var rawData = Encoding.ASCII.GetBytes(realShaderSource);
            var bytecodeId = ObjectId.FromBytes(rawData);
            var bytecode = new ShaderBytecode(bytecodeId, rawData);
            bytecode.Stage = stage;

            shaderBytecodeResult.Bytecode = bytecode;
            return shaderBytecodeResult;
        }

        private string RunOptimizer(string baseShader, bool openGLES, bool es30, bool vertex)
	    {
		    IntPtr ctx = IntPtr.Zero;
            var inputShader = baseShader;
            if (openGLES)
		    {
			    if (es30)
                    ctx = glslopt_initialize(2);
			    else
                    ctx = glslopt_initialize(1);

			    if (vertex)
			    {
                    var pre = "#define gl_Vertex _glesVertex\nattribute highp vec4 _glesVertex;\n";
				    pre += "#define gl_Normal _glesNormal\nattribute mediump vec3 _glesNormal;\n";
				    pre += "#define gl_MultiTexCoord0 _glesMultiTexCoord0\nattribute highp vec4 _glesMultiTexCoord0;\n";
				    pre += "#define gl_MultiTexCoord1 _glesMultiTexCoord1\nattribute highp vec4 _glesMultiTexCoord1;\n";
				    pre += "#define gl_Color _glesColor\nattribute lowp vec4 _glesColor;\n";
                    inputShader = pre + inputShader;
			    }
		    }
		    else
            {
                ctx = glslopt_initialize(0);
		    }

		    int type = vertex ? 0 : 1;
            var shader = glslopt_optimize(ctx, type, inputShader, 0);

		    bool optimizeOk = glslopt_get_status(shader);

            string shaderAsString = null;
		    if (optimizeOk)
		    {
                IntPtr optShader = glslopt_get_output(shader);
                shaderAsString = Marshal.PtrToStringAnsi(optShader);
		    }

            glslopt_shader_delete(shader);
            glslopt_cleanup(ctx);

            return shaderAsString;
	    }

        [DllImport("glsl_optimizer.dll", CallingConvention=CallingConvention.Cdecl)]
        private static extern IntPtr glslopt_initialize(int target);

        [DllImport("glsl_optimizer.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr glslopt_optimize(IntPtr ctx, int type, string shaderSource, uint options);

        [DllImport("glsl_optimizer.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool glslopt_get_status(IntPtr shader);

        [DllImport("glsl_optimizer.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr glslopt_get_output(IntPtr shader);

        [DllImport("glsl_optimizer.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void glslopt_shader_delete(IntPtr shader);

        [DllImport("glsl_optimizer.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void glslopt_cleanup(IntPtr ctx);
    }
}
