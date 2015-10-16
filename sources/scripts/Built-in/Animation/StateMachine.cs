using SiliconStudio.Paradox.Engine;
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
                if (nextState != null)
                {
                    previousState = currentState;
                    currentState = nextState;
                }
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
}