using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Particles.VertexLayouts;

namespace SiliconStudio.Xenko.Particles.Materials
{
    [DataContract("UVBuilderFlipbook")]
    public class UVBuilderFlipbook : UVBuilderBase
    {
        private UInt32 xDivisions = 4;
        private UInt32 yDivisions = 4;
        private float xStep = 0.25f;
        private float yStep = 0.25f;
        private UInt32 totalFrames = 16;
        private UInt32 startingFrame = 0;
        private UInt32 animationSpeedOverLife = 16;

        [DataMember(200)]
        [Display("X divisions")]
        public UInt32 XDivisions
        {
            get { return xDivisions; }
            set
            {
                xDivisions = (value > 0) ? value : 1;
                xStep = (1f / xDivisions);
                totalFrames = xDivisions * yDivisions;
                startingFrame = Math.Min(startingFrame, totalFrames);
            }
        }

        [DataMember(240)]
        [Display("Y divisions")]
        public UInt32 YDivisions
        {
            get { return yDivisions; }
            set
            {
                yDivisions = (value > 0) ? value : 1;
                yStep = (1f / yDivisions);
                totalFrames = xDivisions * yDivisions;
                startingFrame = Math.Min(startingFrame, totalFrames);
            }
        }

        [DataMember(280)]
        [Display("Starting frame")]
        public UInt32 StartingFrame
        {
            get { return startingFrame; }
            set
            {
                startingFrame = value;
                startingFrame = Math.Min(startingFrame, totalFrames);
            }
        }

        [DataMember(320)]
        [Display("Animation speed")]
        public UInt32 AnimationSpeed
        {
            get { return animationSpeedOverLife; }
            set { animationSpeedOverLife = value; }
        }

        public unsafe override void BuildUVCoordinates(ParticleVertexLayout vtxBuilder, ParticlePool pool)
        {
            var lifeField = pool.GetField(ParticleFields.RemainingLife);

            if (!lifeField.IsValid())
                return;

            foreach (var particle in pool)
            {
                var normalizedTimeline = 1f - *(float*)(particle[lifeField]);

                var spriteId = startingFrame + (int)(normalizedTimeline * animationSpeedOverLife);

                var uvTransform = new Vector4((spriteId % xDivisions) * xStep, (spriteId / yDivisions) * yStep, xStep, yStep);

                for (int i = 0; i < vtxBuilder.VerticesPerParticle; i++)
                {
                    vtxBuilder.TransformUvCoords(ref uvTransform);
                    vtxBuilder.NextVertex();
                }
            }

        }
    }
}
