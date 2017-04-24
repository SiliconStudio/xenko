// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using NUnit.Framework;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.UI.Tests.Layering
{
    class MeasureArrangeValidator : UIElement
    {
        public Vector3 ExpectedMeasureValue;
        public Vector3 ExpectedArrangeValue;
        public Vector3 ReturnedMeasuredValue;

        protected override Vector3 MeasureOverride(Vector3 availableSizeWithoutMargins)
        {
            for (int i = 0; i < 3; i++)
            {
                var val1 = availableSizeWithoutMargins[i];
                var val2 = ExpectedMeasureValue[i];

                if (val1 == val2) continue; // value can be infinity

                var maxLength = Math.Max(Math.Abs(val1), Math.Abs(val2));
                Assert.IsTrue(Math.Abs(val1 - val2) < maxLength * 0.001f,
                    "Measure arrange validator test failed: expected value=" + ExpectedMeasureValue + ", Received value=" + availableSizeWithoutMargins + " (Validator='" + Name + "'");
            }

            return ReturnedMeasuredValue;
        }

        protected override Vector3 ArrangeOverride(Vector3 finalSizeWithoutMargins)
        {
            var maxLength = Math.Max(finalSizeWithoutMargins.Length(), ExpectedArrangeValue.Length());
            Assert.IsTrue((finalSizeWithoutMargins - ExpectedArrangeValue).Length() <= maxLength * 0.001f);

            return base.ArrangeOverride(finalSizeWithoutMargins);
        }
    }
}
