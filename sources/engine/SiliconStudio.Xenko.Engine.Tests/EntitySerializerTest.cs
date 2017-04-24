// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;

using NUnit.Framework;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics.Regression;

namespace SiliconStudio.Xenko.Engine.Tests
{
    class EntitySerializerTest : GameTestBase
    {
        [Test]
        public void TestSaveAndLoadEntities()
        {
            PerformTest(game =>
            {
                var entity = new Entity { Transform = { Position = new Vector3(100.0f, 0.0f, 0.0f) } };
                game.Content.Save("EntityAssets/Entity", entity);

                GC.Collect();

                var entity2 = game.Content.Load<Entity>("EntityAssets/Entity");
                Assert.AreEqual(entity.Transform.Position, entity2.Transform.Position);
            });
        }
    }
}
