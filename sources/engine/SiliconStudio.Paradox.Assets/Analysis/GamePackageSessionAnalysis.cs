// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Linq;

using SiliconStudio.Assets;
using SiliconStudio.Assets.Analysis;
using SiliconStudio.Assets.Diagnostics;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Paradox.Assets.Entities;

namespace SiliconStudio.Paradox.Assets.Analysis
{
    /// <summary>
    /// Analyses a game package, checks the default scene exists.
    /// </summary>
    public sealed class GamePackageSessionAnalysis : PackageSessionAnalysisBase
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="GamePackageSessionAnalysis" /> class.
        /// </summary>
        public GamePackageSessionAnalysis()
            : base()
        {
        }

        /// <summary>
        /// Checks if a default scene exists for this game package.
        /// </summary>
        /// <param name="log">The log to output the result of the validation.</param>
        public override void Run(ILogger log)
        {
            if (log == null) throw new ArgumentNullException("log");

            foreach (var package in Session.Packages)
            {
                var hasGameExecutable = package.Profiles.SelectMany(profile => profile.ProjectReferences).Any(projectRef => projectRef.Type == ProjectType.Executable);
                if (!hasGameExecutable)
                {
                    continue;
                }

                var sharedProfile = package.Profiles.FindSharedProfile();
                if (sharedProfile == null) continue;

                var defaultScene = sharedProfile.Properties.Get(GameSettingsAsset.DefaultScene);

                // If the pdxpkg does not reference any scene
                if (defaultScene == null)
                {
                    log.Error(package, null, AssetMessageCode.DefaultSceneNotFound, null);

                    // Creates a new default scene
                    // Checks we don't overwrite an existing asset
                    const string defaultSceneLocation = "MainScene";
                    var existingDefault = package.Assets.Find(defaultSceneLocation);
                    if (existingDefault != null && existingDefault.Asset is SceneAsset)
                    {
                        // A scene at the default location already exists among the assets, let's reference it as the default scene
                        var sceneAsset = new AssetReference<SceneAsset>(existingDefault.Id, existingDefault.Location);
                        GameSettingsAsset.SetDefaultScene(package, sceneAsset);
                    }
                    else if (existingDefault != null)
                    {
                        // Very rare case: the default scene location is occupied by another asset which is not a scene
                        // Compute a new default name to not overwrite the existing asset
                        var newName = NamingHelper.ComputeNewName(defaultSceneLocation, package.Assets, a => a.Location);
                        GameSettingsAsset.CreateAndSetDefaultScene(package, newName);
                    } 
                    else
                    {
                        // Creates a new default scene asset
                        GameSettingsAsset.CreateAndSetDefaultScene(package, defaultSceneLocation);
                    }

                    continue;
                }

                // The pdxpkg references an asset
                var defaultAsset = package.Assets.Find(defaultScene.Location);

                if (defaultAsset != null) continue; // Default scene exists and is referenced

                // The asset referenced does not exist, create it
                log.Error(package, defaultScene, AssetMessageCode.AssetNotFound, defaultScene);
                GameSettingsAsset.CreateAndSetDefaultScene(package);
            }
        }
   }
}