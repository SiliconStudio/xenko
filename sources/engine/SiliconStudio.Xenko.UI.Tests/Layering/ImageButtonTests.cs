// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using NUnit.Framework;

using SiliconStudio.Xenko.UI.Controls;

namespace SiliconStudio.Xenko.UI.Tests.Layering
{
    /// <summary>
    /// A class that contains test functions for layering of the <see cref="ImageButton"/> class.
    /// </summary>
    [TestFixture]
    [System.ComponentModel.Description("Tests for ImageButton layering")]
    public class ImageButtonTests : ImageButton
    {
        [Test]
        public void TestProperties()
        {
            var control = new ImageButton();

            // test properties default values
            Assert.AreEqual(new Thickness(0, 0, 0, 0), control.Padding);
        }
    }
}