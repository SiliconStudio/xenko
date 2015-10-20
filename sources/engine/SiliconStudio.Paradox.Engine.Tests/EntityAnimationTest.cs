// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using SiliconStudio.Core.Updater;

namespace SiliconStudio.Paradox.Engine.Tests
{
    [TestFixture]
    public class EntityAnimationTest
    {
        [Test]
        public void TestComponentAccess()
        {
            var entity = new Entity();

            entity.AddChild(new Entity("child1")
            {
                new LightComponent()
            });

            var compiledUpdate = UpdateEngine.Compile(typeof(Entity), new List<UpdateMemberInfo>
            {
                new UpdateMemberInfo("chidl1.(LightComponent.Key)", 0)
            });

            UpdateEngine.Run(entity, compiledUpdate, IntPtr.Zero, null);
        }
    }
}