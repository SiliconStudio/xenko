// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using Mono.Cecil;

namespace SiliconStudio.AssemblyProcessor
{
    internal interface IAssemblyDefinitionProcessor
    {
        bool Process(AssemblyProcessorContext context);
    }
}
