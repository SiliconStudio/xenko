// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using NUnit.Framework;

namespace SiliconStudio.Core.Mathematics.Tests
{
    [TestFixture]
    public class TestColor
    {
        [Test]
        public void TestRGB2HSVConvertion()
        {
            Assert.AreEqual(new ColorHSV(312, 1, 1, 1), ColorHSV.FromColor(new Color4(1, 0, 0.8f, 1)));
            Assert.AreEqual(new ColorHSV(0, 0, 0, 1), ColorHSV.FromColor(Color.Black));
            Assert.AreEqual(new ColorHSV(0, 0, 1, 1), ColorHSV.FromColor(Color.White));
            Assert.AreEqual(new ColorHSV(0, 1, 1, 1), ColorHSV.FromColor(Color.Red));
            Assert.AreEqual(new ColorHSV(120, 1, 1, 1), ColorHSV.FromColor(Color.Lime));
            Assert.AreEqual(new ColorHSV(240, 1, 1, 1), ColorHSV.FromColor(Color.Blue));
            Assert.AreEqual(new ColorHSV(60, 1, 1, 1), ColorHSV.FromColor(Color.Yellow));
            Assert.AreEqual(new ColorHSV(180, 1, 1, 1), ColorHSV.FromColor(Color.Cyan));
            Assert.AreEqual(new ColorHSV(300, 1, 1, 1), ColorHSV.FromColor(Color.Magenta));
            Assert.AreEqual(new ColorHSV(0, 0, 0.7529412f, 1), ColorHSV.FromColor(Color.Silver));
            Assert.AreEqual(new ColorHSV(0, 0, 0.5019608f, 1), ColorHSV.FromColor(Color.Gray));
            Assert.AreEqual(new ColorHSV(0, 1, 0.5019608f, 1), ColorHSV.FromColor(Color.Maroon));
        }

        [Test]
        public void TestHSV2RGBConvertion()
        {
            Assert.AreEqual(Color.Black.ToColor4(), ColorHSV.FromColor(Color.Black).ToColor());
            Assert.AreEqual(Color.White.ToColor4(), ColorHSV.FromColor(Color.White).ToColor());
            Assert.AreEqual(Color.Red.ToColor4(), ColorHSV.FromColor(Color.Red).ToColor());
            Assert.AreEqual(Color.Lime.ToColor4(), ColorHSV.FromColor(Color.Lime).ToColor());
            Assert.AreEqual(Color.Blue.ToColor4(), ColorHSV.FromColor(Color.Blue).ToColor());
            Assert.AreEqual(Color.Silver.ToColor4(), ColorHSV.FromColor(Color.Silver).ToColor());
            Assert.AreEqual(Color.Maroon.ToColor4(), ColorHSV.FromColor(Color.Maroon).ToColor());
        }
    }
}
