// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using NUnit.Framework;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.UI.Tests.Layering
{
    class ArrangeValidator : UIElement
    {
        public Vector3 ExpectedArrangeValue;
        public Vector3 ReturnedMeasuredValue;

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
