// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// An importable asset.
    /// </summary>
    [DataContract]
    public abstract class AssetImport : Asset
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
        [SourceFileMember(true)]
        public UFile Source { get; set; } = new UFile("");

        /// <summary>
        /// Gets or sets id of the importer used.
        /// </summary>
        /// <value>The id of the importer.</value>
        [DataMember(-40)]
        [DefaultValue(null)]
        [Display(Browsable = false)]
        [Obsolete("This property will be removed soon")]
        public Guid? ImporterId { get; set; } = null;
        
        internal AssetImport GetRootBase()
        {
            if (Base?.Asset is AssetImport && Base.Id == Guid.Empty)
            {
                return (AssetImport)Base.Asset;
            }
            return null;
        }

        virtual internal void SetAsRootImport()
        {
            Id = Guid.Empty;
            Source = null;
        }
    }
}
