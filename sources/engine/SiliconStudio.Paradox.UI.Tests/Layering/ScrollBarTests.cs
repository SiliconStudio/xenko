// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using NUnit.Framework;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.UI.Controls;

namespace SiliconStudio.Paradox.UI.Tests.Layering
{
    /// <summary>
    /// A class that contains test functions for layering of the <see cref="ScrollBar"/> class.
    /// </summary>
    [TestFixture]
    [System.ComponentModel.Description("Tests for ScrollBar layering")]
    public class ScrollBarTests : ScrollBar
    {
        /// <summary>
        /// Test the invalidations generated object property changes.
        /// </summary>
        [Test]
        public void TestBasicInvalidations()
        {
            // - test the properties that are not supposed to invalidate the object layout state
            UIElementLayeringTests.TestNoInvalidation(this, () => BarColor = new Color(1,2,3,4));
        }
    }
}