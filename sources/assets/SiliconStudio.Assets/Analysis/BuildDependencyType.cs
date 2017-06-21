// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

namespace SiliconStudio.Assets.Analysis
{
    [Flags]
    public enum BuildDependencyType
    {
        /// <summary>
        /// The content compiled from target asset will be needed when loading the compiled content of the referencing asset at runtime.
        /// </summary>
        Runtime = 0x1,
        /// <summary>
        /// The target asset is needed uncompiled when compiling the referencing asset.
        /// </summary>
        CompileAsset = 0x2,
        /// <summary>
        /// The content compiled from target asset is needed when compiling the referencing asset.
        /// </summary>
        CompileContent = 0x4
    }
}
