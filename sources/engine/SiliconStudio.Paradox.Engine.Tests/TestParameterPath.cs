// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using NUnit.Framework;

using SiliconStudio.Paradox.Rendering;

namespace SiliconStudio.Paradox.Engine.Tests
{
    [TestFixture, Ignore]
    public class TestParameterPath
    {
        private static readonly ParameterKey<ParameterCollection> PropertyContainer = new ParameterKey<ParameterCollection>("ParameterCollection");
        private static readonly ParameterKey<float> PropertyFloat = new ParameterKey<float>("PropertyFloat");

        [Test]
        [Description("Test PropertyListener")]
        public void Test1()
        {
            var container1 = new ParameterCollection("container1");
            var container2 = new ParameterCollection("container2");
            var container3 = new ParameterCollection("container3");
            var container4 = new ParameterCollection("container4");

#pragma warning disable 612 // for ParameterPath and ParameterListener
            var path = new ParameterPath(PropertyContainer, PropertyContainer, PropertyContainer, PropertyFloat);
            var listener = new ParameterListener(container1, path);
#pragma warning restore 612

            int updateCounter = 0;
            object currentValue = null;

            listener.ParameterUpdated += (container, path2, newValue) => { updateCounter++; currentValue = newValue; };

            container1.Set(PropertyContainer, container2);
            container2.Set(PropertyContainer, container2);
            Assert.AreEqual(0, updateCounter);
            Assert.AreEqual(null, currentValue);

            // New value should have been detected (through cycle)
            container2.Set(PropertyFloat, 32.0f);
            Assert.AreEqual(1, updateCounter);
            Assert.AreEqual(32.0f, currentValue);

            // Setting it again should not trigger anything
            container2.Set(PropertyFloat, 32.0f);
            Assert.AreEqual(1, updateCounter);

            // Value should be resetted (because container3 doesn't point on any value)
            container2.Set(PropertyContainer, container3);
            Assert.AreEqual(2, updateCounter);
            Assert.AreEqual(null, currentValue);

            // New value should come from container3->container4
            container3.Set(PropertyContainer, container4);
            container4.Set(PropertyFloat, 48.0f);
            Assert.AreEqual(3, updateCounter);
            Assert.AreEqual(48.0f, currentValue);

            // Value should be again the one from container2
            container2.Set(PropertyContainer, container2);
            Assert.AreEqual(4, updateCounter);
            Assert.AreEqual(32.0f, currentValue);

            // Value should be resetted
            container2.Remove(PropertyFloat);
            Assert.AreEqual(5, updateCounter);
            Assert.AreEqual(null, currentValue);

            container1.Remove(PropertyContainer);
            Assert.AreEqual(5, updateCounter);
            Assert.AreEqual(null, currentValue);
        }
    }
}