using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Assets.Serializers;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Composers;

namespace SiliconStudio.Xenko.Assets.Rendering
{
    [DataContract("GraphicsCompositorAsset")]
    [Display(82, "Graphics Compositor")]
    [AssetContentType(typeof(GraphicsCompositor))]
    [AssetDescription(FileExtension)]
    [AssetCompiler(typeof(GraphicsCompositorAssetCompiler))]
    public class GraphicsCompositorAsset : AssetComposite
    {
        /// <summary>
        /// The default file extension used by the <see cref="GraphicsCompositorAsset"/>.
        /// </summary>
        public const string FileExtension = ".xkgfxcomp";

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsCompositorAsset"/> class.
        /// </summary>
        public GraphicsCompositorAsset()
        {
            GraphicsCompositor = new SceneGraphicsCompositorLayers();
        }

        /// <summary>
        /// Gets or sets the graphics composer for this scene.
        /// </summary>
        /// <value>The graphics composer.</value>
        /// <userdoc>The compositor in charge of creating the graphic pipeline</userdoc>
        [DataMember(20)]
        [Display(Expand = ExpandRule.Always)]
        [NotNull]
        [Category]
        public ISceneGraphicsCompositor GraphicsCompositor { get; set; }

        public override IEnumerable<AssetPart> CollectParts()
        {
            return Enumerable.Empty<AssetPart>();
        }

        public override IIdentifiable FindPart(Guid partId)
        {
            return null;
        }

        public override bool ContainsPart(Guid id)
        {
            return false;
        }

        protected override object ResolvePartReference(object referencedObject)
        {
            return null;
        }
    }
}