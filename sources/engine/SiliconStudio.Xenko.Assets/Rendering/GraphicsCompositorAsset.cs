// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Assets.Serializers;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Xenko.Assets.Scripts;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Assets.Rendering
{
    [DataContract("GraphicsCompositorAsset")]
    [Display(82, "Graphics Compositor")]
    [AssetDescription(FileExtension)]
    [AssetPartReference(typeof(RenderStage))]
    [AssetPartReference(typeof(RootRenderFeature))]
    [AssetCompiler(typeof(GraphicsCompositorAssetCompiler))]
    public class GraphicsCompositorAsset : AssetComposite
    {
        /// <summary>
        /// The default file extension used by the <see cref="GraphicsCompositorAsset"/>.
        /// </summary>
        public const string FileExtension = ".xkgfxcomp";

        /// <summary>
        /// The list of render stages.
        /// </summary>
        [Category]
        [MemberCollection(CanReorderItems = true, NotNullItems = true)]
        public List<RenderStage> RenderStages { get; } = new List<RenderStage>();

        /// <summary>
        ///  The list of render groups.
        /// </summary>
        [Category]
        [MemberCollection(CanReorderItems = true, NotNullItems = true)]
        public List<string> RenderGroups { get; } = new List<string>();

        /// <summary>
        /// The list of render features.
        /// </summary>
        [Category]
        [MemberCollection(CanReorderItems = true, NotNullItems = true)]
        public List<RootRenderFeature> RenderFeatures { get; } = new List<RootRenderFeature>();

        /// <inheritdoc/>
        public override IEnumerable<AssetPart> CollectParts()
        {
            foreach (var renderStage in RenderStages)
                yield return new AssetPart(renderStage.Id, null, newBase => {});
        }

        /// <inheritdoc/>
        public override IIdentifiable FindPart(Guid partId)
        {
            foreach (var renderStage in RenderStages)
            {
                if (renderStage.Id == partId)
                    return renderStage;
            }

            return null;
        }

        /// <inheritdoc/>
        public override bool ContainsPart(Guid partId)
        {
            foreach (var renderStage in RenderStages)
            {
                if (renderStage.Id == partId)
                    return true;
            }

            return false;
        }

        /// <inheritdoc/>
        protected override object ResolvePartReference(object referencedObject)
        {
            var renderStageReference = referencedObject as RenderStage;
            if (renderStageReference != null)
            {
                foreach (var renderStage in RenderStages)
                {
                    if (renderStage.Id == renderStageReference.Id)
                        return renderStage;
                }
                return null;
            }

            return null;
        }
    }
}