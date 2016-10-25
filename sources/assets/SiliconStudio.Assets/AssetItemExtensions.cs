// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.IO;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Assets
{
    public static class AssetItemExtensions
    {
        /// <summary>
        /// Gets the asset filename relative to its .csproj file for <see cref="IProjectAsset"/>.
        /// </summary>
        /// <param name="assetItem">The asset item.</param>
        /// <returns></returns>
        public static string GetProjectInclude(this AssetItem assetItem)
        {
            var assetFullPath = assetItem.FullPath;
            var projectFullPath = assetItem.SourceProject;
            return assetFullPath.MakeRelative(projectFullPath.GetFullDirectory()).ToWindowsPath();
        }

        /// <summary>
        /// If the asset is a <see cref="IProjectFileGeneratorAsset"/>, gets the generated file full path.
        /// </summary>
        /// <param name="assetItem">The asset item.</param>
        /// <returns></returns>
        public static UFile GetGeneratedAbsolutePath(this AssetItem assetItem)
        {
            return new UFile(new UFile(assetItem.FullPath).GetFullPathWithoutExtension() + ".cs");
        }

        /// <summary>
        /// If the asset is a <see cref="IProjectFileGeneratorAsset"/>, gets the generated file path relative to its containing .csproj.
        /// </summary>
        /// <param name="assetItem">The asset item.</param>
        /// <returns></returns>
        public static string GetGeneratedInclude(this AssetItem assetItem)
        {
            return Path.ChangeExtension(GetProjectInclude(assetItem), ".cs");
        }
    }
}