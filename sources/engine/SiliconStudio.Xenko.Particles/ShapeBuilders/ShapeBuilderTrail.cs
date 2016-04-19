// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Particles.Sorters;
using SiliconStudio.Xenko.Particles.VertexLayouts;
using SiliconStudio.Xenko.Particles.ShapeBuilders.Tools;

namespace SiliconStudio.Xenko.Particles.ShapeBuilders
{
    /// <summary>
    /// Shape builder which builds all particles as a trail, connecting adjacent particles in a ribbon defined by a fixed 3d axis
    /// </summary>
    [DataContract("ShapeBuilderTrail")]
    [Display("Trail")]
    public class ShapeBuilderTrail : ShapeBuilder
    {
        private readonly Ribbonizer ribbonizer = new Ribbonizer();

        /// <summary>
        /// Smoothing provides the option to additionally smooth the ribbon, enhancing visual quality for sharp angles
        /// </summary>
        /// <userdoc>
        /// Smoothing provides the option to additionally smooth the ribbon, enhancing visual quality for sharp angles
        /// </userdoc>
        [DataMember(5)]
        [Display("Smoothing")]
        public SmoothingPolicy SmoothingPolicy
        {
            get { return ribbonizer.SmoothingPolicy; }
            set
            {
                ribbonizer.SmoothingPolicy = value;

                QuadsPerParticle = (ribbonizer.SmoothingPolicy == SmoothingPolicy.None) ?
                    1 : ribbonizer.Segments;
            }
        }

        /// <summary>
        /// If the ribbon is smotthed, how many segments should be used between each two particles
        /// </summary>
        /// <userdoc>
        /// If the ribbon is smotthed, how many segments should be used between each two particles
        /// </userdoc>
        [DataMember(6)]
        [Display("Segments")]
        public int Segments
        {
            get { return ribbonizer.Segments; }
            set
            {
                ribbonizer.Segments = value;

                QuadsPerParticle = (ribbonizer.SmoothingPolicy == SmoothingPolicy.None) ?
                    1 : ribbonizer.Segments;
            }
        }

        /// <summary>
        /// Should the axis of control point be treated as the trail's edge or the trail's center
        /// </summary>
        /// <userdoc>
        /// Should the axis of control point be treated as the trail's edge or the trail's center
        /// </userdoc>
        [DataMember(8)]
        [Display("Axis")]
        public EdgePolicy EdgePolicy { get { return ribbonizer.EdgePolicy; } set { ribbonizer.EdgePolicy = value; } }

        /// <summary>
        /// Specifies how texture coordinates for the ribbons should be built
        /// </summary>
        /// <userdoc>
        /// Specifies how texture coordinates for the ribbons should be built
        /// </userdoc>
        [DataMember(10)]
        [Display("UV Coords")]
        public TextureCoordinatePolicy TextureCoordinatePolicy { get { return ribbonizer.TextureCoordinatePolicy; } set { ribbonizer.TextureCoordinatePolicy = value; } }

        /// <summary>
        /// The factor (coefficient) for length to use when building texture coordinates
        /// </summary>
        /// <userdoc>
        /// The factor (coefficient) for length to use when building texture coordinates
        /// </userdoc>
        [DataMember(20)]
        [Display("UV Factor")]
        public float TexCoordsFactor { get { return ribbonizer.TexCoordsFactor; } set { ribbonizer.TexCoordsFactor = value; } }

        /// <summary>
        /// Texture coordinates flip and rotate policy
        /// </summary>
        /// <userdoc>
        /// Texture coordinates flip and rotate policy
        /// </userdoc>
        [DataMember(30)]
        [Display("UV Rotate")]
        public UVRotate UVRotate { get { return ribbonizer.UVRotate; } set { ribbonizer.UVRotate = value; } }

        /// <inheritdoc />
        public override int QuadsPerParticle { get; protected set; } = 1;

        /// <inheritdoc />
        public override void SetRequiredQuads(int quadsPerParticle, int livingParticles, int totalParticles)
        {
            ribbonizer.Restart(totalParticles, quadsPerParticle);
        }

        /// <inheritdoc />
        public unsafe override int BuildVertexBuffer(ParticleVertexBuilder vtxBuilder, Vector3 invViewX, Vector3 invViewY,
            ref Vector3 spaceTranslation, ref Quaternion spaceRotation, float spaceScale, ParticleSorter sorter)
        {
            // Get all the required particle fields
            var positionField = sorter.GetField(ParticleFields.Position);
            if (!positionField.IsValid())
                return 0;
            var sizeField = sorter.GetField(ParticleFields.Size);
            var directionField = sorter.GetField(ParticleFields.Direction);

            // Check if the draw space is identity - in this case we don't need to transform the position, scale and rotation vectors
            var trsIdentity = (spaceScale == 1f);
            trsIdentity = trsIdentity && (spaceTranslation.Equals(new Vector3(0, 0, 0)));
            trsIdentity = trsIdentity && (spaceRotation.Equals(Quaternion.Identity));


            var renderedParticles = 0;
            vtxBuilder.RestartBuffer();

            uint oldOrderValue = 0;
            var orderField = sorter.GetField(ParticleFields.Order);

            foreach (var particle in sorter)
            {
                if (orderField.IsValid())
                {
                    var orderValue = (*((uint*)particle[orderField]));

                    if ((orderValue >> 16) != (oldOrderValue >> 16))
                    {
                        ribbonizer.Ribbonize(vtxBuilder, QuadsPerParticle);
                        ribbonizer.RibbonSplit();
                    }

                    oldOrderValue = orderValue;
                }

                var centralPos = particle.Get(positionField);

                var particleSize = sizeField.IsValid() ? particle.Get(sizeField) : 1f;
                var particleDirection = directionField.IsValid() ? particle.Get(directionField) * particleSize : new Vector3(0f, particleSize, 0f);

                if (!trsIdentity)
                {
                    spaceRotation.Rotate(ref centralPos);
                    centralPos = centralPos * spaceScale + spaceTranslation;

                    // Direction
                    spaceRotation.Rotate(ref particleDirection);
                    particleDirection *= spaceScale;
                }

                ribbonizer.AddParticle(ref centralPos, ref particleDirection);

                renderedParticles++;
            }

            ribbonizer.Ribbonize(vtxBuilder, QuadsPerParticle);

            var vtxPerShape = 4 * QuadsPerParticle;
            return renderedParticles * vtxPerShape;
        }

        /// <summary>
        /// The <see cref="Ribbonizer"/> takes a list of points and creates a ribbon (connected quads), adjusting its texture coordinates accordingly
        /// </summary>
        sealed class Ribbonizer
        {
            private int lastParticle = 0;
            private Vector3[] positions = new Vector3[1];
            private Vector3[] directions = new Vector3[1];
            private int sections = 1;

            /// <summary>
            /// This property is exposed to the ShapeBuilder class
            /// </summary>
            [DataMemberIgnore]
            public EdgePolicy EdgePolicy { get; set; } = EdgePolicy.Edge;

            /// <summary>
            /// This property is exposed to the ShapeBuilder class
            /// </summary>
            [DataMemberIgnore]
            public TextureCoordinatePolicy TextureCoordinatePolicy { get; set; } = TextureCoordinatePolicy.AsIs;

            /// <summary>
            /// This property is exposed to the ShapeBuilder class
            /// </summary>
            [DataMemberIgnore]
            public SmoothingPolicy SmoothingPolicy { get; set; } = SmoothingPolicy.None;

            /// <summary>
            /// This property is exposed to the ShapeBuilder class
            /// </summary>
            [DataMemberIgnore]
            public int Segments { get; set; } = 5;

            /// <summary>
            /// This property is exposed to the ShapeBuilder class
            /// </summary>
            [DataMemberIgnore]
            public float TexCoordsFactor { get; set; } = 1f;

            /// <summary>
            /// This property is exposed to the ShapeBuilder class
            /// </summary>
            [DataMemberIgnore]
            public UVRotate UVRotate { get; set; } 

            /// <summary>
            /// Restarts the point string, potentially expanding the capacity
            /// </summary>
            /// <param name="newCapacity">Required minimum capacity</param>
            public void Restart(int newCapacity, int sectionsPerParticle)
            {
                lastParticle = 0;
                sections = sectionsPerParticle;

                int requiredCapacity = sectionsPerParticle * newCapacity;

                if (requiredCapacity > positions.Length)
                {
                    positions = new Vector3[requiredCapacity];
                    directions = new Vector3[requiredCapacity];
                }
            }

            /// <summary>
            /// Splits (cuts) the trail without restarting or rebuilding the vertex buffer
            /// </summary>
            public void RibbonSplit()
            {
                lastParticle = 0;
            }

            /// <summary>
            /// Adds a new particle position and size to the point string
            /// </summary>
            /// <param name="position">Position of the control point</param>
            /// <param name="direction">Direction or offset from the control point</param>
            public void AddParticle(ref Vector3 position, ref Vector3 direction)
            {
                if (lastParticle >= positions.Length)
                    return;

                positions[lastParticle] = position;
                directions[lastParticle] = direction;

                lastParticle += sections;
            }

            /// <summary>
            /// Advanced interpolation, drawing the vertices in a circular arc between two adjacent control points
            /// </summary>
            private void ExpandVertices_Circular()
            {
                if (sections <= 1)
                    return;

                var lerpStep = 1f / sections;

                var Pt0 = positions[0] * 2 - positions[sections];
                var Pt1 = positions[0];
                var Pt2 = positions[sections];

                var O1 = Circumcenter(ref Pt0, ref Pt1, ref Pt2);
                var R1 = (O1 - Pt1).Length();

                var d1 = directions[0];
                var d2 = directions[sections];

                int index = 0;
                while (index < lastParticle)
                {
                    var Pt3 = (index + sections * 2 < lastParticle) ? positions[index + sections * 2] : Pt2;
                    var d3 = (index + sections * 2 < lastParticle) ? directions[index + sections * 2] : new Vector3(0f, 0f, 0f);
                    var O2 = Circumcenter(ref Pt1, ref Pt2, ref Pt3);
                    var R2 = (O2 - Pt2).Length();

                    if (index + sections * 2 >= lastParticle)
                    {
                        O2 = O1;
                        R2 = R1;
                    }

                    for (int j = 1; j < sections; j++)
                    {
                        positions[index + j] = Vector3.Lerp(Pt1, Pt2, j * lerpStep);

                        // Circular motion
                        var dist1 = positions[index + j] - O1;
                        dist1.Normalize();
                        var dist2 = positions[index + j] - O2;
                        dist2.Normalize();

                        positions[index + j] = Vector3.Lerp(O1 + dist1 * R1, O2 + dist2 * R2, j * lerpStep);

                        directions[index + j] = Vector3.Lerp(d1, d2, j * lerpStep);
                    }

                    index += sections;
                    Pt1 = Pt2; Pt2 = Pt3;
                    d1 = d2; d2 = d3;
                    O1 = O2;
                    R1 = R2;

                }
            }

            /// <summary>
            /// Simple interpolation using Catmull-Rom
            /// </summary>
            private void ExpandVertices_CatmullRom()
            {
                var lerpStep = 1f / sections;

                var Pt0 = positions[0] * 2 - positions[sections];
                var Pt1 = positions[0];
                var Pt2 = positions[sections];

                var d1 = directions[0];
                var d2 = directions[sections];

                int index = 0;
                while (index < lastParticle)
                {
                    var Pt3 = (index + sections * 2 < lastParticle) ? positions[index + sections * 2] : Pt2;

                    for (int j = 1; j < sections; j++)
                    {
                        positions[index + j] = Vector3.CatmullRom(Pt0, Pt1, Pt2, Pt3, j * lerpStep);
                        directions[index + j] = Vector3.Lerp(d1, d2, j * lerpStep);
                    }

                    Pt0 = Pt1; Pt1 = Pt2; Pt2 = Pt3;
                    d1 = d2; d2 = (index + sections * 2 < lastParticle) ? directions[index + sections * 2] : new Vector3(0f, 0f, 0f);

                    index += sections;
                }
            }

            /// <summary>
            /// Constructs the ribbon by outputting vertex stream based on the positions and sizes specified previously
            /// </summary>
            /// <param name="vtxBuilder">Target <see cref="ParticleVertexBuilder"/></param> to use
            /// <param name="quadsPerParticle">The required number of quads per each particle</param>
            public unsafe void Ribbonize(ParticleVertexBuilder vtxBuilder, int quadsPerParticle)
            {
                if (lastParticle <= 0)
                    return;

                var posAttribute = vtxBuilder.GetAccessor(VertexAttributes.Position);
                var texAttribute = vtxBuilder.GetAccessor(vtxBuilder.DefaultTexCoords);

                if (lastParticle <= sections)
                {
                    // Optional - connect first particle to the origin/emitter

                    // Draw a dummy quad for the first particle
                    var particlePos = new Vector3(0, 0, 0);
                    var uvCoord = new Vector2(0, 0);

                    for (var particleIdx = 0; particleIdx < lastParticle; particleIdx++)
                    {
                        for (var vtxIdx = 0; vtxIdx < 4; vtxIdx++)
                        {
                            vtxBuilder.SetAttribute(posAttribute, (IntPtr)(&particlePos));
                            vtxBuilder.SetAttribute(texAttribute, (IntPtr)(&uvCoord));
                            vtxBuilder.NextVertex();
                        }
                    }

                    return;
                }

                if (sections > 1)
                {
                    if (SmoothingPolicy == SmoothingPolicy.Best)
                        ExpandVertices_Circular();
                    else // if (SmoothingPolicy == SmoothingPolicy.Fast)
                        ExpandVertices_CatmullRom();
                }

                vtxBuilder.SetVerticesPerSegment(quadsPerParticle * 6, quadsPerParticle * 4, quadsPerParticle * 2);

                var axis0 = positions[0] - positions[1];
                axis0.Normalize();

                var oldPoint = positions[0];
                var oldUnitX = directions[0];

                // Step 2 - Draw each particle, connecting it to the previous (front) position

                var vCoordOld = 0f;

                for (int i = 0; i < lastParticle; i++)
                {
                    var centralPos = positions[i];

                    // Directions for smoothing
                    var axis1 = (i + 1 < lastParticle) ? positions[i] - positions[i + 1] : positions[lastParticle - 2] - positions[lastParticle - 1];
                    axis1.Normalize();

                    var unitX = directions[i];

                    // Particle rotation - intentionally IGNORED for ribbon

                    var particlePos = (EdgePolicy == EdgePolicy.Edge) ? oldPoint - oldUnitX : oldPoint;
                    var uvCoord = new Vector2(0, 0);
                    var rotatedCoord = uvCoord;


                    // Top Left - 0f 0f
                    uvCoord.Y = (TextureCoordinatePolicy == TextureCoordinatePolicy.AsIs) ? 0 : vCoordOld;
                    vtxBuilder.SetAttribute(posAttribute, (IntPtr)(&particlePos));

                    rotatedCoord = UVRotate.GetCoords(uvCoord);
                    vtxBuilder.SetAttribute(texAttribute, (IntPtr)(&rotatedCoord));

                    vtxBuilder.NextVertex();


                    // Top Right - 1f 0f
                    particlePos += (EdgePolicy == EdgePolicy.Edge) ? oldUnitX * 2 : oldUnitX;
                    vtxBuilder.SetAttribute(posAttribute, (IntPtr)(&particlePos));

                    uvCoord.X = 1;
                    rotatedCoord = UVRotate.GetCoords(uvCoord);
                    vtxBuilder.SetAttribute(texAttribute, (IntPtr)(&rotatedCoord));

                    vtxBuilder.NextVertex();


                    // Move the position to the next particle in the ribbon
                    particlePos += centralPos - oldPoint;
                    particlePos += unitX - oldUnitX;
                    vCoordOld = (TextureCoordinatePolicy == TextureCoordinatePolicy.Stretched) ?
                        ((i + 1) / (float)(lastParticle) * TexCoordsFactor) : ((centralPos - oldPoint).Length() * TexCoordsFactor) + vCoordOld;


                    // Bottom Left - 1f 1f
                    uvCoord.Y = (TextureCoordinatePolicy == TextureCoordinatePolicy.AsIs) ? 1 : vCoordOld;
                    vtxBuilder.SetAttribute(posAttribute, (IntPtr)(&particlePos));

                    rotatedCoord = UVRotate.GetCoords(uvCoord);
                    vtxBuilder.SetAttribute(texAttribute, (IntPtr)(&rotatedCoord));

                    vtxBuilder.NextVertex();


                    // Bottom Right - 0f 1f
                    particlePos -= (EdgePolicy == EdgePolicy.Edge) ? unitX * 2 : unitX;
                    vtxBuilder.SetAttribute(posAttribute, (IntPtr)(&particlePos));

                    uvCoord.X = 0;
                    rotatedCoord = UVRotate.GetCoords(uvCoord);
                    vtxBuilder.SetAttribute(texAttribute, (IntPtr)(&rotatedCoord));

                    vtxBuilder.NextVertex();


                    // Preserve the old attributes for the next cycle
                    oldUnitX = unitX;
                    oldPoint = centralPos;
                }
            }
        }
    }
}
