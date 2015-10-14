// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio.Shell;
using SiliconStudio.Xenko.VisualStudio.CodeGenerator;
using SiliconStudio.Xenko.VisualStudio.Commands;

namespace SiliconStudio.Xenko.VisualStudio.Shaders
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    [Guid(GuidList.guidXenko_VisualStudio_ShaderKeyFileGenerator)]
    [ProvideObject(typeof(ShaderKeyFileGenerator), RegisterUsing = RegistrationMethod.CodeBase)]
    public class ShaderKeyFileGenerator : BaseCodeGeneratorWithSite
    {
        public const string DisplayName = "Xenko Shader C# Key Generator";
        public const string InternalName = "XenkoShaderKeyGenerator";

        protected override string GetDefaultExtension()
        {
            return ".cs";
        }

        protected override byte[] GenerateCode(string inputFileName, string inputFileContent)
        {
            try
            {
                var remoteCommands = XenkoCommandsProxy.GetProxy();
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