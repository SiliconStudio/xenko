// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using NUnit.Framework;

namespace SiliconStudio.Xenko.Particles.Tests
{
    class VisualTestRibbons : GameTest
    {
        public VisualTestRibbons() : base("VisualTestRibbons")
        {
            IndividualTestVersion = 2;  // Negligible, but consistent differences in the two images (~4-5 pixels total)
        }

        [Test]
        public void RunVisualTests()
        {
            RunGameTest(new VisualTestRibbons());
        }
    }
}
