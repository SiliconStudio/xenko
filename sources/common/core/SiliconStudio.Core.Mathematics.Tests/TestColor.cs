// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using NUnit.Framework;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Core.Tests
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
    }
}