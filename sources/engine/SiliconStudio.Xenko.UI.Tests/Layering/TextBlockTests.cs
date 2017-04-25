// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using NUnit.Framework;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.UI.Controls;

namespace SiliconStudio.Xenko.UI.Tests.Layering
{
    /// <summary>
    /// A class that contains test functions for layering of the <see cref="TextBlock"/> class.
    /// </summary>
    [TestFixture]
    [System.ComponentModel.Description("Tests for TextBlock layering")]
    public class TextBlockTests : TextBlock
    {
        private class DummyFont : SpriteFont { }

        /// <summary>
        /// Test the invalidations generated object property changes.
        /// </summary>
        [Test]
        public void TestBasicInvalidations()
        {
            // - test the properties that are supposed to invalidate the object measurement
            UIElementLayeringTests.TestMeasureInvalidation(this, () => Font = new DummyFont());
            Font = null;
            UIElementLayeringTests.TestMeasureInvalidation(this, () => Text = "New Text");

            // - test the properties that are not supposed to invalidate the object layout state
            UIElementLayeringTests.TestNoInvalidation(this, () => TextColor = new Color(1, 2, 3, 4));
        }
    }
}
