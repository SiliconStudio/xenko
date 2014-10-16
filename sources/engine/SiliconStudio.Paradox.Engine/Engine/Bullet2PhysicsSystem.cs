// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;
using SiliconStudio.Paradox.Physics;

namespace SiliconStudio.Paradox.Engine
{
    public class Bullet2PhysicsSystem : GameSystem, IPhysicsSystem
    {
        public Bullet2PhysicsSystem(IServiceRegistry registry) : base(registry)
        {
            PhysicsEngine = new PhysicsEngine();
            registry.AddService(typeof(IPhysicsSystem), this);
        }

        public PhysicsEngine PhysicsEngine { get; private set; }

        protected override void Destroy()
        {
            base.Destroy();
            PhysicsEngine.Dispose();
        }
    }
}
