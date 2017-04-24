// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core;

namespace SiliconStudio.Assets
{
    [DataContract("ProjectSourceCodeWithFileGeneratorAsset")]
    public abstract class ProjectSourceCodeWithFileGeneratorAsset : ProjectSourceCodeAsset, IProjectFileGeneratorAsset
    {
        /// <inheritdoc/>
        [DataMember(Mask = DataMemberAttribute.IgnoreMask)]
        [Display(Browsable = false)]
        public abstract string Generator { get; }

        /// <param name="assetItem"></param>
        /// <inheritdoc/>
        public abstract void SaveGeneratedAsset(AssetItem assetItem);
    }
}
