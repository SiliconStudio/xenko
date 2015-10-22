// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio.Shell;
using SiliconStudio.Paradox.VisualStudio.CodeGenerator;
using SiliconStudio.Paradox.VisualStudio.Commands;

namespace SiliconStudio.Paradox.VisualStudio.Shaders
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    [Guid(GuidList.guidParadox_VisualStudio_ShaderKeyFileGenerator)]
    [ProvideObject(typeof(ShaderKeyFileGenerator), RegisterUsing = RegistrationMethod.CodeBase)]
    public class ShaderKeyFileGenerator : BaseCodeGeneratorWithSite
    {
        public const string DisplayName = "Paradox Shader C# Key Generator";
        public const string InternalName = "ParadoxShaderKeyGenerator";

        protected override string GetDefaultExtension()
        {
            return ".cs";
        }

        protected override byte[] GenerateCode(string inputFileName, string inputFileContent)
        {
            try
            {
                var remoteCommands = ParadoxCommandsProxy.GetProxy();
                return remoteCommands.GenerateShaderKeys(inputFileName, inputFileContent);
            }
            catch (Exception ex)
            {
                GeneratorError(4, ex.ToString(), 0, 0);

                return new byte[0];
            }
        }
    }
}