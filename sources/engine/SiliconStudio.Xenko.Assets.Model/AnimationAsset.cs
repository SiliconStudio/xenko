// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.ComponentModel;

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Xenko.Animations;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Assets.Model
{
    [DataContract("Animation")]
    [AssetDescription(FileExtension)]
    [AssetCompiler(typeof(AnimationAssetCompiler))]
    [Display(180, "Animation")]
    [AssetFormatVersion(XenkoConfig.PackageName, "1.5.0-alpha02")]
    [AssetUpgrader(XenkoConfig.PackageName, "0", "1.5.0-alpha02", typeof(EmptyAssetUpgrader))]
    public class AnimationAsset : AssetWithSource, IAssetCompileTimeDependencies
    {
        /// <summary>
        /// The default file extension used by the <see cref="AnimationAsset"/>.
        /// </summary>
        public const string FileExtension = ".xkanim;.pdxanim";

        /// <summary>
        /// Gets or sets the scale import.
        /// </summary>
        /// <value>The scale import.</value>
        /// <userdoc>The scale factor to apply to the imported animation.</userdoc>
        [DataMember(10)]
        [DefaultValue(1.0f)]
        public float ScaleImport { get; set; } = 1.0f;

        /// <summary>
        /// Gets or sets the animation repeat mode.
        /// </summary>
        /// <value>The repeat mode</value>
        /// <userdoc>Specifies how the animation should be played. That is played once and stop, infinitely loop, etc...</userdoc>
        [DataMember(20)]
        public AnimationRepeatMode RepeatMode { get; set; } = AnimationRepeatMode.LoopInfinite;

        /// <summary>
        /// Gets or sets the Skeleton.
        /// </summary>
        /// <userdoc>
        /// Describes the node hierarchy that will be active at runtime.
        /// </userdoc>
        [DataMember(50)]
        public Skeleton Skeleton { get; set; }

        /// <summary>
        /// Gets or sets a boolean describing if root movement should be applied inside Skeleton (if false and a skeleton exists) or on TransformComponent (if true)
        /// </summary>
        /// <userdoc>
        /// When root motion is enabled, main motion will be applied to TransformComponent. If false, it will be applied inside the skeleton nodes.
        /// Note that if there is no skeleton, it will always apply motion to TransformComponent.
        /// </userdoc>
        [DataMember(60)]
        public bool RootMotion { get; set; }

        /// <summary>
        /// Gets or sets the preview model
        /// </summary>
        /// <userdoc>
        /// Choose a model to preview with.
        /// </userdoc>
        [DataMember(100)]
        public Rendering.Model PreviewModel { get; set; }

        /// <inheritdoc/>
        public IEnumerable<IReference> EnumerateCompileTimeDependencies()
        {
            var reference = AttachedReferenceManager.GetAttachedReference(Skeleton);
            if (reference != null)
            {
                yield return new AssetReference<Asset>(reference.Id, reference.Url);
            }
        }
    }
}
