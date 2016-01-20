// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Particles.Components
{
    /// <summary>
    /// Edit-time controls for the <see cref="ParticleSystemComponent"/>
    /// </summary>
    [DataContract("ParticleSystemControl")]
    public class ParticleSystemControl
    {
        [DataMember(10)]
        [Display("Loop endlessly")]
        public bool EnableLooping { get; set; } = true;

        [DataMember(20)]
        [Display("Reset after (seconds)")]
        public float ResetSeconds
        {
            get { return resetSeconds; }
            set
            {
                if (value >= 0.5f)
                    resetSeconds = value;
            }
        }

        [DataMember(30)]
        public StateControl Control = StateControl.Play;

        [DataMemberIgnore]
        private StateControl oldControl = StateControl.Play;

        [DataMemberIgnore]
        public float resetSeconds = 5f;

        [DataMemberIgnore]
        private float totalElapsedTime = 0f;

        [DataMemberIgnore]
        private float currentElapsedTime = 0f;

        public void Update(float dt, ParticleSystem particleSystem)
        {
            // Check if state has changed
            if (oldControl != Control)
            {
                switch (Control)
                {
                    case StateControl.Play:
                        particleSystem.Play();
                        break;

                    case StateControl.Pause:
                        particleSystem.Pause();
                        break;

                    case StateControl.Stop:
                        particleSystem.Stop();
                        break;
                }

                oldControl = Control;
            }

            // If the particle system is not currently playing, skip updating the time
            if (Control != StateControl.Play)
                return;

            totalElapsedTime += dt;
            currentElapsedTime += dt;

            if (!EnableLooping)
                return;

            if (currentElapsedTime >= resetSeconds)
            {
                while (currentElapsedTime >= resetSeconds)
                    currentElapsedTime -= resetSeconds;

                particleSystem.RestartSimulation();
            }
        }
    }
}
