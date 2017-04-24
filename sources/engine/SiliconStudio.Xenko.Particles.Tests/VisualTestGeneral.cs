// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using NUnit.Framework;

namespace SiliconStudio.Xenko.Particles.Tests
{
    class VisualTestGeneral : GameTest
    {
        public VisualTestGeneral() : base("VisualTestGeneral")
        {
            IndividualTestVersion = 1;  //  Changes in particle spawning
        }

        [Test]
        public void RunVisualTests()
        {
            RunGameTest(new VisualTestGeneral());
        }
    }
}
