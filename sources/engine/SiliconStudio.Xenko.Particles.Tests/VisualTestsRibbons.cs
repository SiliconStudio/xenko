// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using NUnit.Framework;

namespace SiliconStudio.Xenko.Particles.Tests
{
    class VisualTestRibbons : GameTest
    {
        public VisualTestRibbons() : base("VisualTestRibbons")
        {
            IndividualTestVersion = 2;  // Negligible, but consistent differences in the two images (~4-5 pixels total)
            IndividualTestVersion += 4;  //  Changed to avoid collisions with 1.10
        }

        [Test]
        public void RunVisualTests()
        {
            RunGameTest(new VisualTestRibbons());
        }
    }
}
