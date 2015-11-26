using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Particles.Initializers
{
    [DataContract("InitialSize")]
    public class InitialSize : InitializerBase
    {
        // TODO Change the RNG to a deterministic generator
        readonly Random randomNumberGenerator = new Random();

        public InitialSize()
        {
            RequiredFields.Add(ParticleFields.Size);
        }

        public unsafe override void Initialize(ParticlePool pool, int startIdx, int endIdx, int maxCapacity)
        {
            if (!pool.FieldExists(ParticleFields.Size))
                return;

            var sizeField = pool.GetField(ParticleFields.Size);

            var minSize = WorldScale * RandomSize.X;
            var sizeGap = WorldScale * RandomSize.Y - minSize;

            var i = startIdx;
            while (i != endIdx)
            {
                var particle = pool.FromIndex(i);

                (*((float*)particle[sizeField])) = ((float)randomNumberGenerator.NextDouble() * sizeGap + minSize); 

                i = (i + 1) % maxCapacity;
            }
        }

        [DataMemberIgnore]
        public float WorldScale { get; private set; } = 1f;

        /// <summary>
        /// Note on inheritance. The current values only change once per frame, when the SetParentTRS is called. 
        /// This is intentional and reduces overhead, because SetParentTRS is called exactly once/turn.
        /// </summary>
        [DataMember(5)]
        [Display("Inheritance")]
        public InheritLocation InheritLocation { get; set; } = InheritLocation.Scale;


        [DataMember(30)]
        [Display("Random size")]
        public Vector2 RandomSize = new Vector2(0.5f, 1);

        public override void SetParentTRS(ref Vector3 Translation, ref Quaternion Rotation, float Scale)
        {
            var hasScl = InheritLocation.HasFlag(InheritLocation.Scale);

            WorldScale = (hasScl) ? Scale : 1f;
        }
    }
}
