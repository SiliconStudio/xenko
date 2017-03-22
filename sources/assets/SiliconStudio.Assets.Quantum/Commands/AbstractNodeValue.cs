// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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

        public AbstractNodeValue(object value, string displayValue)
        {
            Value = value;
            DisplayValue = displayValue;
        }

        /// <summary>
        /// The value.
        /// </summary>
        public object Value { get; }

        /// <inheritdoc/>
        public override string DisplayValue { get; }

        /// <inheritdoc/>
        public override object GenerateValue(object currentValue) => Value;

        /// <inheritdoc/>
        public override bool IsMatchingValue(object value) => ReferenceEquals(Value, value);
    }
}