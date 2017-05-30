// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Core;

namespace SiliconStudio.Packages
{
    /// <summary>
    /// Representation of the name of a package made of an Id and a version.
    /// </summary>
    public class PackageName
    {
        /// <summary>
        /// Initializes a new instance of <see cref="PackageName"/>.
        /// </summary>
        /// <param name="id">Id of package.</param>
        /// <param name="version">Version of package.</param>
        public PackageName(string id, PackageVersion version)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            Id = id;
            Version = version;
        }

        /// <inheritdoc cref="object"/>
        public override int GetHashCode()
        {
            return (Id.GetHashCode() * 397) ^ Version.GetHashCode();
        }

        /// <summary>
        /// Is this instance equal to <paramref name="other"/>?
        /// </summary>
        /// <param name="other">Other PackageName to compare against.</param>
        /// <returns>"<c>true</c>if <paramref name="other"/> is equal to this instance, <c>false</c> otherwise.</returns>
        protected bool Equals(PackageName other)
        {
            if (other == null) return false;
            return Equals(Id, other.Id) && Equals(Version, other.Version);
        }

        /// <inheritdoc cref="object"/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((PackageName)obj);
        }


        /// <summary>
        /// Identity of the package.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Version of the package.
        /// </summary>
        public PackageVersion Version { get; }
    }
}
