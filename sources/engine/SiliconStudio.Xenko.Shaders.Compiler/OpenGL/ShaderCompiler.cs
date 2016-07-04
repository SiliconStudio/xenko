// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Storage;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Shaders.Ast;
using SiliconStudio.Shaders.Ast.Glsl;
using SiliconStudio.Shaders.Ast.Hlsl;
using SiliconStudio.Shaders.Convertor;
using SiliconStudio.Shaders.Writer.Hlsl;
using ConstantBuffer = SiliconStudio.Shaders.Ast.Hlsl.ConstantBuffer;
using LayoutQualifier = SiliconStudio.Shaders.Ast.LayoutQualifier;
using ParameterQualifier = SiliconStudio.Shaders.Ast.Hlsl.ParameterQualifier;
using StorageQualifier = SiliconStudio.Shaders.Ast.StorageQualifier;

namespace SiliconStudio.Xenko.Shaders.Compiler.OpenGL
{
    internal partial class ShaderCompiler : IShaderCompiler
    {
        private static readonly object GlslOptimizerLock = new object();

        static ShaderCompiler()
        {
            // Preload proper glsl optimizer native library (depending on CPU type)
            Core.NativeLibrary.PreloadLibrary("glsl_optimizer.dll");
        }

        private int renderTargetCount;

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="rtCount">The number of render targets</param>
        public ShaderCompiler(int rtCount)
        {
            renderTargetCount = rtCount;
        }

        /// <summary>
        /// Converts the hlsl code into glsl and stores the result as plain text
        /// </summary>
        /// <param name="shaderSource">the hlsl shader</param>
        /// <param name="entryPoint">the entrypoint function name</param>
        /// <param name="stage">the shader pipeline stage</param>
        /// <param name="effectParameters"></param>
        /// <param name="reflection">the reflection gathered from the hlsl analysis</param>
        /// <param name="sourceFilename">the name of the source file</param>
        /// <returns></returns>
        public ShaderBytecodeResult Compile(string shaderSource, string entryPoint, ShaderStage stage, EffectCompilerParameters effectParameters, EffectReflection reflection, string sourceFilename = null)
        {
            var shaderBytecodeResult = new ShaderBytecodeResult();
            byte[] rawData;

            var inputAttributeNames = new Dictionary<int, string>();
            var resourceBindings = new Dictionary<string, int>();

            GlslShaderPlatform shaderPlatform;
            int shaderVersion;

            switch (effectParameters.Platform)
            {
                case GraphicsPlatform.OpenGL:
                    shaderPlatform = GlslShaderPlatform.OpenGL;
                    shaderVersion = 420;
                    break;
                case GraphicsPlatform.OpenGLES:
                    shaderPlatform = GlslShaderPlatform.OpenGLES;
                    shaderVersion = effectParameters.Profile >= GraphicsProfile.Level_10_0 ? 300 : 100;
                    break;
                case GraphicsPlatform.Vulkan:
                    shaderPlatform = GlslShaderPlatform.Vulkan;
                    shaderVersion = 450;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("effectParameters.Platform");
            }

            var shader = Compile(shaderSource, entryPoint, stage, shaderPlatform, shaderVersion, shaderBytecodeResult, reflection, inputAttributeNames, resourceBindings, sourceFilename);

            if (shader == null)
                return shaderBytecodeResult;

            if (effectParameters.Platform == GraphicsPlatform.OpenGLES)
            {
                // store both ES 2 and ES 3 on OpenGL ES platforms
                var shaderBytecodes = new ShaderLevelBytecode();
                if (effectParameters.Profile >= GraphicsProfile.Level_10_0)
                {
                    shaderBytecodes.DataES3 = shader;
                    shaderBytecodes.DataES2 = null;
                }
                else
                {
                    shaderBytecodes.DataES2 = shader;
                    shaderBytecodes.DataES3 = Compile(shaderSource, entryPoint, stage, GlslShaderPlatform.OpenGLES, 300, shaderBytecodeResult, reflection, inputAttributeNames, resourceBindings, sourceFilename);
                }
                using (var stream = new MemoryStream())
                {
                    BinarySerialization.Write(stream, shaderBytecodes);
#if !SILICONSTUDIO_RUNTIME_CORECLR
                    rawData = stream.GetBuffer();
#else
// FIXME: Manu: The call to "ToArray()" might be slower than "GetBuffer()"
                    rawData = stream.ToArray();
#endif
                }
            }
            else if (effectParameters.Platform == GraphicsPlatform.Vulkan)
            {
                string inputFileExtension;
                switch (stage)
                {
                    case ShaderStage.Vertex: inputFileExtension = ".vert"; break;
                    case ShaderStage.Pixel: inputFileExtension = ".frag"; break;
                    case ShaderStage.Geometry: inputFileExtension = ".geom"; break;
                    case ShaderStage.Domain: inputFileExtension = ".tese"; break;
                    case ShaderStage.Hull: inputFileExtension = ".tesc"; break;
                    case ShaderStage.Compute: inputFileExtension = ".comp"; break;
                    default:
                        shaderBytecodeResult.Error("Unknown shader profile");
                        return shaderBytecodeResult;
                }

                var inputFileName = Path.ChangeExtension(Path.GetTempFileName(), inputFileExtension);
                var outputFileName = Path.ChangeExtension(inputFileName, ".spv");

                // Write shader source to disk
                File.WriteAllBytes(inputFileName, Encoding.ASCII.GetBytes(shader));

                // Run shader compiler
                var process = new Process
                {
                    StartInfo =
                    {
#if SILICONSTUDIO_PLATFORM_WINDOWS
                        FileName = "glslangValidator.exe",
#else
                        FileName = "glslangValidator",
#endif
                        Arguments = $"-V -s -o {outputFileName} {inputFileName}",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                process.WaitForExit();

                if (!File.Exists(outputFileName))
                {
                    shaderBytecodeResult.Error("Failed to generate SPIR-V from GLSL");
                    return shaderBytecodeResult;
                }

                // Read compiled shader
                var shaderBytecodes = new ShaderInputBytecode
                {
                    InputAttributeNames = inputAttributeNames,
                    ResourceBindings = resourceBindings,
                    Data = File.ReadAllBytes(outputFileName),
                };

                using (var stream = new MemoryStream())
                {
                    BinarySerialization.Write(stream, shaderBytecodes);
                    rawData = stream.ToArray();
                }

                // Cleanup temp files
                File.Delete(inputFileName);
                File.Delete(outputFileName);
            }
            else
            {
                // store string on OpenGL platforms
                rawData = Encoding.ASCII.GetBytes(shader);
            }
            
            var bytecodeId = ObjectId.FromBytes(rawData);
            var bytecode = new ShaderBytecode(bytecodeId, rawData);
            bytecode.Stage = stage;

            shaderBytecodeResult.Bytecode = bytecode;
            
            return shaderBytecodeResult;
        }

        private string Compile(string shaderSource, string entryPoint, ShaderStage stage, GlslShaderPlatform shaderPlatform, int shaderVersion, ShaderBytecodeResult shaderBytecodeResult, EffectReflection reflection, IDictionary<int, string> inputAttributeNames, Dictionary<string, int> resourceBindings, string sourceFilename = null)
        {
            if (shaderPlatform == GlslShaderPlatform.OpenGLES && shaderVersion < 300 && renderTargetCount > 1)
                shaderBytecodeResult.Error("OpenGL ES 2 does not support multiple render targets.");

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
                return null;

            Shader glslShader;

            // null entry point means no shader. In that case, we return a default function in HlslToGlslWriter
            // TODO: support that directly in HlslToGlslConvertor?
            if (entryPoint == null)
            {
                glslShader = null;
            }
            else
            {
                // Convert from HLSL to GLSL
                // Note that for now we parse from shader as a string, but we could simply clone effectPass.Shader to avoid multiple parsing.
                var glslConvertor = new ShaderConverter(shaderPlatform, shaderVersion);
                glslShader = glslConvertor.Convert(shaderSource, entryPoint, pipelineStage, sourceFilename, inputAttributeNames, shaderBytecodeResult);

                if (glslShader == null || shaderBytecodeResult.HasErrors)
                    return null;

                foreach (var constantBuffer in glslShader.Declarations.OfType<ConstantBuffer>())
                {
                    // Update constant buffer itself (first time only)
                    var reflectionConstantBuffer = reflection.ConstantBuffers.FirstOrDefault(x => x.Name == constantBuffer.Name && x.Size == 0);
                    if (reflectionConstantBuffer != null)
                    {
                        // Used to compute constant buffer size and member offsets (std140 rule)
                        int constantBufferOffset = 0;

                        // Fill members
                        for (int index = 0; index < reflectionConstantBuffer.Members.Length; index++)
                        {
                            var member = reflectionConstantBuffer.Members[index];

                            // Properly compute size and offset according to std140 rules
                            var memberSize = ComputeMemberSize(ref member.Type, ref constantBufferOffset);

                            // Store size/offset info
                            member.Offset = constantBufferOffset;
                            member.Size = memberSize;

                            // Adjust offset for next item
                            constantBufferOffset += memberSize;

                            reflectionConstantBuffer.Members[index] = member;
                        }

                        reflectionConstantBuffer.Size = constantBufferOffset;
                    }

                    // Find binding
                    var resourceBindingIndex = reflection.ResourceBindings.IndexOf(x => x.RawName == constantBuffer.Name);
                    if (resourceBindingIndex != -1)
                    {
                        MarkResourceBindingAsUsed(reflection, resourceBindingIndex, stage);
                    }
                }
                
                foreach (var variable in glslShader.Declarations.OfType<Variable>().Where(x => (x.Qualifiers.Contains(StorageQualifier.Uniform))))
                {
                    // Check if we have a variable that starts or ends with this name (in case of samplers)
                    // TODO: Have real AST support for all the list in Keywords.glsl
                    if (variable.Type.Name.Text.Contains("sampler1D")
                        || variable.Type.Name.Text.Contains("sampler2D")
                        || variable.Type.Name.Text.Contains("sampler3D")
                        || variable.Type.Name.Text.Contains("samplerCube")
                        || variable.Type.Name.Text.Contains("samplerBuffer"))
                    {
                        // TODO: Make more robust
                        var textureBindingIndex = reflection.ResourceBindings.IndexOf(x => variable.Name.ToString().StartsWith(x.RawName));
                        var samplerBindingIndex = reflection.ResourceBindings.IndexOf(x => variable.Name.ToString().EndsWith(x.RawName));

                        if (textureBindingIndex != -1)
                            MarkResourceBindingAsUsed(reflection, textureBindingIndex, stage);

                        if (samplerBindingIndex != -1)
                            MarkResourceBindingAsUsed(reflection, samplerBindingIndex, stage);
                    }
                    else
                    {
                        var resourceBindingIndex = reflection.ResourceBindings.IndexOf(x => x.RawName == variable.Name);
                        if (resourceBindingIndex != -1)
                        {
                            MarkResourceBindingAsUsed(reflection, resourceBindingIndex, stage);
                        }
                    }
                }

                if (shaderPlatform == GlslShaderPlatform.Vulkan)
                {
                    // Defines the ordering of resource groups in Vulkan. This is mirrored in the PipelineState
                    var resourceGroups = reflection.ResourceBindings.Select(x => x.ResourceGroup ?? "Globals").Distinct().ToList();

                    var bindings = resourceGroups.SelectMany(resourceGroup => reflection.ResourceBindings
                        .Where(x => x.ResourceGroup == resourceGroup || (x.ResourceGroup == null && resourceGroup == "Globals"))
                        .GroupBy(x => new { KeyName = x.KeyInfo.KeyName, RawName = x.RawName, Class = x.Class, Type = x.Type, SlotCount = x.SlotCount, LogicalGroup = x.LogicalGroup })
                        .OrderBy(x => x.Key.Class == EffectParameterClass.ConstantBuffer ? 0 : 1))
                        .ToList();

                    // Add layout(set, bindings) qualifier to all constant buffers
                    foreach (var constantBuffer in glslShader.Declarations.OfType<ConstantBuffer>())
                    {
                        var layoutBindingIndex = bindings.IndexOf(x => x.Key.RawName == constantBuffer.Name);
                        if (layoutBindingIndex != -1)
                        {
                            var layoutQualifier = constantBuffer.Qualifiers.OfType<SiliconStudio.Shaders.Ast.Glsl.LayoutQualifier>().FirstOrDefault();
                            if (layoutQualifier == null)
                            {
                                layoutQualifier = new SiliconStudio.Shaders.Ast.Glsl.LayoutQualifier();
                                constantBuffer.Qualifiers |= layoutQualifier;
                            }

                            //layoutQualifier.Layouts.Add(new LayoutKeyValue("set", resourceGroups.IndexOf(resourceGroup)));
                            layoutQualifier.Layouts.Add(new LayoutKeyValue("set", 0));
                            layoutQualifier.Layouts.Add(new LayoutKeyValue("binding", layoutBindingIndex + 1));

                            resourceBindings.Add(bindings[layoutBindingIndex].Key.KeyName, layoutBindingIndex + 1);
                        }
                    }

                    // Add layout(set, bindings) qualifier to all other uniforms
                    foreach (var variable in glslShader.Declarations.OfType<Variable>().Where(x => (x.Qualifiers.Contains(StorageQualifier.Uniform))))
                    {
                        var layoutBindingIndex = bindings.IndexOf(x => variable.Name.Text.StartsWith(x.Key.RawName));

                        if (layoutBindingIndex != -1)
                        {
                            var layoutQualifier = variable.Qualifiers.OfType<SiliconStudio.Shaders.Ast.Glsl.LayoutQualifier>().FirstOrDefault();
                            if (layoutQualifier == null)
                            {
                                layoutQualifier = new SiliconStudio.Shaders.Ast.Glsl.LayoutQualifier();
                                variable.Qualifiers |= layoutQualifier;
                            }

                            //layoutQualifier.Layouts.Add(new LayoutKeyValue("set", resourceGroups.IndexOf(resourceGroup)));
                            layoutQualifier.Layouts.Add(new LayoutKeyValue("set", 0));
                            layoutQualifier.Layouts.Add(new LayoutKeyValue("binding", layoutBindingIndex + 1));

                            resourceBindings.Add(bindings[layoutBindingIndex].Key.KeyName, layoutBindingIndex + 1);
                        }
                    }
                }

            }

            // Output the result
            var glslShaderWriter = new HlslToGlslWriter(shaderPlatform, shaderVersion, pipelineStage);

            if (shaderPlatform == GlslShaderPlatform.OpenGLES && shaderVersion < 320)
            {
                glslShaderWriter.ExtraHeaders = "#define texelFetchBufferPlaceholder";
            }

            // Write shader
            glslShaderWriter.Visit(glslShader);

            var shaderString = glslShaderWriter.Text;

            // Build shader source
            var glslShaderCode = new StringBuilder();

            // Append some header depending on target
            //if (isOpenGLES)
            //{
            //    if (isOpenGLES3)
            //    {
            //        glslShaderCode
            //            .AppendLine("#version 300 es") // TODO: 310 version?
            //            .AppendLine();
            //    }
            //
            //    if (pipelineStage == PipelineStage.Pixel)
            //        glslShaderCode
            //            .AppendLine("precision highp float;")
            //            .AppendLine();
            //}
            //else
            //{
            //    glslShaderCode
            //        .AppendLine("#version 420")
            //        .AppendLine()
            //        .AppendLine("#define samplerBuffer sampler2D")
            //        .AppendLine("#define isamplerBuffer isampler2D")
            //        .AppendLine("#define usamplerBuffer usampler2D")
            //        .AppendLine("#define texelFetchBuffer(sampler, P) texelFetch(sampler, ivec2((P) & 0xFFF, (P) >> 12), 0)");
            //        //.AppendLine("#define texelFetchBuffer(sampler, P) texelFetch(sampler, P)");
            //}

            glslShaderCode.Append(shaderString);

            var realShaderSource = glslShaderCode.ToString();

// TODO: Disabled because glsl optimizer don't properly put lowp/highp qualifier inside struct (which make shader compilation fails since we use struct for lighting)
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
            // optimize shader
            try
            {
                var optShaderSource = RunOptimizer(shaderBytecodeResult, realShaderSource, shaderPlatform, shaderVersion, pipelineStage == PipelineStage.Vertex);
                if (!String.IsNullOrEmpty(optShaderSource))
                    realShaderSource = optShaderSource;
            }
            catch (Exception e)
            {
                shaderBytecodeResult.Warning("Could not run GLSL optimizer:\n{0}", e.Message);
            }
#else
            shaderBytecodeResult.Warning("GLSL optimized has not been executed because it is currently not supported on this platform.");
#endif

            return realShaderSource;
        }

        private static void MarkResourceBindingAsUsed(EffectReflection reflection, int resourceBindingIndex, ShaderStage stage)
        {
            var resourceBinding = reflection.ResourceBindings[resourceBindingIndex];
            if (resourceBinding.Stage == ShaderStage.None)
            {
                resourceBinding.Stage = stage;
                reflection.ResourceBindings[resourceBindingIndex] = resourceBinding;
            }
        }

        private static int ComputeMemberSize(ref EffectTypeDescription memberType, ref int constantBufferOffset)
        {
            var elementSize = ComputeTypeSize(memberType.Type);
            int size;
            int alignment;

            switch (memberType.Class)
            {
                case EffectParameterClass.Struct:
                    {
                        // Fill members
                        size = 0;
                        for (int index = 0; index < memberType.Members.Length; index++)
                        {
                            // Properly compute size and offset according to DX rules
                            var memberSize = ComputeMemberSize(ref memberType.Members[index].Type, ref size);

                            // Align offset and store it as member offset
                            memberType.Members[index].Offset = size;

                            // Adjust offset for next item
                            size += memberSize;
                        }

                        alignment = size;
                        break;
                    }
                case EffectParameterClass.Scalar:
                    {
                        size = elementSize;
                        alignment = size;
                        break;
                    }
                case EffectParameterClass.Color:
                case EffectParameterClass.Vector:
                    {
                        size = elementSize * memberType.ColumnCount;
                        alignment = (memberType.ColumnCount == 3 ? 4 : memberType.ColumnCount) * elementSize; // vec3 uses alignment of vec4
                        break;
                    }
                case EffectParameterClass.MatrixColumns:
                    {
                        size = elementSize * 4 * memberType.ColumnCount;
                        alignment = size;
                        break;
                    }
                case EffectParameterClass.MatrixRows:
                    {
                        size = elementSize * 4 * memberType.RowCount;
                        alignment = size;
                        break;
                    }
                default:
                    throw new NotImplementedException();
            }

            // Update element size
            memberType.ElementSize = size;

            // Array
            if (memberType.Elements > 0)
            {
                var roundedSize = (size + 15) / 16 * 16; // Round up to vec4
                size = roundedSize * memberType.Elements;
                alignment = roundedSize * memberType.Elements;
            }

            // Alignment is maxed up to vec4
            if (alignment > 16)
                alignment = 16;

            // Align offset and store it as member offset
            constantBufferOffset = (constantBufferOffset + alignment - 1) / alignment * alignment;

            return size;
        }

        private static int ComputeTypeSize(EffectParameterType type)
        {
            switch (type)
            {
                case EffectParameterType.Bool:
                case EffectParameterType.Float:
                case EffectParameterType.Int:
                case EffectParameterType.UInt:
                    return 4;
                case EffectParameterType.Double:
                    return 8;
                case EffectParameterType.Void:
                    return 0;
                default:
                    throw new NotImplementedException();
            }
        }

        private string RunOptimizer(ShaderBytecodeResult shaderBytecodeResult, string baseShader, GlslShaderPlatform shaderPlatform, int shaderVersion, bool vertex)
	    {
            lock (GlslOptimizerLock)
            {
                IntPtr ctx = IntPtr.Zero;
                var inputShader = baseShader;
                if (shaderPlatform == GlslShaderPlatform.OpenGLES)
                {
                    if (shaderVersion >= 300)
                        ctx = glslopt_initialize(2); // kGlslTargetOpenGLES30
                    else
                        ctx = glslopt_initialize(1); // kGlslTargetOpenGLES20
                }
                else
                {
                    ctx = glslopt_initialize(0); // kGlslTargetOpenGL
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
                else
                {
                    IntPtr log = glslopt_get_log(shader);
                    var logAsString = Marshal.PtrToStringAnsi(log);
                    shaderBytecodeResult.Warning("Could not run GLSL optimizer:\n    glsl_opt: {0}", string.Join("\r\n    glsl_opt: ", logAsString.Split(new[] { "\n", "\r", "\r\n" }, StringSplitOptions.RemoveEmptyEntries)));
                }

                glslopt_shader_delete(shader);
                glslopt_cleanup(ctx);

                return shaderAsString;
            }
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
        private static extern IntPtr glslopt_get_log(IntPtr shader);

        [DllImport("glsl_optimizer.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void glslopt_cleanup(IntPtr ctx);
    }
}
