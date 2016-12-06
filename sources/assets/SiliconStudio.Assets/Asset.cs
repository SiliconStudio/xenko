// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Yaml;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Base class for Asset.
    /// </summary>
    [DataContract(Inherited = true)]
    public abstract class Asset : IIdentifiable
    {
        private AssetId id;

        // Note: Please keep this code in sync with Package class
        /// <summary>
        /// Locks the unique identifier for further changes.
        /// </summary>
        internal bool IsIdLocked;

        /// <summary>
        /// Initializes a new instance of the <see cref="Asset"/> class.
        /// </summary>
        protected Asset()
        {
            Id = AssetId.New();
            Tags = new TagCollection();

            // Initializse asset with default versions (same code as in Package..ctor())
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
        [DataMember(-10000)]
        [NonOverridable]
        [Display(Browsable = false)]
        public AssetId Id
        {
            // Note: Please keep this code in sync with Package class
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

        // Note: Please keep this code in sync with Package class
        /// <summary>
        /// Gets or sets the version number for this asset, used internally when migrating assets.
        /// </summary>
        /// <value>The version.</value>
        [DataMember(-8000, DataMemberMode.Assign)]
        [DataStyle(DataStyle.Compact)]
        [Display(Browsable = false)]
        [DefaultValue(null)]
        [NonOverridable]
        [NonIdentifiableCollectionItems]
        public Dictionary<string, PackageVersion> SerializedVersion { get; set; }

        /// <summary>
        /// Gets the tags for this asset.
        /// </summary>
        /// <value>
        /// The tags for this asset.
        /// </value>
        [DataMember(-1000)]
        [Display(Browsable = false)]
        [NonIdentifiableCollectionItems]
        [NonOverridable]
        [MemberCollection(NotNullItems = true)]
        public TagCollection Tags { get; private set; }

        [DataMember(-500)]
        [Display(Browsable = false)]
        [NonOverridable]
        [DefaultValue(null)]
        public AssetReference Archetype { get; set; }

        /// <summary>
        /// Gets or sets the build order for this asset.
        /// </summary>
        /// <value>The build order.</value>
        [DataMember(-450)]
        [DefaultValue(0)]
        [Display(Browsable = false)]
        [NonOverridable]
        [Obsolete]
        public int BuildOrder { get; set; }

        /// <summary>
        /// Gets the main source file for this asset, used in the editor.
        /// </summary>
        [DataMemberIgnore]
        public virtual UFile MainSource => null;

        /// <inheritdoc/>
        Guid IIdentifiable.Id { get { return (Guid)Id; } set { Id = (AssetId)value; } }

        /// <summary>
        /// Creates an asset that inherits from this asset.
        /// </summary>
        /// <param name="baseLocation">The location of this asset.</param>
        /// <param name="idRemapping">A dictionary in which will be stored all the <see cref="Guid"/> remapping done for the child asset.</param>
        /// <returns>An asset that inherits this asset instance</returns>
        // TODO: turn internal protected and expose only AssetItem.CreateDerivedAsset()
        public virtual Asset CreateDerivedAsset(string baseLocation, IDictionary<Guid, Guid> idRemapping = null)
        {
            if (baseLocation == null) throw new ArgumentNullException(nameof(baseLocation));

            // Make sure we have identifiers for all items
            AssetCollectionItemIdHelper.GenerateMissingItemIds(this);

            // Clone this asset without overrides (as we want all parameters to inherit from base)
            var newAsset = AssetCloner.Clone(this);

            // Create a new identifier for this asset
            var newId = AssetId.New();

            // Register this new identifier in the remapping dictionary
            idRemapping?.Add((Guid)newAsset.Id, (Guid)newId);
            
            // Write the new id into the new asset.
            newAsset.Id = newId;

            // Create the base of this asset
            newAsset.Archetype = new AssetReference(Id, baseLocation);
            return newAsset;
        }

        /// <summary>
        /// Resolves the actual target of references to a part or base part of this asset. Depending on whether <paramref name="clearMissingReferences"/> is <c>true</c>,
        /// missing references will be cleared, or left as-is.
        /// </summary>
        /// <param name="clearMissingReferences"><c>true</c> to clear missing references to parts or base parts; otherwise, <c>false</c>.</param>
        public virtual void FixupPartReferences(bool clearMissingReferences = true)
        {
        }

        public override string ToString()
        {
            return $"{GetType().Name}: {Id}";
        }
    }
}
