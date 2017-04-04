// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Assets.Analysis;
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

        /// <summary>
        /// Enumerates all the dependencies required to compile this asset
        /// </summary>
        /// <param name="context">The asset compiler context</param>
        /// <param name="assetItem">The asset for which dependencies are enumerated</param>
        /// <returns>The dependencies</returns>
        IEnumerable<ObjectUrl> GetInputFiles(AssetCompilerContext context, AssetItem assetItem);

        /// <summary>
        /// Enumerates all the asset types required to compile this asset
        /// </summary>
        /// <param name="context">The asset compiler context</param>
        /// <param name="assetItem">The asset for which types are enumerated</param>
        /// <returns>The dependencies</returns>
        IEnumerable<KeyValuePair<Type, BuildDependencyType>> GetInputTypes(AssetCompilerContext context, AssetItem assetItem);

        /// <summary>
        /// Enumerates all the asset types to exclude when compiling this asset
        /// </summary>
        /// <param name="context">The asset compiler context</param>
        /// <param name="assetItem">The asset for which types are enumerated</param>
        /// <returns>The types to exclude</returns>
        /// <remarks>This method takes higher priority, it will exclude assets included with inclusion methods even in the same compiler</remarks>
        IEnumerable<Type> GetInputTypesToExclude(AssetCompilerContext context, AssetItem assetItem);

        /// <summary>
        /// A boolean indicating if this asset can be skipped in a build chain if its preparation has failed.
        /// </summary>
        bool CanBeSkipped { get; }
    }
}