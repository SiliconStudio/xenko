using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Particles.Modules
{
    public class SampleInitializer : ParticleModule
    {
        // TODO Change the RNG to a deterministic generator
        readonly Random randomNumberGenerator = new Random();

        public SampleInitializer()
        {
            Type = ModuleType.Initializer;

            RequiredFields.Add(ParticleFields.Position);
            RequiredFields.Add(ParticleFields.Velocity);
        }

        public unsafe override void Initialize(ParticlePool pool, int startIdx, int endIdx, int maxCapacity)
        {
            
            if (!pool.FieldExists(ParticleFields.Position) || !pool.FieldExists(ParticleFields.Velocity))
                return;

            var posField = pool.GetField(ParticleFields.Position);
            var velField = pool.GetField(ParticleFields.Velocity);

            var startPos = new Vector3(0, 0, 0);
            var startVel = new Vector3(0, 0, 0);

            var i = startIdx;
            while (i != endIdx)
            {
                var particle = pool.FromIndex(i);

                (*((Vector3*)particle[posField])) = startPos;

                startVel.X = ((float)randomNumberGenerator.NextDouble() * 4 - 2);
                startVel.Y = ((float)randomNumberGenerator.NextDouble() * 2 + 2);
                startVel.Z = ((float)randomNumberGenerator.NextDouble() * 4 - 2);
                (*((Vector3*)particle[velField])) = startVel;

                i = (i + 1) % maxCapacity;
            }
        }
    }
}
