// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// Class ParameterKeyPermutationsMetadata.
    /// </summary>
    public class ParameterKeyPermutationsMetadata : PropertyKeyMetadata
    {
        // TODO: Should we provide a callback GetValues() instead?

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterKeyPermutationsMetadata"/> class.
        /// </summary>
        /// <param name="values">The values.</param>
        public ParameterKeyPermutationsMetadata(params object[] values)
        {
            Values = values;
        }

        /// <summary>
        /// Gets the values.
        /// </summary>
        /// <value>The values.</value>
        public object[] Values { get; private set; }
    }
}