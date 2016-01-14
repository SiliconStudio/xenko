// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using NUnit.Framework;

namespace SiliconStudio.Xenko.Engine.Tests
{
    [TestFixture]
    public class TestEntity
    {
        [Test]
        public void TestTransformComponent()
        {
            var entity = new Entity();

            // Make sure that an entity has a transform component
            Assert.NotNull(entity.Transform);
            Assert.AreEqual(1, entity.Components.Count);
            Assert.AreEqual(entity.Transform, entity.Components[0]);

            // Remove Transform
            entity.Components.RemoveAt(0);
            Assert.Null(entity.Transform);

            // Readd transform
            var transform = new TransformComponent();
            entity.Components.Add(transform);
            Assert.NotNull(entity.Transform);

            Assert.Catch<InvalidOperationException>(() => entity.Components.Add(new TransformComponent()));



        }



    }
}