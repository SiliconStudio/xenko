// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Shaders.Analysis.Hlsl;
using SiliconStudio.Shaders.Convertor;
using SiliconStudio.Shaders.Parser;
using SiliconStudio.Shaders.Parser.Hlsl;
using ShaderMacro = SiliconStudio.Xenko.Shaders.ShaderMacro;

namespace SiliconStudio.Xenko.Shaders.Compiler.OpenGL
{
    /// <summary>
    /// Converts from HLSL shader sourcecode to a GLSL sourcecode.
    /// </summary>
    internal class ShaderConverter
    {
        private bool isOpenGLES;

        private bool isOpenGLES3;

        private bool isVulkan;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderConverter"/> class.
        /// </summary>
        public ShaderConverter(bool isOpenGLES, bool isOpenGLES3, bool isVulkan)
        {
            this.isOpenGLES = isOpenGLES;
            this.isOpenGLES3 = isOpenGLES3;
            this.isVulkan = isVulkan;
            Log = Console.Out;
            IsVerboseLog = true;
            Macros = new List<ShaderMacro>();
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is producing a verbose log.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is producing a verbose log; otherwise, <c>false</c>.
        /// </value>
        public bool IsVerboseLog { get; set; }

        /// <summary>
        /// Gets or sets the include directories.
        /// </summary>
        /// <value>
        /// The include directories.
        /// </value>
        public string[] IncludeDirectories { get; set; }


        /// <summary>
        /// Gets or sets the macros.
        /// </summary>
        /// <value>
        /// The macros.
        /// </value>
        public List<ShaderMacro> Macros { get; set; }

        /// <summary>
        /// Gets or sets the log used by this instance.
        /// </summary>
        /// <value>
        /// The log used by this instance.
        /// </value>
        public TextWriter Log { get; set; }

        /// <summary>
        /// Converts the specified hlsl source code to glsl.
        /// </summary>
        /// <param name="hlslSourcecode">The HLSL source code.</param>
        /// <param name="hlslEntryPoint">The HLSL entry point.</param>
        /// <param name="stage">The stage to convert.</param>
        /// <param name="shader">The shader.</param>
        /// <param name="inputHlslFilepath">The input HLSL filepath.</param>
        /// <returns>
        /// The resulting glsl AST tree.
        /// </returns>
        public global::SiliconStudio.Shaders.Ast.Shader Convert(string hlslSourcecode, string hlslEntryPoint, PipelineStage stage, string inputHlslFilepath, LoggerResult log)
        {
            try
            {
                // Convert from Framework.Graphics ShaderMacro to Framework.Shaders ShaderMacro
                var macros = new global::SiliconStudio.Shaders.Parser.ShaderMacro[Macros.Count];
                for (int index = 0; index < Macros.Count; index++)
                    macros[index] = new global::SiliconStudio.Shaders.Parser.ShaderMacro(Macros[index].Name, Macros[index].Definition);

                var result = HlslParser.TryPreProcessAndParse(hlslSourcecode, inputHlslFilepath, macros, IncludeDirectories);

                if (result.HasErrors)
                {
                    log.Error(result.ToString());
                    return null;
                }

                // Prepare the shader before type inference analysis
                HlslToGlslConvertor.Prepare(result.Shader);

                HlslSemanticAnalysis.Run(result);

                // If there are any type inference analysis, just display all errors but ytu
                if (result.HasErrors)
                {
                    log.Error(result.ToString());
                    return null;
                }

                return Convert(result, hlslEntryPoint, stage, inputHlslFilepath, log);
            }
            catch (Exception ex)
            {
                log.Error("Unexpected error while converting file [{0}] with entry point [{1}]", ex, inputHlslFilepath, hlslEntryPoint);
            }
            return null;
        }

        /// <summary>
        /// Converts the specified hlsl source code to glsl.
        /// </summary>
        /// <param name="hlslEntryPoint">The HLSL entry point.</param>
        /// <param name="stage">The stage to convert.</param>
        /// <param name="shader">The shader.</param>
        /// <param name="inputHlslFilepath">The input HLSL filepath.</param>
        /// <returns>
        /// The resulting glsl AST tree.
        /// </returns>
        private global::SiliconStudio.Shaders.Ast.Shader Convert(ParsingResult result, string hlslEntryPoint, PipelineStage stage, string inputHlslFilepath, LoggerResult log)
        {
            try
            {
                var convertor = new HlslToGlslConvertor(hlslEntryPoint, stage, ShaderModel.Model40) // TODO HARDCODED VALUE to change
                {
                    KeepConstantBuffer = !isOpenGLES || isOpenGLES3,
                    TextureFunctionsCompatibilityProfile = isOpenGLES && !isOpenGLES3,
                    NoSwapForBinaryMatrixOperation = true,
                    UseBindingLayout = false,
                    UseLocationLayout = false,
                    UseSemanticForVariable = true,
                    IsPointSpriteShader = false,
                    ViewFrustumRemap = true,
                    FlipRenderTarget = !isVulkan,
                    KeepNonUniformArrayInitializers = !isOpenGLES,
                    IsOpenGLES2 = isOpenGLES && !isOpenGLES3
                };
                convertor.Run(result);

                // After the converter we display the errors but we don't stop writing output glsl
                if (result.HasErrors)
                {
                    //DisplayError(log, result, "Error while converting file:");
                }


                return result.Shader;
            }
            catch (Exception ex)
            {
                log.Error("Unexpected error while converting file [{0}] with entry point [{1}]", ex, inputHlslFilepath, hlslEntryPoint);
                return null;
            }
        }
    }
}
