// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Assets.Serializers;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.Entities
{
    /// <summary>
    /// A scene asset.
    /// </summary>
    [DataContract("SceneAsset")]
    [AssetDescription(FileSceneExtension, AllowArchetype = false)]
    [AssetFormatVersion(XenkoConfig.PackageName, CurrentVersion)]
    [AssetCompiler(typeof(SceneAssetCompiler))]
    [AssetUpgrader(XenkoConfig.PackageName, 0, 1, typeof(RemoveSourceUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, 1, 2, typeof(RemoveBaseUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, 2, 3, typeof(RemoveModelDrawOrderUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, 3, 4, typeof(RenameSpriteProviderUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, 4, 5, typeof(RemoveSpriteExtrusionMethodUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, 5, 6, typeof(RemoveModelParametersUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, 6, 7, typeof(RemoveEnabledFromIncompatibleComponent))]
    [AssetUpgrader(XenkoConfig.PackageName, 7, 8, typeof(SceneIsNotEntityUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, 8, 9, typeof(ColliderShapeAssetOnlyUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, 9, 10, typeof(NoBox2DUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, 10, 11, typeof(RemoveShadowImportanceUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, 11, 12, typeof(NewElementLayoutUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, 12, 13, typeof(NewElementLayoutUpgrader2))]
    [AssetUpgrader(XenkoConfig.PackageName, 13, 14, typeof(RemoveGammaTransformUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, 14, 15, typeof(EntityDesignUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, 15, 16, typeof(NewElementLayoutUpgrader3))]
    [AssetUpgrader(XenkoConfig.PackageName, 16, 17, typeof(NewElementLayoutUpgrader4))]
    [AssetUpgrader(XenkoConfig.PackageName, 17, 18, typeof(RemoveSceneEditorCameraSettings))]
    [AssetUpgrader(XenkoConfig.PackageName, "0.0.18", "1.5.0-alpha01", typeof(ChangeSpriteColorTypeAndTriggerElementRemoved))]
    [AssetUpgrader(XenkoConfig.PackageName, "1.5.0-alpha01", "1.5.0-alpha02", typeof(MoveSceneSettingsToSceneAsset))]
    [AssetUpgrader(XenkoConfig.PackageName, "1.5.0-alpha02", "1.6.0-beta", typeof(MigrateToNewComponents))]
    [AssetUpgrader(XenkoConfig.PackageName, "1.6.0-beta", "1.6.0-beta01", typeof(ParticleMinMaxFieldsUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, "1.6.0-beta01", "1.6.0-beta02", typeof(ModelEffectUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, "1.6.0-beta02", "1.6.0-beta03", typeof(PhysicsFiltersUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, "1.6.0-beta03", "1.7.0-beta01", typeof(SpriteComponentUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, "1.7.0-beta01", "1.7.0-beta02", typeof(UIComponentRenamingResolutionUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, "1.7.0-beta02", "1.7.0-beta03", typeof(ParticleColorAnimationUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, "1.7.0-beta03", "1.7.0-beta04", typeof(EntityHierarchyAssetBase.EntityDesignUpgrader))]
    [Display(200, "Scene")]
    [AssetPartReference(typeof(SceneSettings))]
    public partial class SceneAsset : EntityHierarchyAssetBase
    {
        private const string CurrentVersion = "1.7.0-beta04";

        public const string FileSceneExtension = ".xkscene;.pdxscene";

        public SceneAsset()
        {
            SceneSettings = new SceneSettings();
        }

        /// <summary>
        /// Gets the scene settings for this instance.
        /// </summary>
        [DataMember(10)]
        public SceneSettings SceneSettings { get; private set; }
    }
}
