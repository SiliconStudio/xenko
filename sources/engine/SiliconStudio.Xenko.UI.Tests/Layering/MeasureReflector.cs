// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.UI.Tests.Layering
{
    /// <summary>
    /// Element that returns the size provided during the measure.
    /// Can be used to analyze the size during measure.
    /// </summary>
    public class MeasureReflector: UIElement
    {
        protected override Vector3 MeasureOverride(Vector3 availableSizeWithoutMargins)
        {
            return availableSizeWithoutMargins;
        }
    }
}
