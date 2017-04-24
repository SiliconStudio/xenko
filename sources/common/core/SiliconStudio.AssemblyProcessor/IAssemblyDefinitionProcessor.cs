// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using Mono.Cecil;

namespace SiliconStudio.AssemblyProcessor
{
    internal interface IAssemblyDefinitionProcessor
    {
        bool Process(AssemblyProcessorContext context);
    }
}
