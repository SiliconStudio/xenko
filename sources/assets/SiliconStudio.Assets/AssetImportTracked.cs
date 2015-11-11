// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// An importable asset with a content that need to be tracked if original asset is changing.
    /// </summary>
    [DataContract]
    public abstract class AssetImportTracked : AssetImport
    {
        /// <summary>
        /// Gets or sets the source hash.
        /// </summary>
        /// <value>The source hash.</value>
        [DataMember(-30)]
        [DefaultValue(null)]
        [Display(Browsable = false)]
        public ObjectId? SourceHash { get; set; }

        override internal void SetAsRootImport()
        {
            base.SetAsRootImport();
            ImporterId = null;
            SourceHash = null;
        }
    }
}