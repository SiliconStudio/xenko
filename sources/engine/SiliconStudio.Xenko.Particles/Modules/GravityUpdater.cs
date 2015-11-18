using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Particles.Modules
{
    [DataContract("GravityUpdater")]
    public class GravityUpdater : UpdaterBase
    {
        /// <summary>
        /// Direction and magnitude of the gravitational acceleration
        /// </summary>
        [DataMember(10)]
        public Vector3 GravitationalAcceleration;

        public GravityUpdater()
        {
            // In case of a conventional standard Earth gravitational acceleration, where Y is up. Change it depending on your needs.
            GravitationalAcceleration = new Vector3(0, -9.80665f, 0);

            RequiredFields.Add(ParticleFields.Position);
            RequiredFields.Add(ParticleFields.Velocity);
        }

        public override unsafe void Update(float dt, ParticlePool pool)
        {
            if (!pool.FieldExists(ParticleFields.Position) || !pool.FieldExists(ParticleFields.Velocity))
                return;

            var posField = pool.GetField(ParticleFields.Position);
            var velField = pool.GetField(ParticleFields.Velocity);

            var deltaVel = GravitationalAcceleration * dt;
            var deltaPos = deltaVel * (dt * 0.5f);

            foreach (var particle in pool)
            {
                (*((Vector3*)particle[posField])) += deltaPos;
                (*((Vector3*)particle[velField])) += deltaVel;
            }
        }
    }
}
