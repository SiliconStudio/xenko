// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using NUnit.Framework;

namespace SiliconStudio.Xenko.Particles.Tests
{
    class VisualTestSpawners : GameTest
    {
        public VisualTestSpawners() : base("VisualTestSpawners") { }

        [Test]
        public void RunVisualTests()
        {
            RunGameTest(new VisualTestSpawners());
        }
    }
}
