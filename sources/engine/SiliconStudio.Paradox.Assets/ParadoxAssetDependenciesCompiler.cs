// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;

namespace SiliconStudio.Paradox.Assets
{
    public abstract class ParadoxAssetDependenciesCompiler : AssetDependenciesCompiler
    {
        protected void AddGameSettings(AssetItem assetItem, AssetItem originalItem)
        {
            // Copy game settings from the original package to the micro-package of the asset being compiled.
            if (originalItem.Location != GameSettingsAsset.GameSettingsLocation)
            {
                var currentPackage = originalItem.Package.Session.CurrentPackage;
                if (currentPackage != null)
                {
                    var gameSettingsAssetItem = currentPackage.Assets.Find(GameSettingsAsset.GameSettingsLocation);
                    if (gameSettingsAssetItem != null)
                    {
                        var gameSettingsAssetCopy = (GameSettingsAsset)AssetCloner.Clone((GameSettingsAsset)gameSettingsAssetItem.Asset);
                        var newGameSettingsAssetItem = new AssetItem(GameSettingsAsset.GameSettingsLocation, gameSettingsAssetCopy)
                        {
                            SourceFolder = gameSettingsAssetItem.FullPath.GetParent()
                        };
                        assetItem.Package.Assets.Add(newGameSettingsAssetItem);
                    }
                }
            }            
        }
    }
}