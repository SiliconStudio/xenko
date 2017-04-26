// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

using NUnit.Framework;

using SiliconStudio.Xenko.UI.Controls;
using SiliconStudio.Xenko.UI.Events;

namespace SiliconStudio.Xenko.UI.Tests.Events
{
    /// <summary>
    /// class used to test <see cref="RoutedEventArgs"/>
    /// </summary>
    public class RoutedEventArgsTest : RoutedEventArgs
    {
        /// <summary>
        /// Launch all tests of the class
        /// </summary>
        public void TestAll()
        {
            TestEventFreezing();
        }

        /// <summary>
        /// Test that when the event is marked as routed its values cannot be modified any more
        /// </summary>
        [Test]
        public void TestEventFreezing()
        {
            // check that values can freely be modified by default
            Assert.AreEqual(false, IsBeingRouted);
            var image = new ImageElement();
            var routedEvent = EventManager.RegisterRoutedEvent<RoutedEventArgs>("test", RoutingStrategy.Tunnel, typeof(RoutedEventArgsTest));
            Source = image;
            Assert.AreEqual(image, Source);
            Source = null;
            Assert.AreEqual(null, Source);
            RoutedEvent = routedEvent;
            Assert.AreEqual(routedEvent, RoutedEvent);
            RoutedEvent = null;
            Assert.AreEqual(null, RoutedEvent);

            // check that value of IsBeingRouted is updated
            StartEventRouting();
            Assert.AreEqual(true, IsBeingRouted);

            // check that modifications are now prohibited
            Assert.Throws<InvalidOperationException>(() => Source = null);
            Assert.Throws<InvalidOperationException>(() => RoutedEvent = null);

            // check that value of IsBeingRouted is update
            EndEventRouting();
            Assert.AreEqual(false, IsBeingRouted);
        }
    }
}
