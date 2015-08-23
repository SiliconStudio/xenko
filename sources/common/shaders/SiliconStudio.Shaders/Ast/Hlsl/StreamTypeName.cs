// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Globalization;
using System.Linq;

namespace SiliconStudio.Shaders.Ast.Hlsl
{
    /// <summary>
    /// A State type.
    /// </summary>
    public class StreamTypeName : ObjectType
    {
        #region Constants and Fields

        /// <summary>
        /// A PointStream
        /// </summary>
        public static readonly StreamTypeName PointStream = new StreamTypeName("PointStream");

        /// <summary>
        /// A LineStream.
        /// </summary>
        public static readonly StreamTypeName LineStream = new StreamTypeName("LineStream");

        /// <summary>
        /// A TriangleStream.
        /// </summary>
        public static readonly StreamTypeName TriangleStream = new StreamTypeName("TriangleStream");

        private static readonly StreamTypeName[] StreamTypesName = new[] { PointStream, LineStream, TriangleStream };

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamTypeName"/> class.
        /// </summary>
        public StreamTypeName()
        {
            IsBuiltIn = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamTypeName"/> class.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        public StreamTypeName(string name, params string[] altNames)
            : base(name, altNames)
        {
            IsBuiltIn = true;
        }

        /// <inheritdoc/>
        public bool Equals(StreamTypeName other)
        {
            return base.Equals(other);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            return Equals(obj as StreamTypeName);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(StreamTypeName left, StreamTypeName right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(StreamTypeName left, StreamTypeName right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Parses the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static StreamTypeName Parse(string name)
        {
            return StreamTypesName.FirstOrDefault(streamType =>  CultureInfo.InvariantCulture.CompareInfo.Compare(name, streamType.Name.Text, CompareOptions.None) == 0);
        }
    }
}