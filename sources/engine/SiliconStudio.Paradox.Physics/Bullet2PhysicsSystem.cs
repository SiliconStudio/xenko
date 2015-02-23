// Copyright (c) 2014-2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Paradox.Games;

namespace SiliconStudio.Paradox.Physics
{
    public class Bullet2PhysicsSystem : GameSystemBase, IPhysicsSystem
    {
        internal readonly ConcurrentDictionary<string, Simulation> PhysicsEngines = new ConcurrentDictionary<string, Simulation>();

        private readonly PhysicsProcessor processor;

        public Bullet2PhysicsSystem(Game game)
            : base(game.Services)
        {
            UpdateOrder = -1000; //make sure physics runs before everything
            game.Services.AddService(typeof(IPhysicsSystem), this);
            game.GameSystems.Add(this);

            Simulation.Initialize(game);
            Enabled = true; //enabled by default
        }

        protected override void Destroy()
        {
            base.Destroy();

            foreach (var physicsEngine in PhysicsEngines)
            {
                physicsEngine.Value.Dispose();
            }
        }

        public Simulation Create(string spaceNameTag = "default", PhysicsEngineFlags flags = PhysicsEngineFlags.None)
        {
            var newEngine = new Simulation(flags);
            PhysicsEngines.TryAdd(spaceNameTag, newEngine);
            Simulation.CacheContacts = PhysicsEngines.Count > 1;
            return PhysicsEngines[spaceNameTag];
        }

        public void Release(string spaceNameTag = "default")
        {
            Simulation engine;
            if (PhysicsEngines.TryRemove(spaceNameTag, out engine)) engine.Dispose();
            Simulation.CacheContacts = PhysicsEngines.Count > 1;
        }

        private void Simulate(float deltaTime)
        {
            var engines = PhysicsEngines.Values;

            if (engines.Count == 1)
            {
                var engine = engines.First();
                engine.Simulate(deltaTime);
            }
            else if (engines.Count > 1)
            {
                var simulationTasks = engines.Select(simulation1 => Task.Run(() => simulation1.Simulate(deltaTime))).ToArray();
                Task.WaitAll(simulationTasks);

                //in this case contacts are cached so we proccess after simulations are done
                foreach (var simulation in engines)
                {
                    simulation.ProcessContacts();
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            //read skinned meshes bone positions
            processor.UpdateBones();

            //simulate, can might spawn tasks for multiple simulations
            Simulate((float)gameTime.Elapsed.TotalSeconds);
            
            //update character bound entity's transforms
            processor.UpdateCharacters();
        }
    }
}