// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Linq;

using SiliconStudio.Assets;
using SiliconStudio.Assets.Analysis;
using SiliconStudio.Assets.Diagnostics;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Xenko.Assets.Entities;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.Analysis
{
    /// <summary>
    /// Analyses a game package, checks the default scene exists.
    /// </summary>
    public sealed class GamePackageSessionAnalysis : PackageSessionAnalysisBase
    {
        /// <summary>
        /// Checks if a default scene exists for this game package.
        /// </summary>
        /// <param name="log">The log to output the result of the validation.</param>
        public override void Run(ILogger log)
        {
            if (log == null) throw new ArgumentNullException(nameof(log));

            foreach (var package in Session.Packages)
            {
                // Make sure package has its assets loaded
                if (package.State < PackageState.AssetsReady)
                    continue;

                var hasGameExecutable = package.Profiles.SelectMany(profile => profile.ProjectReferences).Any(projectRef => projectRef.Type == ProjectType.Executable);
                if (!hasGameExecutable)
                {
                    continue;
                }

                // Find game settings
                var gameSettingsAssetItem = package.Assets.Find(GameSettingsAsset.GameSettingsLocation);
                AssetItem defaultScene = null;

                // If game settings is found, try to find default scene inside
                var defaultSceneRuntime = ((GameSettingsAsset)gameSettingsAssetItem?.Asset)?.DefaultScene;
                var defaultSceneReference = AttachedReferenceManager.GetAttachedReference(defaultSceneRuntime);
                if (defaultSceneReference != null)
                {
                    // Find it either by Url or Id
                    defaultScene = package.Assets.Find(defaultSceneReference.Id) ?? package.Assets.Find(defaultSceneReference.Url);

                    // Check it is actually a scene asset
                    if (defaultScene != null && !(defaultScene.Asset is SceneAsset))
                        defaultScene = null;
                }

                // Find or create default scene
                if (defaultScene == null)
                {
                    defaultScene = package.Assets.Find(GameSettingsAsset.DefaultSceneLocation);
                    if (defaultScene != null && !(defaultScene.Asset is SceneAsset))
                        defaultScene = null;
                }

                // Otherwise, try to find any scene
                if (defaultScene == null)
                    defaultScene = package.Assets.FirstOrDefault(x => x.Asset is SceneAsset);

                // Nothing found, let's create an empty one
                if (defaultScene == null)
                {
                    log.Error(package, null, AssetMessageCode.DefaultSceneNotFound, null);

                    var defaultSceneName = NamingHelper.ComputeNewName(GameSettingsAsset.DefaultSceneLocation, package.Assets, a => a.Location);
                    var defaultSceneAsset = DefaultAssetFactory<SceneAsset>.Create();

                    defaultScene = new AssetItem(defaultSceneName, defaultSceneAsset);
                    package.Assets.Add(defaultScene);
                    defaultScene.IsDirty = true;
                }

                // Create game settings if not done yet
                if (gameSettingsAssetItem == null)
                {
                    log.Error(package, null, AssetMessageCode.AssetNotFound, GameSettingsAsset.GameSettingsLocation);

                    var gameSettingsAsset = GameSettingsFactory.Create();

                    gameSettingsAsset.DefaultScene = AttachedReferenceManager.CreateProxyObject<Scene>(defaultScene.Id, defaultScene.Location);

                    gameSettingsAssetItem = new AssetItem(GameSettingsAsset.GameSettingsLocation, gameSettingsAsset);
                    package.Assets.Add(gameSettingsAssetItem);

                    gameSettingsAssetItem.IsDirty = true;
                }
            }
        }
   }
}
