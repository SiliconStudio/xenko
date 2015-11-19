// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Particles.Modules
{
    [DataContract("GravityUpdater")]
    [Display("OldPosition")]
    public class OldPositionUpdater : UpdaterBase
    {
        public OldPositionUpdater()
        {
            RequiredFields.Add(ParticleFields.Position);
            RequiredFields.Add(ParticleFields.OldPosition);
        }

        public override unsafe void Update(float dt, ParticlePool pool)
        {
            if (!pool.FieldExists(ParticleFields.Position) || !pool.FieldExists(ParticleFields.OldPosition))
                return;

            var posField = pool.GetField(ParticleFields.Position);
            var oldPosField = pool.GetField(ParticleFields.OldPosition);

            foreach (var particle in pool)
            {
                (*((Vector3*)particle[oldPosField])) = (*((Vector3*)particle[posField]));
            }
        }
    }
}
