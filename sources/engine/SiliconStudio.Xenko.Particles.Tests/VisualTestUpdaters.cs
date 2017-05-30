// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using NUnit.Framework;

namespace SiliconStudio.Xenko.Particles.Tests
{
    class VisualTestUpdaters : GameTest
    {
        public VisualTestUpdaters() : base("VisualTestUpdaters") { }

        [Test]
        public void RunVisualTests()
        {
            RunGameTest(new VisualTestUpdaters());
        }
    }
}
