// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using NUnit.Framework;

using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Sprites;
using SiliconStudio.Xenko.UI.Controls;

namespace SiliconStudio.Xenko.UI.Tests.Layering
{
    /// <summary>
    /// A class that contains test functions for layering of the <see cref="Button"/> class.
    /// </summary>
    [TestFixture]
    [System.ComponentModel.Description("Tests for Button layering")]
    public class ButtonTests : Button
    {
        [Test]
        public void TestProperties()
        {
            var control = new Button();

            // test properties default values
            Assert.AreEqual(new Thickness(10, 5, 10, 7), control.Padding);
        }

        /// <summary>
        /// Test the invalidations generated object property changes.
        /// </summary>
        [Test]
        public void TestBasicInvalidations()
        {
            // - test the properties that are not supposed to invalidate the object layout state
            UIElementLayeringTests.TestNoInvalidation(this, () => PressedImage = (SpriteFromTexture)new Sprite());
            UIElementLayeringTests.TestNoInvalidation(this, () => NotPressedImage = (SpriteFromTexture)new Sprite());
        }
    }
}
