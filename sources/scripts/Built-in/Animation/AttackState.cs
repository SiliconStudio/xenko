using System.Threading;
using System.Threading.Tasks;
using SiliconStudio.Paradox.Animations;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Input;

namespace Xenko.Scripts
{
    public class AttackState : StateMachineState
    {
        private AnimationComponent animationComponent;

        public AnimationClip Animation { get; set; }

        public override int StatePriority { get; } = 0;

        public override Task Initialize()
        {
            if (Animation == null) return Task.FromResult(0);

            animationComponent = Entity.GetOrCreate<AnimationComponent>();
            animationComponent.Animations.Add("Attack", Animation);

            return Task.FromResult(0);
        }

        public override bool ShouldRun()
        {
            return Input.IsKeyDown(Keys.Space);
        }

        private Task Shoot()
        {
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