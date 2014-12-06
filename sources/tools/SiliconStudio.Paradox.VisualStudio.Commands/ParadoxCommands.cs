// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextTemplating.VSHost;

using SiliconStudio.Paradox.Shaders.Navigation;
using SiliconStudio.Paradox.VisualStudio.BuildEngine;
using SiliconStudio.Paradox.VisualStudio.Commands.Shaders;
using SiliconStudio.Paradox.VisualStudio.DataGenerator;
using SiliconStudio.Paradox.VisualStudio.Shaders;
using SiliconStudio.Shaders.Ast;

using SourceLocation = NShader.SourceLocation;

namespace SiliconStudio.Paradox.VisualStudio.Commands
{
    public class ParadoxCommands : IParadoxCommands
    {
        public bool ShouldReload()
        {
            // This is implemented in the proxy only
            throw new NotImplementedException();
        }

        public void StartRemoteBuildLogServer(IBuildMonitorCallback buildMonitorCallback, string logPipeUrl)
        {
            new PackageBuildMonitorRemote(buildMonitorCallback, logPipeUrl);
        }

        public byte[] GenerateShaderKeys(string inputFileName, string inputFileContent)
        {
            return ShaderKeyFileHelper.GenerateCode(inputFileName, inputFileContent);
        }

        public byte[] GenerateDataClasses(string assemblyOutput, string projectFullName, string intermediateAssembly)
        {
            return DataCodeGeneratorHelper.GenerateSource(assemblyOutput, projectFullName, intermediateAssembly);
        }

        public SourceLocation GoToDefinition(string sourceCode, SourceLocation location)
        {
            var navigation = new ShaderNavigation();
            var spanResult = navigation.FindDeclaration(null, sourceCode, new SiliconStudio.Shaders.Ast.SourceLocation(location.File, 0, location.Line, location.Column));
            if (spanResult.HasValue)
            {
                var span = spanResult.Value;

                return new SourceLocation()
                {
                    File = span.Location.FileSource,
                    Line = span.Location.Line,
                    EndLine = span.Location.Line,
                    Column = span.Location.Column,
                    EndColumn = span.Location.Column + span.Length
                };
            }

            return new SourceLocation();
        }
    }
}