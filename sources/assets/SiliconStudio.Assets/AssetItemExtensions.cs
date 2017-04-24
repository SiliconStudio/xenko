// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.IO;
using SiliconStudio.Core.Annotations;
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
        public static string GetProjectInclude([NotNull] this AssetItem assetItem)
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
        [NotNull]
        public static UFile GetGeneratedAbsolutePath([NotNull] this AssetItem assetItem)
        {
            return new UFile(new UFile(assetItem.FullPath).GetFullPathWithoutExtension() + ".cs");
        }

        /// <summary>
        /// If the asset is a <see cref="IProjectFileGeneratorAsset"/>, gets the generated file path relative to its containing .csproj.
        /// </summary>
        /// <param name="assetItem">The asset item.</param>
        /// <returns></returns>
        public static string GetGeneratedInclude([NotNull] this AssetItem assetItem)
        {
            return Path.ChangeExtension(GetProjectInclude(assetItem), ".cs");
        }
    }
}
