// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Assets.Analysis;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Assets.Compiler
{
    /// <summary>
    /// Main interface for compiling an <see cref="Asset"/>.
    /// </summary>
    public interface IAssetCompiler
    {
        /// <summary>
        /// Compiles a list of assets from the specified package.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="assetItem">The asset reference.</param>
        /// <returns>The result of the compilation.</returns>
        AssetCompilerResult Prepare(AssetCompilerContext context, AssetItem assetItem);

        IEnumerable<ObjectUrl> GetInputFiles(AssetItem assetItem);

        IEnumerable<KeyValuePair<Type, BuildDependencyType>> GetInputTypes(AssetCompilerContext context, AssetItem assetItem);

        IEnumerable<Type> GetTypesToFilterOut(AssetCompilerContext context, AssetItem assetItem);
    }
}