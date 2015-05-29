// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using NUnit.Framework;

using SiliconStudio.Paradox.Graphics.Regression;

namespace SiliconStudio.Paradox.Graphics.Tests
{
    public class TestGraphicsResourceAllocator : GraphicsUnitTestBatch
    {
        [Test]
        public void TestFake()
        {
            RunDrawTest(
                () =>
                {
                    // TODO: Place GraphicsResourceAllocatorTest here
                    // Assert.IsTrue(false);
                });
        }
    }
}