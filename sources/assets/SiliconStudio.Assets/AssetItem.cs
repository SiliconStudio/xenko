// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.Assets.Analysis;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Assets
{

    /// <summary>
    /// An asset item part of a <see cref="Package"/> accessible through <see cref="SiliconStudio.Assets.Package.Assets"/>.
    /// </summary>
    [DataContract("AssetItem")]
    public sealed class AssetItem : IFileSynchronizable
    {
        private UFile location;
        private Asset asset;
        private bool isDirty;
        private Package package;
        private UDirectory sourceFolder;
        private UFile sourceProject;

        /// <summary>
        /// The default comparer use only the id of an assetitem to match assets.
        /// </summary>
        public static readonly IEqualityComparer<AssetItem> DefaultComparerById = new AssetItemComparerById();

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetItem" /> class.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="asset">The asset.</param>
        /// <exception cref="System.ArgumentNullException">location</exception>
        /// <exception cref="System.ArgumentException">asset</exception>
        public AssetItem(UFile location, Asset asset) : this(location, asset, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetItem" /> class.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="asset">The asset.</param>
        /// <param name="package">The package.</param>
        /// <exception cref="System.ArgumentNullException">location</exception>
        /// <exception cref="System.ArgumentException">asset</exception>
        internal AssetItem(UFile location, Asset asset, Package package)
        {
            if (location == null) throw new ArgumentNullException("location");
            if (asset == null) throw new ArgumentException("asset");
            this.location = location;
            this.asset = asset;
            this.package = package;
            isDirty = true;
        }

        /// <summary>
        /// Gets the location of this asset.
        /// </summary>
        /// <value>The location.</value>
        public UFile Location
        {
            get
            {
                return location;
            }
            internal set
            {
                if (value == null) throw new ArgumentNullException("value");
                this.location = value;
            }
        }

        /// <summary>
        /// Gets the directory where the assets will be stored on the disk relative to the <see cref="Package"/>. The directory
        /// will update the list found in <see cref="PackageProfile.AssetFolders"/>
        /// </summary>
        /// <value>The directory.</value>
        public UDirectory SourceFolder
        {
            get
            {
                return sourceFolder;
            }
            set
            {
                sourceFolder = value;
            }
        }

        public UFile SourceProject
        {
            get
            {
                return sourceProject;
                
            }
            set
            {
                sourceProject = value;
            }
        }

        /// <summary>
        /// Gets the unique identifier of this asset.
        /// </summary>
        /// <value>The unique identifier.</value>
        public Guid Id
        {
            get
            {
                return asset.Id;
            }
        }

        /// <summary>
        /// Gets the package where this asset is stored.
        /// </summary>
        /// <value>The package.</value>
        public Package Package
        {
            get
            {
                return package;
            }
            internal set
            {
                package = value;
            }
        }

        /// <summary>
        /// Converts this item to a reference.
        /// </summary>
        /// <returns>AssetReference.</returns>
        public AssetReference ToReference()
        {
            return new AssetReference<Asset>(Id, Location);
        }

        /// <summary>
        /// Clones this instance without the attached package.
        /// </summary>
        /// <param name="newLocation">The new location that will be used in the cloned <see cref="AssetItem"/>. If this parameter
        /// is null, it keeps the original location of the asset.</param>
        /// <param name="newAsset">The new asset that will be used in the cloned <see cref="AssetItem"/>. If this parameter
        /// is null, it clones the original asset. otherwise, the specified asset is used as-is in the new <see cref="AssetItem"/>
        /// (no clone on newAsset is performed)</param>
        /// <returns>A clone of this instance.</returns>
        public AssetItem Clone(UFile newLocation = null, Asset newAsset = null)
        {
            return Clone(false, newLocation, newAsset);
        }

        /// <summary>
        /// Clones this instance without the attached package.
        /// </summary>
        /// <param name="newLocation">The new location that will be used in the cloned <see cref="AssetItem" />. If this parameter
        /// is null, it keeps the original location of the asset.</param>
        /// <param name="newAsset">The new asset that will be used in the cloned <see cref="AssetItem" />. If this parameter
        /// is null, it clones the original asset. otherwise, the specified asset is used as-is in the new <see cref="AssetItem" />
        /// (no clone on newAsset is performed)</param>
        /// <param name="copyPackage">if set to <c>true</c> copy package information, only used by the <see cref="AssetDependencyManager" />.</param>
        /// <returns>A clone of this instance.</returns>
        internal AssetItem Clone(bool copyPackage, UFile newLocation = null, Asset newAsset = null)
        {
            // Set the package after the new AssetItem(), to make sure that isDirty is not going to call a notification on the
            // package
            var item = new AssetItem(newLocation ?? location, newAsset ?? (Asset)AssetCloner.Clone(Asset, AssetClonerFlags.KeepBases), copyPackage ? Package : null)
                {
                    isDirty = isDirty,
                    SourceFolder = SourceFolder,
                    SourceProject = SourceProject
                };
            return item;
        }

        /// <summary>
        /// Gets the full absolute path of this asset on the disk, taking into account the <see cref="SourceFolder"/>, and the
        /// <see cref="SiliconStudio.Assets.Package.RootDirectory"/>. See remarks.
        /// </summary>
        /// <value>The full absolute path of this asset on the disk.</value>
        /// <remarks>
        /// This value is only valid if this instance is attached to a <see cref="Package"/>, and that the package has 
        /// a non null <see cref="SiliconStudio.Assets.Package.RootDirectory"/>.
        /// </remarks>
        public UFile FullPath
        {
            get
            {
                var localSourceFolder = SourceFolder ?? (Package != null ? 
                    Package.GetDefaultAssetFolder()
                    : UDirectory.This );

                // Root directory of package
                var rootDirectory = Package != null && Package.RootDirectory != null ? Package.RootDirectory : null;

                // If the source folder is absolute, make it relative to the root directory
                if (localSourceFolder.IsAbsolute)
                {
                    if (rootDirectory != null)
                    {
                        localSourceFolder = localSourceFolder.MakeRelative(rootDirectory);
                    }
                }

                rootDirectory = rootDirectory != null ? UPath.Combine(rootDirectory, localSourceFolder) : localSourceFolder;

                var locationAndExtension = new UFile(Location + AssetRegistry.GetDefaultExtension(Asset.GetType()));
                return rootDirectory != null ? UPath.Combine(rootDirectory, locationAndExtension) : locationAndExtension;
            }
        }

        /// <summary>
        /// Gets or sets the asset.
        /// </summary>
        /// <value>The asset.</value>
        public Asset Asset
        {
            get
            {
                return asset;
            }
            internal set
            {
                if (value == null) throw new ArgumentNullException("value");
                asset = value;
            }
        }

        /// <summary>
        /// Gets the modified time. See remarks.
        /// </summary>
        /// <value>The modified time.</value>
        /// <remarks>
        /// By default, contains the last modified time of the asset from the disk. If IsDirty is also updated from false to true
        /// , this time will get current time of modification.
        /// </remarks>
        public DateTime ModifiedTime { get; internal set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is dirty. See remarks.
        /// </summary>
        /// <value><c>true</c> if this instance is dirty; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// When an asset is modified, this property must be set to true in order to track assets changes.
        /// </remarks>
        public bool IsDirty
        {
            get
            {
                return isDirty;
            }
            set
            {
                if (value && !isDirty)
                {
                    ModifiedTime = DateTime.Now;
                }

                var oldValue = isDirty;
                isDirty = value;
                Package?.OnAssetDirtyChanged(asset, oldValue, value);
            }
        }

        public override string ToString()
        {
            return $"[{Asset.GetType().Name}] {location} => {Id}";
        }

        /// <summary>
        /// Creates a child asset that is inheriting the values of this asset.
        /// </summary>
        /// <returns>A new asset inheriting the values of this asset.</returns>
        public Asset CreateChildAsset()
        {
            return Asset.CreateChildAsset(Location);
        }

        /// <summary>
        /// Finds the base item referenced by this item from the current session (using the <see cref="Package"/> setup
        /// on this instance)
        /// </summary>
        /// <returns>The base item or null if not found.</returns>
        public AssetItem FindBase()
        {
            if (Package == null || Package.Session == null || Asset.Base == null || Asset.Base.IsRootImport)
            {
                return null;
            }
            var session = Package.Session;
            return session.FindAsset(Asset.Base.Id);
        }

        /// <summary>
        /// This methods returns all assets that would be changed when trying to change this asset.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="action">The action.</param>
        /// <param name="value">The value.</param>
        /// <returns>LoggerResult.</returns>
        /// <exception cref="System.ArgumentNullException">path</exception>
        public List<AssetItem> FindAssetsFromChange(MemberPath path, MemberPathAction action, object value)
        {
            if (path == null) throw new ArgumentNullException("path");

            var result = new List<AssetItem>();

            FindAssetsFromChange(path, action, value, result);
            return result;
        }

        private void FindAssetsFromChange(MemberPath path, MemberPathAction action, object value, List<AssetItem> items)
        {
            object oldValue;
            var pathSucceeded = path.TryGetValue(Asset, out oldValue);

            // If the path exists and value changed or we are doing another operation (remove key...etc.)
            // then add the items as a list of item to change
            if (pathSucceeded && (action != MemberPathAction.ValueSet || value != oldValue))
            {
                items.Add(this);
            }

            if (Package != null && Package.Session != null)
            {
                var itemsToDetect = Package.Session.DependencyManager.FindAssetsInheritingFrom(Id);
                foreach (var item in itemsToDetect)
                {
                    item.FindAssetsFromChange(path, action, value, items);
                }
            }
        }

        private class AssetItemComparerById : IEqualityComparer<AssetItem>
        {
            public bool Equals(AssetItem x, AssetItem y)
            {
                if (ReferenceEquals(x, y))
                    return true;

                if (x == null || y == null)
                {
                    return false;
                }

                if (ReferenceEquals(x.Asset, y.Asset))
                {
                    return true;
                }

                return x.Id == y.Id;
            }

            public int GetHashCode(AssetItem obj)
            {
                return obj.Id.GetHashCode();
            }
        }
    }
}
