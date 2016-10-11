// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.IO;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Assets
{
    public static class AssetItemExtensions
    {
        public static string GetProjectInclude(this AssetItem assetItem)
        {
            var assetFullPath = assetItem.FullPath;
            var projectFullPath = assetItem.SourceProject;
            return assetFullPath.MakeRelative(projectFullPath.GetFullDirectory()).ToWindowsPath();
        }

        public static UFile GetGeneratedAbsolutePath(this AssetItem assetItem)
        {
            return new UFile(new UFile(assetItem.FullPath).GetFullPathWithoutExtension() + ".cs");
        }

        public static string GetGeneratedInclude(this AssetItem assetItem)
        {
            return Path.ChangeExtension(GetProjectInclude(assetItem), ".cs");
        }
    }
}