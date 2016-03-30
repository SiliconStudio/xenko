// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using SiliconStudio.Core;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// A collection of <see cref="PackageProfile"/>.
    /// </summary>
    [DataContract("PackageDependencyCollection")]
    public sealed class PackageDependencyCollection : KeyedCollection<string, PackageDependency>
    {
        protected override string GetKeyForItem(PackageDependency item)
        {
            return item.Name;
        }
    }

    /// <summary>
    /// A reference to a package either internal (directly to a <see cref="Package"/> inside the same solution) or external
    /// (to a package distributed on the store).
    /// </summary>
    [DataContract("PackageDependency")]
    [NonIdentifiable]
    public sealed class PackageDependency : PackageReferenceBase, IEquatable<PackageDependency>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PackageDependency"/> class.
        /// </summary>
        public PackageDependency()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageDependency"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="version">The version.</param>
        public PackageDependency(string name, PackageVersionRange version)
        {
            Name = name;
            Version = version;
        }

        /// <summary>
        /// Gets or sets the package name Id.
        /// </summary>
        /// <value>The name.</value>
        [DefaultValue(null)]
        [DataMember(10)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>The version.</value>
        [DefaultValue(null)]
        [DataMember(20)]
        public PackageVersionRange Version { get; set; }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>PackageDependency.</returns>
        public PackageDependency Clone()
        {
            return new PackageDependency(Name, Version);
        }

        public bool Equals(PackageDependency other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Name, other.Name) && Equals(Version, other.Version);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is PackageDependency && Equals((PackageDependency)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0)*397) ^ (Version != null ? Version.GetHashCode() : 0);
            }
        }

        public static bool operator ==(PackageDependency left, PackageDependency right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PackageDependency left, PackageDependency right)
        {
            return !Equals(left, right);
        }

        /// <inherit/>
        public override string ToString()
        {
            if (Name != null)
            {
                return string.Format("{0} {1}", Name, Version);
            }
            return "Empty";
        }
    }
}