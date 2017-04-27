// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Collections.Generic;
using NUnit.Framework;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Rendering.LightProbes;

namespace SiliconStudio.Xenko.Engine.Tests
{
    [TestFixture]
    public class TestBowyerWatsonTetrahedralization
    {
        [Test]
        public void TestCube()
        {
            // Build cube from (0,0,0) to (1,1,1)
            var positions = new FastList<Vector3>();
            for (int i = 0; i < 8; ++i)
            {
                positions.Add(new Vector3
                {
                    X = i % 2 == 0 ? 0.0f : 1.0f,
                    Y = i / 4 == 0 ? 0.0f : 1.0f,
                    Z = (i / 2) % 2 == 0 ? 0.0f : 1.0f,
                });
            }

            var tetra = new BowyerWatsonTetrahedralization();
            var tetraResult = tetra.Compute(positions);
        }
    }
}
