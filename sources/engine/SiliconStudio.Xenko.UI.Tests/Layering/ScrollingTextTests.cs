// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using NUnit.Framework;

using SiliconStudio.Xenko.UI.Controls;

namespace SiliconStudio.Xenko.UI.Tests.Layering
{
    /// <summary>
    /// A class that contains test functions for layering of the <see cref="ScrollingText"/> class.
    /// </summary>
    [TestFixture]
    [System.ComponentModel.Description("Tests for ScrollingText layering")]
    public class ScrollingTextTests : ScrollingText
    {
        /// <summary>
        /// Test the invalidations generated object property changes.
        /// </summary>
        [Test]
        public void TestBasicInvalidations()
        {
            // - test the properties that are supposed to invalidate the object measurement
            UIElementLayeringTests.TestMeasureInvalidation(this, () => DesiredCharacterNumber = 99);

            // - test the properties that are not supposed to invalidate the object layout state
            UIElementLayeringTests.TestNoInvalidation(this, () => ScrollingSpeed = 5.5f);
            UIElementLayeringTests.TestNoInvalidation(this, () => RepeatText = !RepeatText);
        }
    }
}
