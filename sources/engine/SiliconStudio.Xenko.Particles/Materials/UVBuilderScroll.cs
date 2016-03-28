// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Particles.Sorters;
using SiliconStudio.Xenko.Particles.VertexLayouts;

namespace SiliconStudio.Xenko.Particles.Materials
{
    /// <summary>
    /// Animates the texture coordinates starting with one rectangle and scrolling/zooming it to an ending rectangle over the particle's life
    /// </summary>
    [DataContract("UVBuilderScroll")]
    [Display("Scrolling")]
    public class UVBuilderScroll : UVBuilder, IAttributeTransformer<Vector2>
    {
        /// <summary>
        /// Starting sub-region (rectangle) for the scroll
        /// </summary>
        /// <userdoc>
        /// The rectangular sub-region of the texture where the scrolling should start, given as (Xmin, Ymin, Xmax, Ymax) ( (0, 0, 1, 1) being the entire texture). Numbers also can be negative or bigger than 1.
        /// </userdoc>
        [DataMember(200)]
        [Display("Start frame")]
        public Vector4 StartFrame { get; set; } = new Vector4(0, 0, 1, 1);

        /// <summary>
        /// Ending sub-region (rectangle) for the scroll
        /// </summary>
        /// <userdoc>
        /// The rectangular sub-region of the texture where the scrolling should end at the particle life's end, given as (Xmin, Ymin, Xmax, Ymax) ( (0, 0, 1, 1) being the entire texture). Numbers also can be negative or bigger than 1.
        /// </userdoc>
        [DataMember(240)]
        [Display("End frame")]
        public Vector4 EndFrame { get; set; } = new Vector4(0, 1, 1, 2);

        /// <inheritdoc />
        public unsafe override void BuildUVCoordinates(ParticleVertexBuilder vertexBuilder, ParticleSorter sorter, AttributeDescription texCoordsDescription)
        {
            var lifeField = sorter.GetField(ParticleFields.RemainingLife);

            if (!lifeField.IsValid())
                return;

            var texAttribute = vertexBuilder.GetAccessor(texCoordsDescription);
            if (texAttribute.Size == 0 && texAttribute.Offset == 0)
            {
                return;
            }

            var texDefault = vertexBuilder.GetAccessor(vertexBuilder.DefaultTexCoords);
            if (texDefault.Size == 0 && texDefault.Offset == 0)
            {
                return;
            }

            foreach (var particle in sorter)
            {
                var normalizedTimeline = 1f - *(float*)(particle[lifeField]); ;

                uvTransform = Vector4.Lerp(StartFrame, EndFrame, normalizedTimeline);
                uvTransform.Z -= uvTransform.X;
                uvTransform.W -= uvTransform.Y;

                vertexBuilder.TransformAttributePerSegment(texDefault, texAttribute, this);

                vertexBuilder.NextSegment();
            }


            vertexBuilder.RestartBuffer();
        }

        private Vector4 uvTransform = new Vector4(0, 0, 1, 1);

        public void Transform(ref Vector2 attribute)
        {
            attribute.X = uvTransform.X + uvTransform.Z * attribute.X;
            attribute.Y = uvTransform.Y + uvTransform.W * attribute.Y;
        }

    }
}
