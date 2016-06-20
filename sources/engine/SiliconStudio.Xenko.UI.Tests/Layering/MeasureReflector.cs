// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.UI.Tests.Layering
{
    /// <summary>
    /// Element that returns the size provided during the measure.
    /// Can be used to analyze the size during measure.
    /// </summary>
    public class MeasureReflector: UIElement
    {
        /// <inheritdoc/>
        protected override IEnumerable<IUIElementChildren> EnumerateChildren() => Enumerable.Empty<IUIElementChildren>();

        protected override Vector3 MeasureOverride(Vector3 availableSizeWithoutMargins)
        {
            return availableSizeWithoutMargins;
        }
    }
}
