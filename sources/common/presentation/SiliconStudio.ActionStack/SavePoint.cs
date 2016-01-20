// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.ActionStack
{
    /// <summary>
    /// Represents a save point marker in the undo/redo action items stack.
    /// </summary>
    /// <remarks>A save point does not hold any reference to an action item or an action stack. It only stores the identifier of the related action item</remarks>
    public sealed class SavePoint : IEquatable<SavePoint>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SavePoint"/> class.
        /// </summary>
        /// <param name="identifier">Identifier of the action item pointed by this save point.</param>
        internal SavePoint(Guid identifier)
        {
            ActionItemIdentifier = identifier;
        }

        /// <summary>
        /// Gets the identifier of the action item pointed by the current marker.
        /// </summary>
        public Guid ActionItemIdentifier { get; }

        /// <summary>
        /// Empty save point.
        /// </summary>
        public static readonly SavePoint Empty = new SavePoint(Guid.Empty);

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return ActionItemIdentifier.GetHashCode();
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj != null && Equals(obj as SavePoint);
        }

        /// <inheritdoc/>
        public bool Equals(SavePoint other)
        {
            if ((object)other == null)
                return false;

            // return equality check of the guids
            return ActionItemIdentifier == other.ActionItemIdentifier;
        }

        /// <summary>
        /// Compares two <see cref="SavePoint"/> by their identifier and returns <c>true</c> if they are equal.
        /// </summary>
        /// <param name="left">The first save point to compare.</param>
        /// <param name="right">The second save point to compare.</param>
        /// <returns><c>true</c> if the save points are equal, <c>false</c> otherwise.</returns>
        public static bool operator ==(SavePoint left, SavePoint right)
        {
            // first check reference equality (if references are equal, then the value are equal)
            // it returns true if both references are null
            if (ReferenceEquals(left, right))
                return true;

            // if one reference is null, then they cannot be equal
            if ((object)left == null || (object)right == null)
                return false;

            // return equality check of the guids
            return left.ActionItemIdentifier == right.ActionItemIdentifier;
        }

        /// <summary>
        /// Compares two <see cref="SavePoint"/> by their identifier and returns <c>true</c> if they are different.
        /// </summary>
        /// <param name="left">The first save point to compare.</param>
        /// <param name="right">The second save point to compare.</param>
        /// <returns><c>true</c> if the save points are different, <c>false</c> otherwise.</returns>
        public static bool operator !=(SavePoint left, SavePoint right)
        {
            return !(left == right);
        }
    }
}
