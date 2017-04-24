// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using NUnit.Framework;

using SiliconStudio.Xenko.UI.Controls;

namespace SiliconStudio.Xenko.UI.Tests.Layering
{
    /// <summary>
    /// A class that contains test functions for layering of the <see cref="ImageButton"/> class.
    /// </summary>
    [TestFixture, Ignore("ImageButton is deprecated")]
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
