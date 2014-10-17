// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using Mono.Cecil;

using SiliconStudio.Paradox.VisualStudio.Commands.DataGenerator;

namespace SiliconStudio.Paradox.VisualStudio.DataGenerator
{
    static class DataCodeGeneratorHelper
    {
        public static byte[] GenerateSource(string assemblyOutput, string projectFullName, string intermediateAssembly)
        {
            // Create assembly resolver with original assembly path
            var assemblyResolver = new DefaultAssemblyResolver();
            assemblyResolver.AddSearchDirectory(Path.GetDirectoryName(assemblyOutput));

            // Load assembly
            var intermediateAssemblyFullPath = Path.Combine(Path.GetDirectoryName(projectFullName), intermediateAssembly);
            var assembly = AssemblyDefinition.ReadAssembly(intermediateAssemblyFullPath,
                new ReaderParameters { AssemblyResolver = assemblyResolver, ReadSymbols = false });
            var dataConverterGenerator = new DataConverterGenerator(assemblyResolver, assembly);

            // Generate source
            var sourceCode = dataConverterGenerator.TransformText();
            return Encoding.ASCII.GetBytes(sourceCode);
        }
    }
}