// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.IO;
using System.Linq;

using SiliconStudio.Assets;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;
using SiliconStudio.Paradox.Assets.Effect;

namespace SiliconStudio.Paradox.Assets
{
    [PackageUpgrader("Paradox", "1.0.0-beta01", "1.3.0-beta")]
    public class ParadoxPackageUpgrader : PackageUpgrader
    {
        public override bool Upgrade(PackageSession session, ILogger log, Package dependentPackage, PackageDependency dependency, Package dependencyPackage, IList<PackageLoadingAssetFile> assetFiles)
        {
            // Paradox 1.1 projects didn't have their dependency properly updated (they might have been marked as 1.0).
            // We know they are 1.1 only because there is a .props file.
            // This check shouldn't be necessary from 1.2.
            var packagePath = dependentPackage.FullPath;
            var propsFilePath = UPath.Combine(packagePath.GetParent(), (UFile)(packagePath.GetFileName() + ".props"));
            if (!File.Exists(propsFilePath) && dependency.Version.MinVersion < new PackageVersion("1.1.0-beta"))
            {
                log.Error("Can't upgrade old projects from {0} 1.0 to 1.1", dependency.Name);
                return false;
            }

            // Nothing to do for now, most of the work is already done by individual asset upgraders
            // We can later add logic here for package-wide upgrades (i.e. GameSettingsAsset)
            if (dependency.Version.MinVersion < new PackageVersion("1.2.0-beta"))
            {
                // UIImageGroups and SpriteGroups asset have been merged into a single SpriteSheet => rename the assets and modify the tag
                var uiImageGroups = assetFiles.Where(f => f.FilePath.GetFileExtension() == ".pdxuiimage");
                var spritesGroups = assetFiles.Where(f => f.FilePath.GetFileExtension() == ".pdxsprite");
                RenameAndChangeTag(assetFiles, uiImageGroups, "!UIImageGroup");
                RenameAndChangeTag(assetFiles, spritesGroups, "!SpriteGroup");
            }

            if (dependency.Version.MinVersion < new PackageVersion("1.3.0-alpha01"))
            {
                // Create GameSettingsAsset
                GameSettingsAsset.UpgraderVersion130.Upgrade(session, log, dependentPackage, dependency, dependencyPackage, assetFiles);
            }

            if (dependency.Version.MinVersion < new PackageVersion("1.3.0-alpha02"))
            {
                // Delete EffectLogAsset
                foreach (var assetFile in assetFiles)
                {
                    if (assetFile.FilePath.GetFileName() == EffectLogAsset.DefaultFile)
                    {
                        assetFile.Deleted = true;
                    }
                }
            }

            return true;
        }

        /// <inheritdoc/>
        public override bool UpgradeAfterAssetsLoaded(PackageSession session, ILogger log, Package dependentPackage, PackageDependency dependency, Package dependencyPackage, PackageVersionRange dependencyVersionBeforeUpdate)
        {
            if (dependencyVersionBeforeUpdate.MinVersion < new PackageVersion("1.3.0-alpha02"))
            {
                // Add everything as root assets (since we don't know what the project was doing in the code before)
                foreach (var assetItem in dependentPackage.Assets)
                {
                    if (!AssetRegistry.IsAssetTypeAlwaysMarkAsRoot(assetItem.Asset.GetType()))
                        dependentPackage.RootAssets.Add(new AssetReference<Asset>(assetItem.Id, assetItem.Location));
                }
            }

            return true;
        }

        private void ChangeFileExtension(IList<PackageLoadingAssetFile> assetFiles, PackageLoadingAssetFile file, string newExtension)
        {
            // Create the new file
            var newFileName = new UFile(file.FilePath.FullPath.Replace(file.FilePath.GetFileExtension(), newExtension));
            var newFile = new PackageLoadingAssetFile(newFileName, file.SourceFolder) { AssetContent = file.AssetContent };

            // Add the new file
            assetFiles.Add(newFile);

            // Mark the old file as "To Delete"
            file.Deleted = true;
        }

        private void RenameAndChangeTag( IList<PackageLoadingAssetFile> assetFiles, IEnumerable<PackageLoadingAssetFile> groupFiles, string oldTag)
        {
            var oldTagLength = System.Text.Encoding.UTF8.GetBytes(oldTag).Length;
            var newTagBuffer = System.Text.Encoding.UTF8.GetBytes("!SpriteSheet");
            
            foreach (var file in groupFiles.ToArray())
            {
                // set the content of the new asset (replace the tags)
                using (var stream = new FileStream(file.FilePath.FullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    file.AssetContent = new byte[stream.Length + newTagBuffer.Length - oldTagLength];
                    using (var memoryStream = new MemoryStream(file.AssetContent))
                    {
                        memoryStream.Write(newTagBuffer, 0, newTagBuffer.Length);
                        stream.Position = oldTagLength;
                        stream.CopyTo(memoryStream);
                    }
                }

                // rename the file
                ChangeFileExtension(assetFiles, file, ".pdxsheet");
            }
        }
    }
}