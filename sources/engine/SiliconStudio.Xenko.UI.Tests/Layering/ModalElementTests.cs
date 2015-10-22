// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using NUnit.Framework;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.UI.Controls;

namespace SiliconStudio.Paradox.UI.Tests.Layering
{
    /// <summary>
    /// A class that contains test functions for layering of the <see cref="ModalElement"/> class.
    /// </summary>
    [TestFixture]
    [System.ComponentModel.Description("Tests for ModalElement layering")]
    public class ModalElementTests : ModalElement
    {
        /// <summary>
        /// Test the invalidations generated object property changes.
        /// </summary>
        [Test]
        public void TestBasicInvalidations()
        {
            // - test the properties that are not supposed to invalidate the object layout state
            UIElementLayeringTests.TestNoInvalidation(this, () => OverlayColor = new Color(1, 2, 3, 4));
            UIElementLayeringTests.TestNoInvalidation(this, () => IsModal = !IsModal);
        }
    }
}