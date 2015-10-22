// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using NUnit.Framework;

using SiliconStudio.Core.Mathematics;
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
        /// <summary>
        /// Test the invalidations generated object property changes.
        /// </summary>
        [Test]
        public void TestBasicInvalidations()
        {
            // - test the properties that are supposed to invalidate the object measurement
            UIElementLayeringTests.TestMeasureInvalidation(this, () => Font = null);
            UIElementLayeringTests.TestMeasureInvalidation(this, () => Text = "New Text");

            // - test the properties that are not supposed to invalidate the object layout state
            UIElementLayeringTests.TestNoInvalidation(this, () => TextColor = new Color(1, 2, 3, 4));
        }
    }
}