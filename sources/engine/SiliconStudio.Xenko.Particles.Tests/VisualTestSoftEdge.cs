// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using NUnit.Framework;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Particles.Tests
{
    class VisualTestSoftEdge : GameTest
    {
        public VisualTestSoftEdge() : base("VisualTestSoftEdge") { }

        [Test]
        public void RunVisualTests10()
        {
            RunGameTest(new GameTest("VisualTestSoftEdge", GraphicsProfile.Level_10_0));
        }

        [Test]
        public void RunVisualTests11()
        {
            RunGameTest(new GameTest("VisualTestSoftEdge", GraphicsProfile.Level_11_0));
        }

    }
}
