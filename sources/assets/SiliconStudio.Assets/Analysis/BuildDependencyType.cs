// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using SiliconStudio.Assets.Compiler;

namespace SiliconStudio.Assets.Analysis
{
    [Flags]
    public enum BuildDependencyType
    {
        /// <summary>
        /// The content generated during compilation needs the content compiled from the target asset to be loaded at runtime.
        /// </summary>
        Runtime = 0x1,
        /// <summary>
        /// The uncompiled target asset is accessed during compilation.
        /// </summary>
        CompileAsset = 0x2,
        /// <summary>
        /// The content compiled from the target asset is needed during compilation.
        /// </summary>
        CompileContent = 0x4
    }
}
