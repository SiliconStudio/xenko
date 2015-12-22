// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Particles.Sorters;
using SiliconStudio.Xenko.Particles.VertexLayouts;

namespace SiliconStudio.Xenko.Particles.Materials
{
    [DataContract("UVBuilderScroll")]
    [Display("Scrolling")]
    public class UVBuilderScroll : UVBuilderBase
    {
        [DataMember(200)]
        [Display("Start frame")]
        public Vector4 StartFrame { get; set; } = new Vector4(0, 0, 1, 1);

        [DataMember(240)]
        [Display("End frame")]
        public Vector4 EndFrame { get; set; } = new Vector4(0, 1, 1, 2);

        public unsafe override void BuildUVCoordinates(ParticleVertexBuilder vertexBuilder, ParticleSorter sorter)
        {
            var lifeField = sorter.GetField(ParticleFields.RemainingLife);

            if (!lifeField.IsValid())
                return;

            var texAttribute = vertexBuilder.GetAccessor(vertexBuilder.DefaultTexCoords);

            foreach (var particle in sorter)
            {
                var normalizedTimeline = 1f - *(float*)(particle[lifeField]); ;

                var uvTransform = Vector4.Lerp(StartFrame, EndFrame, normalizedTimeline);
                uvTransform.Z -= uvTransform.X;
                uvTransform.W -= uvTransform.Y;

                ParticleVertexBuilder.TransformAttributeDelegate<Vector2> transformCoords =
                    (ref Vector2 value) =>
                    {
                        value.X = uvTransform.X + uvTransform.Z * value.X;
                        value.Y = uvTransform.Y + uvTransform.W * value.Y;
                    };

                vertexBuilder.TransformAttributePerParticle(texAttribute, transformCoords);

                vertexBuilder.NextParticle();
            }
        }
    }
}
