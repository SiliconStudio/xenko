using System;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Input;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Animations;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Physics;
using SiliconStudio.Paradox.Rendering.Sprites;
using SpriteStudioDemo18;

namespace Xenko.Scripts
{
    public class StateMachine : AsyncScript
    {
        private readonly Dictionary<Type, StateMachineState> states = new Dictionary<Type, StateMachineState>();

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
                    states.Add(machineState.GetType(), machineState);
                    machineState.StateMachine = this;
                }
            }

            foreach (var stateMachineState in states)
            {
                stateMachineState.Value.Initialize();
            }

            currentState = states.First().Value;
            currentState.Begin(null);

            while (Game.IsRunning)
            {
                if (currentState.ShouldEnd() && currentState.Next() != null)
                {
                    var nextState = currentState.Next();
                    var prevState = currentState;
                    currentState.End(nextState);

                    currentState = nextState;
                    currentState.Begin(prevState);
                }

                currentState.Update();

                await Script.NextFrame();
            }
        }

        public StateMachineState GetState(Type stateType)
        {
            return states[stateType];
        }
    }

    public abstract class StateMachineState : AsyncScript
    {
        internal StateMachine StateMachine { get; set; }

        public override Task Execute()
        {
            return Task.FromResult(0);
        }

        public abstract void Initialize();

        public abstract bool ShouldBegin();

        public abstract bool ShouldEnd();

        public abstract void Begin(StateMachineState previouState);

        public abstract void End(StateMachineState nextState);

        public abstract void Update();

        public abstract StateMachineState Next();
    }

    public class IdleState : StateMachineState
    {
        private AnimationComponent animationComponent;

        private StateMachineState atkState, runState;

        public override void Initialize()
        {
            animationComponent = Entity.Get<AnimationComponent>();
            atkState = StateMachine.GetState(typeof(AttackState));
            runState = StateMachine.GetState(typeof(RunState));
        }

        public override bool ShouldBegin()
        {
            return !ShouldEnd();
        }

        public override bool ShouldEnd()
        {
            return Input.IsKeyDown(Keys.A) || Input.IsKeyDown(Keys.Left) ||
                   Input.IsKeyDown(Keys.D) || Input.IsKeyDown(Keys.Right) ||
                   Input.IsKeyDown(Keys.Space);
        }

        public override void Begin(StateMachineState previouState)
        {
            for (var index = 0; index < animationComponent.PlayingAnimations.Count; index++)
            {
                var animation = animationComponent.PlayingAnimations[index];
                if (animation.Name == "Stance") return;
            }
            animationComponent.Play("Stance");
        }

        public override void End(StateMachineState nextState)
        {           
        }

        public override void Update()
        {            
        }

        public override StateMachineState Next()
        {
            if(atkState.ShouldBegin()) return atkState;
            if(runState.ShouldBegin()) return runState;
            return null;
        }
    }

    public class RunState : StateMachineState
    {
        private AnimationComponent animationComponent;

        private StateMachineState atkState, idlState;

        private const int AgentMoveDistance = 10; // virtual resolution unit/second
        private const float gameWidthX = 16f;       // from -8f to 8f
        private const float gameWidthHalfX = gameWidthX / 2f;
        private float baseScaleX;

        public override void Initialize()
        {
            animationComponent = Entity.Get<AnimationComponent>();
            atkState = StateMachine.GetState(typeof(AttackState));
            idlState = StateMachine.GetState(typeof(IdleState));
            baseScaleX = Entity.Transform.Scale.X;
        }

        public override bool ShouldBegin()
        {
            return !ShouldEnd();
        }

        public override bool ShouldEnd()
        {
            return !Input.IsKeyDown(Keys.A) && !Input.IsKeyDown(Keys.Left) &&
                   !Input.IsKeyDown(Keys.D) && !Input.IsKeyDown(Keys.Right);
        }

        public override void Begin(StateMachineState previouState)
        {
            for (var index = 0; index < animationComponent.PlayingAnimations.Count; index++)
            {
                var animation = animationComponent.PlayingAnimations[index];
                if (animation.Name == "Run") return;
            }
            animationComponent.Play("Run");
        }

        public override void End(StateMachineState nextState)
        {
        }

        public override void Update()
        {
            // Update Agent's position
            var dt = (float)Game.UpdateTime.Elapsed.TotalSeconds;

            Entity.Transform.Position.X += ((Input.IsKeyDown(Keys.D) || Input.IsKeyDown(Keys.Right)) ? AgentMoveDistance : -AgentMoveDistance) * dt;

            if (Entity.Transform.Position.X < -gameWidthHalfX)
                Entity.Transform.Position.X = -gameWidthHalfX;

            if (Entity.Transform.Position.X > gameWidthHalfX)
                Entity.Transform.Position.X = gameWidthHalfX;

            // If agent face left, flip the sprite
            Entity.Transform.Scale.X = (Input.IsKeyDown(Keys.D) || Input.IsKeyDown(Keys.Right)) ? baseScaleX : -baseScaleX;
        }

        public override StateMachineState Next()
        {
            if (atkState.ShouldBegin()) return atkState;
            return idlState;
        }
    }

    public class AttackState : StateMachineState
    {
        private AnimationComponent animationComponent;

        private StateMachineState runState, idlState;

        private Task animationTask;

        public SpriteSheet BulletSheet { get; set; }

        public PhysicsColliderShape BulletColliderShape { get; set; }

        private readonly Vector3 bulletOffset = new Vector3(1.3f, 1.65f, 0f);

        public override void Initialize()
        {
            animationComponent = Entity.Get<AnimationComponent>();
            runState = StateMachine.GetState(typeof(AttackState));
            idlState = StateMachine.GetState(typeof(IdleState));
        }

        public override bool ShouldBegin()
        {
            return Input.IsKeyDown(Keys.Space);
        }

        public override bool ShouldEnd()
        {
            if (animationTask != null && !animationTask.IsCompleted) return false;
            return !ShouldBegin();
        }

        private void Shoot()
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
                if (animation.Name == "Attack") return;
            }
            var anim = animationComponent.Play("Attack");
            animationTask = anim.Ended();
        }

        public override void Begin(StateMachineState previouState)
        {
            Shoot();
        }

        public override void End(StateMachineState nextState)
        {
        }

        public override void Update()
        {
            if (Input.IsKeyDown(Keys.Space))
            {
                if (animationTask != null && animationTask.IsCompleted)
                {
                    Shoot();
                }
            }
        }

        public override StateMachineState Next()
        {
            if (runState.ShouldBegin()) return runState;
            return idlState;
        }
    }
}