using System.Threading;
using System.Threading.Tasks;
using SiliconStudio.Paradox.Animations;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Input;

namespace Xenko.Scripts
{
    public class RunState : StateMachineState
    {
        private AnimationComponent animationComponent;

        public AnimationClip Animation { get; set; }

        public override int StatePriority { get; } = 50;

        public override Task Initialize()
        {
            if (Animation == null) return Task.FromResult(0);

            animationComponent = Entity.GetOrCreate<AnimationComponent>();
            animationComponent.Animations.Add("Run", Animation);

            return Task.FromResult(0);
        }

        public override bool ShouldRun()
        {
            return Input.IsKeyDown(Keys.A) || Input.IsKeyDown(Keys.Left) ||
                   Input.IsKeyDown(Keys.D) || Input.IsKeyDown(Keys.Right) ||
                   Input.IsKeyDown(Keys.W) || Input.IsKeyDown(Keys.Up) ||
                   Input.IsKeyDown(Keys.S) || Input.IsKeyDown(Keys.Down);
        }

        public override async Task<StateMachineState> Run(StateMachineState previouState, CancellationToken cancellation)
        {
            if (Animation == null)
            {
                //Avoid spinning
                await Script.NextFrame();
                return null;
            }

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

                await Script.NextFrame();
            }

            return null;
        }
    }
}