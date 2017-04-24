// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

using SiliconStudio.Xenko.Effects;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.EntityModel;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Games.Mathematics;
using SiliconStudio.Xenko.Games.Serialization.Contents;
using Xenko.Framework.Shaders;

namespace ScriptTest
{
    [ContentSerializer(typeof(EntityComponentContentSerializer<SmokeParticleEmitterComponent>))]
    [Display]
    public class SmokeParticleEmitterComponent : ParticleEmitterComponent
    {
        public SmokeParticleEmitterComponent()
        {
            Type = ParticleEmitterType.GpuStatic;
            Shader = new ShaderClassSource("ParticleUpdaterSmoke");
            UpdateData += UpdateParticlesData;
            ParticleElementSize = Utilities.SizeOf<ScriptParticleSmoke.ParticleData>();
            Parameters.Set(ScriptParticleSmoke.SmokeEmitterKey, new SmokeEmitterDescription());
        }

        private void UpdateParticlesData(ParticleEmitterComponent smokeParticleEmitterComponent)
        {
            bool isUptoDate = true;
            if (ParticleData == null || ParticleData.Length != Count)
            {
                ParticleData = new ScriptParticleSmoke.ParticleData[Count];
                isUptoDate = false;
            }

            if (isUptoDate)
                return;

            var description = Description;
            var random = new Random();
            var particlesBuffer = (ScriptParticleSmoke.ParticleData[])ParticleData;

            for (int i = 0; i < particlesBuffer.Length; i++)
            {
                particlesBuffer[i] = new ScriptParticleSmoke.ParticleData
                    {
                        Position = description.Position,
                        Velocity = (Half3)(description.Velocity + Vector3.Modulate(new Vector3((float)(random.NextDouble() * 2 - 1), (float)(random.NextDouble() * 2 - 1), (float)(random.NextDouble())), description.Scatter)),
                        Size = (Half)description.InitialSize,
                        Time = (float)random.NextDouble() * description.MaxTime,
                        Opacity = (Half)description.Opacity,
                        Factors = new Half4((Half)random.NextDouble(), (Half)0, (Half)description.Opacity, (Half)0),
                        TimeStep = (Half)10.0f,
                    };
                particlesBuffer[i].Position += ((Vector3)particlesBuffer[i].Velocity) * particlesBuffer[i].Time * 100.0f / 1000.0f;
            }
        }

        [DataMemberConvert]
        [Display]
        public SmokeEmitterDescription Description
        {
            get
            {
                return Parameters.TryGet(ScriptParticleSmoke.SmokeEmitterKey);
            }
            set
            {
                Parameters.Set(ScriptParticleSmoke.SmokeEmitterKey, value);
            }
        }
    }
}
