// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Assets.Quantum.Commands
{
    /// <summary>
    /// Represents a specific value for <see cref="CreateNewInstanceCommand"/>.
    /// </summary>
    public class AbstractNodeValue : AbstractNodeEntry
    {
        /// <summary>
        /// An object that can be passed as parameter to the command, in order to set the value of the node to <c>null</c>.
        /// </summary>
        public static AbstractNodeValue Null { get; } = new AbstractNodeValue(null, "None");

        public AbstractNodeValue(object value, [NotNull] string displayValue)
        {
            if (displayValue == null) throw new ArgumentNullException(nameof(displayValue));
            Value = value;
            DisplayValue = displayValue;
        }

        /// <summary>
        /// The value.
        /// </summary>
        public object Value { get; }

        /// <inheritdoc/>
        public override bool Equals(AbstractNodeEntry other)
        {
            var abstractNodeValue = other as AbstractNodeValue;
            if (abstractNodeValue == null)
                return false;

            return Equals(Value, abstractNodeValue.Value);
        }

        /// <inheritdoc/>
        public override string DisplayValue { get; }

        /// <inheritdoc/>
        public override object GenerateValue(object currentValue) => Value;

        /// <inheritdoc/>
        public override bool IsMatchingValue(object value) => ReferenceEquals(Value, value);

        /// <inheritdoc/>
        protected override int ComputeHashCode()
        {
            return (DisplayValue.GetHashCode() * 397) ^ (Value?.GetHashCode() ?? 0);
        }
    }
}