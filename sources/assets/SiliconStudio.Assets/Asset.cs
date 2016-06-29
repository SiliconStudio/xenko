// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using SiliconStudio.Assets.Diff;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Base class for Asset.
    /// </summary>
    [DataContract(Inherited = true)]
    public abstract class Asset : IIdentifiable
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

            // Initializse asset with default versions
            var defaultPackageVersion = AssetRegistry.GetCurrentFormatVersions(GetType());
            if (defaultPackageVersion != null)
            {
                SerializedVersion = new Dictionary<string, PackageVersion>(defaultPackageVersion);
            }
        }

        /// <summary>
        /// Gets the build order, currently per type (replaces BuildOrder). Later, we want per asset dependencies to improve parallelism
        /// </summary>
        protected internal virtual int InternalBuildOrder => 0;

        /// <summary>
        /// Gets or sets the unique identifier of this asset.
        /// </summary>
        /// <value>The identifier.</value>
        /// <exception cref="System.InvalidOperationException">Cannot change an Asset Object Id once it is locked</exception>
        [DataMember(-2000)]
        [Display(Browsable = false)]
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
        [DataMember(-1000, DataMemberMode.Assign)]
        [DataStyle(DataStyle.Compact)]
        [Display(Browsable = false)]
        [DefaultValue(null)]
        public Dictionary<string, PackageVersion> SerializedVersion { get; set; }

        /// <summary>
        /// Gets or sets the base.
        /// </summary>
        /// <value>The base.</value>
        [DataMember(BaseProperty), DefaultValue(null)]
        [Display(Browsable = false)]
        public AssetBase Base { get; set; }

        /// <summary>
        /// The YAML serialized name of the <see cref="Base"/> property.
        /// </summary>
        public const string BaseProperty = "~" + nameof(Base);

        /// <summary>
        /// Gets or sets the base for part assets.
        /// </summary>
        /// <value>The part assets.</value>
        [DataMember(BasePartsProperty), DefaultValue(null)]
        [Display(Browsable = false)]
        [NotNullItems]
        public List<AssetBase> BaseParts { get; set; }

        /// <summary>
        /// The YAML serialized name of the <see cref="BaseParts"/> property.
        /// </summary>
        public const string BasePartsProperty = "~" + nameof(BaseParts);

        /// <summary>
        /// Gets or sets the build order for this asset.
        /// </summary>
        /// <value>The build order.</value>
        [DataMember(-980)]
        [DefaultValue(0)]
        [Display(Browsable = false)]
        [Obsolete]
        public int BuildOrder { get; set; }

        /// <summary>
        /// Gets the tags for this asset.
        /// </summary>
        /// <value>
        /// The tags for this asset.
        /// </value>
        [DataMember(-900)]
        [Display(Browsable = false)]
        [NotNullItems]
        public TagCollection Tags { get; private set; }

        /// <summary>
        /// Gets the main source file for this asset, used in the editor.
        /// </summary>
        [DataMemberIgnore]
        public virtual UFile MainSource => null;

        /// <summary>
        /// Creates an asset that inherits from this asset.
        /// </summary>
        /// <param name="location">The location of this asset.</param>
        /// <returns>An asset that inherits this asset instance</returns>
        public virtual Asset CreateChildAsset(string location)
        {
            if (location == null) throw new ArgumentNullException(nameof(location));

            // Clone this asset to make the base
            var assetBase = (Asset)AssetCloner.Clone(this);

            // Clone it again without the base and without overrides (as we want all parameters to inherit from base)
            var newAsset = (Asset)AssetCloner.Clone(assetBase, AssetClonerFlags.RemoveOverrides);

            // Sets a new identifier for this asset
            newAsset.Id = Guid.NewGuid();

            // Create the base of this asset
            newAsset.Base = new AssetBase(location, assetBase);
            return newAsset;
        }

        /// <summary>
        /// Merge an asset with its base, and new base and parts into this instance.
        /// </summary>
        /// <param name="baseAsset">A copy of the base asset. Can be null if no base asset for newAsset</param>
        /// <param name="newBase">A copy of the next base asset. Can be null if no base asset for newAsset.</param>
        /// <param name="newBaseParts">A copy of the new base parts</param>
        /// <param name="debugLocation">The location of the asset being merged, used only for debug/log purpose</param>
        /// <returns>The result of the merge</returns>
        /// <remarks>The this instance is not used by this method.</remarks>
        public virtual MergeResult Merge(Asset baseAsset, Asset newBase, List<AssetBase> newBaseParts, UFile debugLocation = null)
        {
            var diff = new AssetDiff(baseAsset, this, newBase)
            {
                UseOverrideMode = true
            };

            return AssetMerge.Merge(diff, AssetMergePolicies.MergePolicyAsset2AsNewBaseOfAsset1);
        }

        public override string ToString()
        {
            return $"{GetType().Name}: {Id}";
        }
    }
}
