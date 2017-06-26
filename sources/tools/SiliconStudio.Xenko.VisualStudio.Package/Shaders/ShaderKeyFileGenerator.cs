// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
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
