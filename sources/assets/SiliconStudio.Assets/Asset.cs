// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.ComponentModel;
using SiliconStudio.Core;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Base class for Asset.
    /// </summary>
    [DataContract(Inherited = true)]
    public abstract class Asset
    {
        private Guid id;

        /// <summary>
        /// Locks the unique identifier for further changes.
        /// </summary>
        internal bool IsIdLocked;

        /// <summary>
        /// Initializes a new instance of the <see cref="Asset"/> class.
        /// </summary>
        protected Asset()
        {
            Id = Guid.NewGuid();
            Tags = new TagCollection();
            AssetFormatVersion = AssetRegistry.GetCurrentFormatVersion(GetType());
        }

        /// <summary>
        /// Gets the build order, currently per type (replaces BuildOrder). Later, we want per asset dependencies to improve parallelism
        /// </summary>
        internal protected virtual int InternalBuildOrder 
        {
            get { return 0; }
        }

        /// <summary>
        /// Gets or sets the unique identifier of this asset.
        /// </summary>
        /// <value>The identifier.</value>
        /// <exception cref="System.InvalidOperationException">Cannot change an Asset Object Id once it is locked</exception>
        [DataMember(-2000)]
        [Browsable(false)]
        public Guid Id
        {
            get
            {
                return id;
            }
            set
            {
                if (value != id && IsIdLocked) 
                    throw new InvalidOperationException("Cannot change an Asset Object Id once it is locked by a package");

                id = value;
            }
        }

        /// <summary>
        /// Gets or sets the version number for this asset, used internally when migrating assets.
        /// </summary>
        /// <value>The version.</value>
        [DataMember(-1000)]
        [Browsable(false)]
        [DefaultValue(0)]
        public int SerializedVersion { get;  set; }

        /// <summary>
        /// Gets the current asset format version for this asset.
        /// </summary>
        /// <value>The current asset format version for this asset.</value>
        [DataMemberIgnore]
        public int AssetFormatVersion { get; private set; }

        /// <summary>
        /// Gets or sets the base.
        /// </summary>
        /// <value>The base.</value>
        [DataMember("~Base"), DefaultValue(null)]
        [Browsable(false)]
        public AssetBase Base { get; set; }

        /// <summary>
        /// Gets or sets the build order for this asset.
        /// </summary>
        /// <value>The build order.</value>
        [DataMember(-980)]
        [DefaultValue(0)]
        [Browsable(false)]
        [Obsolete]
        public int BuildOrder { get; set; }

        /// <summary>
        /// Gets the tags for this asset.
        /// </summary>
        /// <value>
        /// The tags for this asset.
        /// </value>
        [DataMember(-900)]
        [Browsable(false)]
        public TagCollection Tags { get; private set; }

        /// <summary>
        /// Sets the defaults values for this instance
        /// </summary>
        public virtual void SetDefaults()
        {
        }

        public override string ToString()
        {
            return Id.ToString();
        }
    }
}