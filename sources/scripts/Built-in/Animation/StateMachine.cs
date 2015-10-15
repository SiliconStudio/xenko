using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Input;
using SiliconStudio.Paradox.Physics;
using SiliconStudio.Paradox.Rendering.Sprites;
using SpriteStudioDemo18;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Xenko.Scripts
{
    public class StateMachine : AsyncScript
    {
        private List<StateMachineState> states = new List<StateMachineState>();

        private StateMachineState currentState;

        public override async Task Execute()
        {
            var scripts = Entity.Get<ScriptComponent>();
            for (var index = 0; index < scripts.Scripts.Count; index++)
            {
                var script = scripts.Scripts[index];
                var machineState = script as StateMachineState;
                if (machineState != null)
                {
                    states.Add(machineState);
                    machineState.StateMachine = this;
                }
            }

            states = states.OrderBy(x => x.StatePriority).ToList();

            foreach (var stateMachineState in states)
            {
                await stateMachineState.Initialize();
            }

            currentState = states.First();
            StateMachineState previousState = null;

            while (!CancellationToken.IsCancellationRequested)
            {
                var nextState = await currentState.Run(previousState, CancellationToken);
                previousState = currentState;
                currentState = nextState;
            }
        }

        public bool Checkpoint(out StateMachineState nextState)
        {
            nextState = null;

            foreach (var state in states)
            {
                if (currentState == state) continue;
                if (!state.ShouldRun()) continue;
                nextState = state;
                return true;
            }

            return false;
        }
    }

    public abstract class StateMachineState : AsyncScript
    {
        /// <summary>
        /// The priority (lower value = higher priority) of this state machine over others
        /// </summary>
        public abstract int StatePriority { get; }

        internal StateMachine StateMachine { get; set; }

        /// <summary>
        /// not really used
        /// </summary>
        /// <returns></returns>
        public override Task Execute()
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// Initialization tasks go here.
        /// </summary>
        /// <returns></returns>
        public abstract Task Initialize();

        /// <summary>
        /// Checks if this state is valid.
        /// </summary>
        /// <returns>If this state is valid and should begin.</returns>
        public abstract bool ShouldRun();

        /// <summary>
        /// Run this state until the end.
        /// </summary>
        /// <returns>The next state.</returns>
        public abstract Task<StateMachineState> Run(StateMachineState previouState, CancellationToken cancellation);
    }

    public class IdleState : StateMachineState
    {
        private AnimationComponent animationComponent;

        public override int StatePriority { get; } = 100;

        public override Task Initialize()
        {
            animationComponent = Entity.Get<AnimationComponent>();
            return Task.FromResult(0);
        }

        public override bool ShouldRun()
        {
            return !Input.IsKeyDown(Keys.A) && !Input.IsKeyDown(Keys.Left) &&
                   !Input.IsKeyDown(Keys.D) && !Input.IsKeyDown(Keys.Right) &&
                   !Input.IsKeyDown(Keys.Space);
        }

        public override async Task<StateMachineState> Run(StateMachineState previouState, CancellationToken cancellation)
        {
            var playing = false;
            for (var index = 0; index < animationComponent.PlayingAnimations.Count; index++)
            {
                var animation = animationComponent.PlayingAnimations[index];
                if (animation.Name == "Stance") playing = true;
            }
            if (!playing) animationComponent.Play("Stance");

            while (!cancellation.IsCancellationRequested)
            {
                StateMachineState nextState;
                if (StateMachine.Checkpoint(out nextState))
                {
                    return nextState;
                }

                await Script.NextFrame();
            }

            return null;
        }
    }

    public class RunState : StateMachineState
    {
        private AnimationComponent animationComponent;

        private const int AgentMoveDistance = 10; // virtual resolution unit/second
        private const float gameWidthX = 16f;       // from -8f to 8f
        private const float gameWidthHalfX = gameWidthX / 2f;
        private float baseScaleX;

        public override int StatePriority { get; } = 50;

        public override Task Initialize()
        {
            animationComponent = Entity.Get<AnimationComponent>();
            baseScaleX = Entity.Transform.Scale.X;
            return Task.FromResult(0);
        }

        public override bool ShouldRun()
        {
            return Input.IsKeyDown(Keys.A) || Input.IsKeyDown(Keys.Left) ||
                   Input.IsKeyDown(Keys.D) || Input.IsKeyDown(Keys.Right);
        }

        public override async Task<StateMachineState> Run(StateMachineState previouState, CancellationToken cancellation)
        {
            var playing = false;
            for (var index = 0; index < animationComponent.PlayingAnimations.Count; index++)
            {
                var animation = animationComponent.PlayingAnimations[index];
                if (animation.Name == "Run") playing = true;
            }
            if (!playing) animationComponent.Play("Run");

            while (!cancellation.IsCancellationRequested)
            {
                StateMachineState nextState;
                if (StateMachine.Checkpoint(out nextState))
                {
                    return nextState;
                }

                // Update Agent's position
                var dt = (float)Game.UpdateTime.Elapsed.TotalSeconds;

                Entity.Transform.Position.X += ((Input.IsKeyDown(Keys.D) || Input.IsKeyDown(Keys.Right))
                    ? AgentMoveDistance
                    : -AgentMoveDistance) * dt;

                if (Entity.Transform.Position.X < -gameWidthHalfX)
                    Entity.Transform.Position.X = -gameWidthHalfX;

                if (Entity.Transform.Position.X > gameWidthHalfX)
                    Entity.Transform.Position.X = gameWidthHalfX;

                // If agent face left, flip the sprite
                Entity.Transform.Scale.X = (Input.IsKeyDown(Keys.D) || Input.IsKeyDown(Keys.Right))
                    ? baseScaleX
                    : -baseScaleX;

                await Script.NextFrame();
            }
            return null;
        }
    }

    public class AttackState : StateMachineState
    {
        private AnimationComponent animationComponent;

        public SpriteSheet BulletSheet { get; set; }

        public PhysicsColliderShape BulletColliderShape { get; set; }

        private readonly Vector3 bulletOffset = new Vector3(1.3f, 1.65f, 0f);

        public override int StatePriority { get; } = 0;

        public override Task Initialize()
        {
            animationComponent = Entity.Get<AnimationComponent>();
            return Task.FromResult(0);
        }

        public override bool ShouldRun()
        {
            return Input.IsKeyDown(Keys.Space);
        }

        private Task Shoot()
        {
            var rb = new RigidbodyElement { CanCollideWith = CollisionFilterGroupFlags.CustomFilter1, CollisionGroup = CollisionFilterGroups.DefaultFilter };
            rb.ColliderShapes.Add(new ColliderShapeAssetDesc { Shape = BulletColliderShape });

            // Spawns a new bullet
            var bullet = new Entity
                    {
                        new SpriteComponent { SpriteProvider = new SpriteFromSheet {Sheet = BulletSheet}, CurrentFrame = BulletSheet.FindImageIndex("bullet") },
                        new PhysicsComponent { Elements = { rb } },
                        new ScriptComponent { Scripts = { new BeamScript() }}
                    };
            bullet.Name = "bullet";

            bullet.Transform.Position = Entity.Transform.Scale.X > 0.0f ? Entity.Transform.Position + bulletOffset : Entity.Transform.Position + (bulletOffset * new Vector3(-1, 1, 1));
            bullet.Transform.UpdateWorldMatrix();

            SceneSystem.SceneInstance.Scene.Entities.Add(bullet);

            rb.RigidBody.LinearFactor = new Vector3(1, 0, 0);
            rb.RigidBody.AngularFactor = new Vector3(0, 0, 0);
            rb.RigidBody.ApplyImpulse(Entity.Transform.Scale.X > 0.0f ? new Vector3(25, 0, 0) : new Vector3(-25, 0, 0));

            for (var index = 0; index < animationComponent.PlayingAnimations.Count; index++)
            {
                var animation = animationComponent.PlayingAnimations[index];
                if (animation.Name == "Attack") return animation.Ended();
            }
            var anim = animationComponent.Play("Attack");
            return anim.Ended();
        }

        public override async Task<StateMachineState> Run(StateMachineState previouState, CancellationToken cancellation)
        {
            while (!cancellation.IsCancellationRequested)
            {
                if (!Input.IsKeyDown(Keys.Space))
                {
                    StateMachineState nextState;
                    if (StateMachine.Checkpoint(out nextState))
                    {
                        return nextState;
                    }
                }

                await Shoot();
            }
            return null;
        }
    }
}