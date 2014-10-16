// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using NUnit.Framework;

using SiliconStudio.Paradox.UI.Controls;

namespace SiliconStudio.Paradox.UI.Tests.Layering
{
    /// <summary>
    /// Unit tests for <see cref="Control"/>
    /// </summary>
    class ControlTests : Control
    {
        /// <summary>
        /// Launch all the tests contained in <see cref="ControlTests"/>
        /// </summary>
        public void TestAll()
        {
            TestProperties();
        }

        [Test]
        public void TestProperties()
        {
            var control = new ControlTests();

            // test properties default values
            Assert.AreEqual(Thickness.UniformCuboid(0), control.Padding);
        }
        
        /// <summary>
        /// Test the invalidations generated object property changes.
        /// </summary>
        [Test]
        public void TestBasicInvalidations()
        {
            // - test the properties that are not supposed to invalidate the object layout state

            UIElementLayeringTests.TestMeasureInvalidation(this, () => Padding = Thickness.UniformRectangle(23));
        }
    }
}
