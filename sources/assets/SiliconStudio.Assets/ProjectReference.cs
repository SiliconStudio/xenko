// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Diagnostics;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// A reference to a Visual Studio project that is part of a <see cref="Package"/>
    /// </summary>
    [DataContract("ProjectReference")]
    [DebuggerDisplay("Id: {Id}, Location: {Location}")]
    public sealed class ProjectReference : IEquatable<ProjectReference>, IIdentifiable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectReference"/> class.
        /// </summary>
        public ProjectReference()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectReference"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="location">The location.</param>
        /// <param name="type">The type.</param>
        public ProjectReference(Guid id, UFile location, ProjectType type)
        {
            Id = id;
            Location = location;
            Type = type;
        }

        /// <summary>
        /// Gets or sets the unique identifier of the VS project.
        /// </summary>
        /// <value>The identifier.</value>
        [DataMember(10)]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the location of the file on the disk.
        /// </summary>
        /// <value>The location.</value>
        [DataMember(20)]
        public UFile Location { get; set; }

        /// <summary>
        /// Gets or sets the type of project.
        /// </summary>
        /// <value>The type.</value>
        [DataMember(30)]
        public ProjectType Type { get; set; }

        /// <summary>
        /// Gets or set the root namespace of the project
        /// </summary>
        [DataMemberIgnore]
        public string RootNamespace { get; internal set; }

        public bool Equals(ProjectReference other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id) && Equals(Location, other.Location) && Type == other.Type;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is ProjectReference && Equals((ProjectReference)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id.GetHashCode();
                hashCode = (hashCode*397) ^ (Location != null ? Location.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (int)Type;
                return hashCode;
            }
        }

        public static bool operator ==(ProjectReference left, ProjectReference right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ProjectReference left, ProjectReference right)
        {
            return !Equals(left, right);
        }
    }
}
