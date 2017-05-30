// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// An asset that references a source file used during the compilation of the asset.
    /// </summary>
    [DataContract]
    public abstract class AssetWithSource : Asset
    {
        /// <summary>
        /// Gets or sets the source file of this asset.
        /// </summary>
        /// <value>The source.</value>
        /// <userdoc>
        /// The source file of this asset.
        /// </userdoc>
        [DataMember(-50)]
        [DefaultValue(null)]
        [SourceFileMember(false)]
        public UFile Source { get; set; } = new UFile("");

        [DataMemberIgnore]
        public override UFile MainSource => Source;
    }
}
