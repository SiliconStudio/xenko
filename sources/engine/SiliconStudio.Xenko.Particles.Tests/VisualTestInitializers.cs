// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using NUnit.Framework;

namespace SiliconStudio.Xenko.Particles.Tests
{
    class VisualTestInitializers : GameTest
    {
        public VisualTestInitializers() : base("VisualTestInitializers") { }      

        [Test]
        public void RunVisualTests()
        {
            RunGameTest(new VisualTestInitializers());
        }
    }
}
