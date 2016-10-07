// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.IO;
using System.Linq;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Assets
{
    public static class AssetItemExtensions
    {
        public static UFile FindSourceProject(this AssetItem asset)
        {
            var projectAsset = asset.Asset as IProjectAsset;
            if (projectAsset != null)
            {
                var profile = asset.Package.Profiles.FindSharedProfile();

                var lib = profile?.ProjectReferences.FirstOrDefault(x => x.Type == ProjectType.Library && asset.Location.FullPath.StartsWith(x.Location.GetFileName()));
                if (lib == null)
                    return null;

                return UPath.Combine(asset.Package.RootDirectory, lib.Location);
            }

            return null;
        }

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