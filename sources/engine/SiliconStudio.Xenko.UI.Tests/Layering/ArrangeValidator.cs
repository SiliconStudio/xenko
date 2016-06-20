// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.UI.Tests.Layering
{
    class ArrangeValidator : UIElement
    {
        public Vector3 ExpectedArrangeValue;
        public Vector3 ReturnedMeasuredValue;

        /// <inheritdoc/>
        protected override IEnumerable<IUIElementChildren> EnumerateChildren() => Enumerable.Empty<IUIElementChildren>();

        protected override Vector3 MeasureOverride(Vector3 availableSizeWithoutMargins)
        {
            return ReturnedMeasuredValue;
        }

        protected override Vector3 ArrangeOverride(Vector3 finalSizeWithoutMargins)
        {
            var maxLength = Math.Max(finalSizeWithoutMargins.Length(), ExpectedArrangeValue.Length());
            Assert.IsTrue((finalSizeWithoutMargins - ExpectedArrangeValue).Length() <= maxLength * 0.001f, 
                "Arrange validator test failed: expected value=" + ExpectedArrangeValue + ", Received value=" + finalSizeWithoutMargins + " (Validator='" + Name + "'");

            return base.ArrangeOverride(finalSizeWithoutMargins);
        }
    }
}
