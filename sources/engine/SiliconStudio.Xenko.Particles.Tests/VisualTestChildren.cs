// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using NUnit.Framework;

namespace SiliconStudio.Xenko.Particles.Tests
{
    class VisualTestChildren : GameTest
    {
        public VisualTestChildren() : base("VisualTestChildren")
        {
            IndividualTestVersion = 1;  //  Changes in particle spawning
            IndividualTestVersion += 4;  //  Changed to avoid collisions with 1.10
        }

        [Test]
        public void RunVisualTests()
        {
            RunGameTest(new VisualTestChildren());
        }
    }
}
