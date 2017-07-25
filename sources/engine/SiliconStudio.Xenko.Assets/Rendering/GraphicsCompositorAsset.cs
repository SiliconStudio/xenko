// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Core.Yaml.Serialization;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Compositing;

namespace SiliconStudio.Xenko.Assets.Rendering
{
    // TODO: this list is here just to easily identify object references. It can be removed once we have access to the path from IsObjectReference
    [DataContract]
    public class RenderStageCollection : List<RenderStage>
    {
    }

    // TODO: this list is here just to easily identify object references. It can be removed once we have access to the path from IsObjectReference
    [DataContract]
    public class SharedRendererCollection : TrackingCollection<ISharedRenderer>
    {
    }

    [DataContract("GraphicsCompositorAsset")]
    [Display(8000, "Graphics compositor")]
    [AssetContentType(typeof(GraphicsCompositor))]
    [AssetDescription(FileExtension)]
#if SILICONSTUDIO_XENKO_SUPPORT_BETA_UPGRADE
    [AssetFormatVersion(XenkoConfig.PackageName, CurrentVersion)]
    [AssetUpgrader(XenkoConfig.PackageName, "0.0.0", "1.10.0-beta01", typeof(AssetComposite.FixPartReferenceUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, "1.10.0-beta01", "2.0.0.0", typeof(EmptyAssetUpgrader))]
#else
    [AssetFormatVersion(XenkoConfig.PackageName, CurrentVersion, "2.0.0.0")]
#endif
    [AssetUpgrader(XenkoConfig.PackageName, "2.0.0.0", "2.1.0.2", typeof(FXAAEffectDefaultQualityUpgrader))]
    public class GraphicsCompositorAsset : Asset
    {
        private const string CurrentVersion = "2.1.0.2";

        /// <summary>
        /// The default file extension used by the <see cref="GraphicsCompositorAsset"/>.
        /// </summary>
        public const string FileExtension = ".xkgfxcomp";

        /// <summary>
        /// Gets the cameras used by this composition.
        /// </summary>
        /// <value>The cameras.</value>
        /// <userdoc>The list of cameras used in the graphic pipeline</userdoc>
        [Category("Camera Slots")]
        [MemberCollection(CanReorderItems = true, NotNullItems = true)]
        public SceneCameraSlotCollection Cameras { get; } = new SceneCameraSlotCollection();

        /// <summary>
        /// The list of render stages.
        /// </summary>
        [Category]
        [MemberCollection(CanReorderItems = true, NotNullItems = true)]
        public RenderStageCollection RenderStages { get; } = new RenderStageCollection();

        /// <summary>
        /// The list of render features.
        /// </summary>
        [Category]
        [MemberCollection(CanReorderItems = true, NotNullItems = true)]
        public List<RootRenderFeature> RenderFeatures { get; } = new List<RootRenderFeature>();

        /// <summary>
        /// The list of graphics compositors.
        /// </summary>
        [Category]
        [MemberCollection(CanReorderItems = true, NotNullItems = true)]
        public SharedRendererCollection SharedRenderers { get; } = new SharedRendererCollection();

        /// <summary>
        /// The entry point for the game compositor.
        /// </summary>
        /// <userdoc>
        /// The renderer used by the game at runtime. It requires a properly set camera from the scene, found in the Cameras list.
        /// </userdoc>
        [Display("Game renderer")]
        public ISceneRenderer Game { get; set; }

        /// <summary>
        /// The entry point for a compositor that can render a single view.
        /// </summary>
        /// <userdoc>
        /// The utility renderer is used for rendering cubemaps, light maps, render-to-texture, etc. It should be a single-only view renderer with no post-processing. It doesn't require camera or render target, because they are supplied by the caller.
        /// </userdoc>
        [Display("Utility renderer")]
        public ISceneRenderer SingleView { get; set; }

        /// <summary>
        /// The entry point for a compositor used by the scene editor.
        /// </summary>
        /// <userdoc>
        /// The renderer used by the game studio while editing the scene. It can share the forward renderer with the game entry or not. It doesn't require a camera and uses the camera in the game studio instead.
        /// </userdoc>
        [Display("Editor renderer")]
        public ISceneRenderer Editor { get; set; }

        /// <summary>
        /// The positions of the blocks of the compositor in the editor canvas.
        /// </summary>
        [Display(Browsable = false)]
        public Dictionary<Guid, Vector2> BlockPositions { get; } = new Dictionary<Guid, Vector2>();

        public GraphicsCompositor Compile(bool copyRenderers)
        {
            var graphicsCompositor = new GraphicsCompositor();

            foreach (var cameraSlot in Cameras)
                graphicsCompositor.Cameras.Add(cameraSlot);
            foreach (var renderStage in RenderStages)
                graphicsCompositor.RenderStages.Add(renderStage);
            foreach (var renderFeature in RenderFeatures)
                graphicsCompositor.RenderFeatures.Add(renderFeature);

            if (copyRenderers)
            {
                graphicsCompositor.Game = Game;
                graphicsCompositor.SingleView = SingleView;
            }

            return graphicsCompositor;
        }

        private class FXAAEffectDefaultQualityUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
            {
                var rootNode = (YamlNode)asset.Node;

                foreach (var fxaaEffectNode in rootNode.AllNodes.OfType<YamlMappingNode>().Where(x => x.Tag == "!FXAAEffect").Select(x => new DynamicYamlMapping(x)))
                {
                    // We could remap quality but probably not worth the code complexity (esp. since previous quality slider from 10 to 39 was not "continuous", user probably didn't set it up properly anyway)
                    // Simply remove it so that it goes back to default value
                    fxaaEffectNode.RemoveChild("Quality");
                }
            }
        }
    }
}
