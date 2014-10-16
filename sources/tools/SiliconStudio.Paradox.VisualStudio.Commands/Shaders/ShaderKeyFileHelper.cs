// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using SiliconStudio.Paradox.Shaders.Parser;
using SiliconStudio.Paradox.Shaders.Parser.Ast;
using SiliconStudio.Paradox.Shaders.Parser.Mixins;
using SiliconStudio.Shaders.Utility;

namespace SiliconStudio.Paradox.VisualStudio.Commands.Shaders
{
    class ShaderKeyFileHelper
    {
        public static byte[] GenerateCode(string inputFileName, string inputFileContent)
        {
            // Compile
            var shader = ParadoxShaderParser.PreProcessAndParse(inputFileContent, inputFileName);
            var shaderClass = shader.Declarations.OfType<ShaderClassType>().FirstOrDefault();

            ShaderKeyGeneratorBase shaderKeyGenerator;

            if (shaderClass != null)
            {
                shaderKeyGenerator = new ShaderKeyGenerator(shaderClass);
            }
            else
            {
                // Try to generate a mixin code.
                var loggerResult = new LoggerResult();
                shaderKeyGenerator = new ShaderMixinCodeGen(shader, loggerResult);
            }

            if (!shaderKeyGenerator.Run())
            {
                throw new InvalidOperationException(string.Format("Unable to parse shader class from {0}", inputFileName));
            }

            return Encoding.ASCII.GetBytes(shaderKeyGenerator.Text);
        }
    }
}