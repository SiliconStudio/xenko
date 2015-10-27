// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Shaders
{
    /// <summary>
    /// An array of <see cref="ShaderSource"/> used only in shader mixin compositions.
    /// </summary>
    [DataContract("ShaderArraySource")]
    public sealed class ShaderArraySource : ShaderSource, IEnumerable<ShaderSource>, IEquatable<ShaderArraySource>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderArraySource"/> class.
        /// </summary>
        public ShaderArraySource()
        {
            Values = new List<ShaderSource>();
        }

        /// <summary>
        /// Gets or sets the values.
        /// </summary>
        /// <value>The values.</value>
        public List<ShaderSource> Values { get; set; }

        /// <summary>
        /// Adds the specified composition.
        /// </summary>
        /// <param name="composition">The composition.</param>
        public void Add(ShaderSource composition)
        {
            Values.Add(composition);
        }

        public override object Clone()
        {
            return new ShaderArraySource { Values = Values.Select(x => (ShaderSource)x.Clone()).ToList() };
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ShaderArraySource)obj);
        }

        public bool Equals(ShaderArraySource other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            if (ReferenceEquals(Values, other.Values))
                return true;

            if (ReferenceEquals(Values, null))
                return false;

            if (Values.Count != other.Values.Count)
                return false;

            return !Values.Where((t, i) => !t.Equals(other.Values[i])).Any();
        }

        public override int GetHashCode()
        {
            return Utilities.GetHashCode(Values);
        }

        public IEnumerator<ShaderSource> GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            return string.Format("[{0}]", Values != null ? string.Join(", ", Values) : string.Empty);
        }
    }
}