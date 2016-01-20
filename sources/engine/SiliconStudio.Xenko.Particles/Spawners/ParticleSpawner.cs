// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Particles.Spawners
{
    public enum SpawnerLoopCondition : byte
    {
        /// <summary>
        /// Looping spawner will loop to the beginning of its Delay state when its Duration is over
        /// If there is no delay it's indistinguishable from LoopingNoDelay
        /// </summary>
        [Display("Looping")]
        Looping,

        /// <summary>
        /// LoopingNoDelay spawner will loop to the beginning of its Active state when its Duration is over
        ///     essentially skipping the Delay state after the first loop
        /// If there is no delay it's indistinguishable from Looping
        /// </summary>
        [Display("Looping, no delay")]
        LoopingNoDelay,

        /// <summary>
        /// OneShot particle spawners will not loop and will only be ative for a period of time equal to its Duration
        /// </summary>
        [Display("One shot")]
        OneShot,

    }

    public enum SpawnerState : byte
    {
        /// <summary>
        /// A spawner in Inactive state hasn't been updated yet. Upon constructing, the Spawner is Inactive,
        ///     but can't return to this state anymore
        /// </summary>
        Inactive,

        /// <summary>
        /// A spawner starts in Rest state and stays in this state for as long as it is delayed.
        /// While in Rest state it doesn't emit particles and switches to Active state after the Rest state is over
        /// </summary>
        Rest,

        /// <summary>
        /// A spawner in Active state emits particles. After the Active state expires, the spawner can switch to
        ///     Rest, Active or Dead state depending on its looping condition.
        /// </summary>
        Active,

        /// <summary>
        /// A spawner in Dead state is not emitting particles and likely not switching to Rest or Active anymore.
        /// </summary>
        Dead,
    }

    [DataContract("ParticleSpawner")]
    public abstract class ParticleSpawner 
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ParticleSpawner"/> is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if enabled; otherwise, <c>false</c>.
        /// </value>
        [DataMember(-10)]
        [DefaultValue(true)]
        public bool Enabled { get; set; } = true;

        [DataMember(5)]
        [Display("Loop")]
        public SpawnerLoopCondition LoopCondition = SpawnerLoopCondition.Looping;

        [DataMember(10)]
        [Display("Delay")]
        public Vector2 Delay = new Vector2(0, 0);

        [DataMember(15)]
        [Display("Duration")]
        public Vector2 Duration = new Vector2(1, 1);

        [DataMemberIgnore]
        protected ParticleEmitter emitter = null;

        [DataMemberIgnore]
        private SpawnerState state = SpawnerState.Inactive;

        [DataMemberIgnore]
        private float stateDuration = 0f;

        internal virtual void RestartSimulation()
        {
            state = SpawnerState.Inactive;
            stateDuration = 0f;
        }

        [DataMemberIgnore]
        private RandomSeed randomSeed = new RandomSeed(0);

        [DataMemberIgnore]
        private UInt32 randomOffset = 0;

        protected void MarkAsDirty()
        {
            if (emitter != null)
            {
                emitter.Dirty = true;
            }
        }

        private float NextFloat()
        {
            return randomSeed.GetFloat(unchecked(randomOffset++));
        }

        private void SwitchToState(SpawnerState newState)
        {
            state = newState;

            if (state == SpawnerState.Active)
            {
                stateDuration = Duration.X + (Duration.Y - Duration.X) * NextFloat();
            }
            else
            if (state == SpawnerState.Rest)
            {
                stateDuration = Delay.X + (Delay.Y - Delay.X) * NextFloat();
            }
            else
            {
                stateDuration = 10f;
            }
        }

        protected SpawnerState GetUpdatedState(float dt, ParticleEmitter emitter)
        {
            // If this is the first time we activate the spawner add it to the emitter list and initialize its random seed
            if (this.emitter == null)
            {
                this.emitter = emitter;
                randomSeed = emitter.RandomSeedGenerator.GetNextSeed();
                emitter.Dirty = true;
            }

            var remainingTime = dt;
            while (stateDuration <= remainingTime)
            {
                remainingTime -= stateDuration;

                switch (state)
                {
                    case SpawnerState.Inactive:
                        SwitchToState(SpawnerState.Rest);
                        break;

                    case SpawnerState.Rest:
                        SwitchToState(SpawnerState.Active);
                        break;

                    case SpawnerState.Dead:
                        SwitchToState(SpawnerState.Dead);
                        break;

                    case SpawnerState.Active:
                        if (LoopCondition == SpawnerLoopCondition.OneShot)
                        {
                            SwitchToState(SpawnerState.Dead);
                            break;
                        }
                        if (LoopCondition == SpawnerLoopCondition.Looping)
                        {
                            SwitchToState(SpawnerState.Rest);
                            break;
                        }

                        SwitchToState(SpawnerState.Active);
                        break;

                    default:
                        SwitchToState(SpawnerState.Dead);
                        break;
                }
            }

            stateDuration -= remainingTime;
            return state;
        }

        public abstract void SpawnNew(float dt, ParticleEmitter emitter);

        public abstract int GetMaxParticlesPerSecond();
    }
}
