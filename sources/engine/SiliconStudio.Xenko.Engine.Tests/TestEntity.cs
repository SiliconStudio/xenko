// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using NUnit.Framework;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine.Design;

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

            Assert.Catch<InvalidOperationException>(() => entity.Components.Add(new TransformComponent()), $"Cannot add a component of type [{typeof(TransformComponent)}] multiple times");

            // Replace Transform
            var custom = new CustomEntityComponent();
            entity.Components[0] = custom;
            Assert.Null(entity.Transform);
        }
    }

    [DataContract()]
    [DefaultEntityComponentProcessor(typeof(CustomEntityComponentProcessor<CustomEntityComponent>))]
    [AllowMultipleComponent]
    public sealed class CustomEntityComponent : CustomEntityComponentBase
    {
    }

    [DataContract()]
    public abstract class CustomEntityComponentBase : EntityComponent
    {
        [DataMemberIgnore]
        public Action<EntityComponent> ComponentDataGenerated;

        [DataMemberIgnore]
        public Action<EntityComponent> EntityComponentAdded;

        [DataMemberIgnore]
        public Action<EntityComponent> EntityComponentRemoved;
    }
}