// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.Xenko.Shaders
{
    /// <summary>
    /// A dictionary of associations betweens asset shader urls and <see cref="ObjectId"/>
    /// </summary>
    [DataContract]
    public class HashSourceCollection : Dictionary<string, ObjectId>, IEquatable<HashSourceCollection>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HashSourceCollection"/> class.
        /// </summary>
        public HashSourceCollection()
        {
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
        public bool Equals(HashSourceCollection other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return Utilities.Compare<string, ObjectId>(this, other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((HashSourceCollection)obj);
        }

        public override int GetHashCode()
        {
            return Utilities.GetHashCode(this);
        }
    }
}
