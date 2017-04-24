// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using NUnit.Framework;

namespace SiliconStudio.Xenko.Particles.Tests
{
    class VisualTestCurves : GameTest
    {
        public VisualTestCurves() : base("VisualTestCurves")
        {
            IndividualTestVersion = 1;  //  Changed the default rotation for paritcles using curve rotation
        }

        [Test]
        public void RunVisualTests()
        {
            RunGameTest(new VisualTestCurves());
        }
    }
}
