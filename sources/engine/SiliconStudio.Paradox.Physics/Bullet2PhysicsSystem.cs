// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Paradox.Physics
{
    public class Bullet2PhysicsSystem : GameSystem, IPhysicsSystem
    {
        public Bullet2PhysicsSystem(Game game) : base(game.Services)
        {
            PhysicsEngine = new PhysicsEngine(game);
            game.Services.AddService(typeof(IPhysicsSystem), this);
            game.GameSystems.Add(this);
        }

        public PhysicsEngine PhysicsEngine { get; private set; }

        protected override void Destroy()
        {
            base.Destroy();
            PhysicsEngine.Dispose();
        }
    }
}
