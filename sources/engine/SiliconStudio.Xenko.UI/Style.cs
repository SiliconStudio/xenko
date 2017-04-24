// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;

namespace SiliconStudio.Xenko.UI
{
    /// <summary>
    /// Represents a style that can be applied to a control.
    /// </summary>
    internal class Style
    {
        /// <summary>
        /// Creates a new instance of the Style class to use on the given <see cref="Type"/>.
        /// </summary>
        /// <param name="targetType"></param>
        public Style(Type targetType, Style basedOn = null)
        {
            TargetType = targetType;
            BasedOn = basedOn;
        }

        /// <summary>
        /// Gets or sets the type this style applies to.
        /// </summary>
        public Type TargetType { get; set; }

        /// <summary>
        /// Gets or sets a style used as a base for this style.
        /// </summary>
        public Style BasedOn { get; set; }

        /// <summary>
        /// Gets the list of <see cref="Setter"/>.
        /// </summary>
        public List<Setter> Setters { get; } = new List<Setter>();

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Style for {TargetType}: {Setters.Count} setter(s)";
        }
    }
}
