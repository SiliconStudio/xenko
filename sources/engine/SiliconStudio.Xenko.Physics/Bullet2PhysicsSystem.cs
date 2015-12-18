// Copyright (c) 2014-2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Xenko.Games;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SiliconStudio.Xenko.Physics
{
    public class Bullet2PhysicsSystem : GameSystemBase, IPhysicsSystem
    {
        private class PhysicsScene
        {
            public PhysicsProcessor Processor;
            public Simulation Simulation;
        }

        private readonly List<PhysicsScene> scenes = new List<PhysicsScene>();

        static Bullet2PhysicsSystem()
        {
            // Preload proper libbulletc native library (depending on CPU type)
            NativeLibrary.PreloadLibrary("libbulletc.dll");
        }

        public Bullet2PhysicsSystem(IServiceRegistry registry)
            : base(registry)
        {
            UpdateOrder = -1000; //make sure physics runs before everything
            registry.AddService(typeof(IPhysicsSystem), this);

            Enabled = true; //enabled by default
        }

        protected override void Destroy()
        {
            base.Destroy();

            lock (this)
            {
                foreach (var scene in scenes)
                {
                    scene.Simulation.Dispose();
                }
            }
        }

        public Simulation Create(PhysicsProcessor sceneProcessor, PhysicsEngineFlags flags = PhysicsEngineFlags.None)
        {
            var scene = new PhysicsScene { Processor = sceneProcessor, Simulation = new Simulation(sceneProcessor, flags) };
            lock (this)
            {
                scenes.Add(scene);
            }
            return scene.Simulation;
        }

        public void Release(PhysicsProcessor processor)
        {
            lock (this)
            {
                var scene = scenes.SingleOrDefault(x => x.Processor == processor);
                if (scene == null) return;
                scenes.Remove(scene);
                scene.Simulation.Dispose();
            }
        }

        private void Simulate(float deltaTime)
        {
            foreach (var simulation in scenes)
            {
                simulation.Simulation.Simulate(deltaTime);
                simulation.Simulation.ProcessContacts();
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (Simulation.DisableSimulation) return;

            lock (this)
            {
                //read skinned meshes bone positions
                foreach (var physicsScene in scenes)
                {
                    physicsScene.Processor.UpdateBones();
                }

                //simulate, might spawn tasks for multiple simulations
                Simulate((float)gameTime.Elapsed.TotalSeconds);

                //update character bound entity's transforms
                foreach (var physicsScene in scenes)
                {
                    physicsScene.Processor.UpdateCharacters();
                }
            }
        }
    }
}