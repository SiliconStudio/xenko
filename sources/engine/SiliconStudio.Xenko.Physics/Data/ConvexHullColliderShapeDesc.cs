// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Rendering;
using System.Collections.Generic;

namespace SiliconStudio.Xenko.Physics
{
    [ContentSerializer(typeof(DataContentSerializer<ConvexHullColliderShapeDesc>))]
    [DataContract("ConvexHullColliderShapeDesc")]
    [Display(50, "Convex Hull")]
    public class ConvexHullColliderShapeDesc : IAssetColliderShapeDesc
    {
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP

        [Display(Browsable = false)]
#endif
        [DataMember(10)]
        public List<List<List<Vector3>>> ConvexHulls; // Multiple meshes -> Multiple Hulls -> Hull points

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP

        [Display(Browsable = false)]
#endif
        [DataMember(20)]
        public List<List<List<uint>>> ConvexHullsIndices; // Multiple meshes -> Multiple Hulls -> Hull tris

        /// <userdoc>
        /// Model asset from where the engine will derive the convex hull.
        /// </userdoc>
        [DataMember(30)]
        public Model Model;

        /// <userdoc>
        /// If this is checked the following parameters are totally ignored, as only a simple convex hull of the whole model will be generated.
        /// </userdoc>
        [DataMember(40)]
        [Display(Browsable = false)]
        public bool SimpleWrap = true;

        /// <userdoc>
        /// The scaling of the generated convex hull.
        /// </userdoc>
        [DataMember(45)]
        public Vector3 Scaling = Vector3.One;

        /// <userdoc>
        /// Control how many sub convex hulls will be created, more depth will result in a more complex decomposition.
        /// </userdoc>
        [DataMember(50)]
        [Display(Browsable = false)]
        public int Depth = 10;

        /// <userdoc>
        /// How many position samples to internally compute clipping planes ( the higher the more complex ).
        /// </userdoc>
        [DataMember(60)]
        [Display(Browsable = false)]
        public int PosSampling = 10;

        /// <userdoc>
        /// How many angle samples to internally compute clipping planes ( the higher the more complex ), nested with position samples, for each position sample it will compute the amount defined here.
        /// </userdoc>
        [DataMember(70)]
        [Display(Browsable = false)]
        public int AngleSampling = 10;

        /// <userdoc>
        /// If higher then 0 the computation will try to further improve the shape position sampling (this will slow down the process).
        /// </userdoc>
        [DataMember(80)]
        [Display(Browsable = false)]
        public int PosRefine = 5;

        /// <userdoc>
        /// If higher then 0 the computation will try to further improve the shape angle sampling (this will slow down the process).
        /// </userdoc>
        [DataMember(90)]
        [Display(Browsable = false)]
        public int AngleRefine = 5;

        /// <userdoc>
        /// Applied to the concavity during crippling plane approximation.
        /// </userdoc>
        [DataMember(100)]
        [Display(Browsable = false)]
        public float Alpha = 0.01f;

        /// <userdoc>
        /// Threshold of concavity, rising this will make the shape simpler.
        /// </userdoc>
        [DataMember(110)]
        [Display(Browsable = false)]
        public float Threshold = 0.01f;

        public int CompareTo(object obj)
        {
            var other = obj as ConvexHullColliderShapeDesc;
            if (other == null) return -1;

            if (other.Model == Model &&
                other.SimpleWrap == SimpleWrap &&
                other.Scaling == Scaling &&
                other.Depth == Depth &&
                other.PosSampling == PosSampling &&
                other.AngleSampling == AngleSampling &&
                other.PosRefine == PosRefine &&
                other.AngleRefine == AngleRefine &&
                Math.Abs(other.Alpha - Alpha) < float.Epsilon &&
                Math.Abs(other.Threshold - Threshold) < float.Epsilon) return 0;

            return 1;
        }
    }
}