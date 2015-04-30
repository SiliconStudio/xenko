using System.Collections.Generic;
using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Paradox.Rendering;

namespace SiliconStudio.Paradox.Physics
{
    [ContentSerializer(typeof(DataContentSerializer<ConvexHullColliderShapeDesc>))]
    [DataContract("ConvexHullColliderShapeDesc")]
    [Display(50, "ConvexHullColliderShape")]
    public class ConvexHullColliderShapeDesc : IColliderShapeDesc
    {
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
        [Browsable(false)]
#endif
        [DataMember(10)]
        public List<List<List<Vector3>>> ConvexHulls; // Multiple meshes -> Multiple Hulls -> Hull points

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
        [Browsable(false)]
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
        public int Depth = 10;

        /// <userdoc>
        /// How many position samples to internally compute clipping planes ( the higher the more complex ).
        /// </userdoc>
        [DataMember(60)]
        public int PosSampling = 10;

        /// <userdoc>
        /// How many angle samples to internally compute clipping planes ( the higher the more complex ), nested with position samples, for each position sample it will compute the amount defined here.
        /// </userdoc>
        [DataMember(70)]
        public int AngleSampling = 10;

        /// <userdoc>
        /// If higher then 0 the computation will try to further improve the shape position sampling (this will slow down the process).
        /// </userdoc>
        [DataMember(80)]
        public int PosRefine = 5;

        /// <userdoc>
        /// If higher then 0 the computation will try to further improve the shape angle sampling (this will slow down the process).
        /// </userdoc>
        [DataMember(90)]
        public int AngleRefine = 5;

        /// <userdoc>
        /// Applied to the concavity during crippling plane approximation.
        /// </userdoc>
        [DataMember(100)]
        public float Alpha = 0.01f;

        /// <userdoc>
        /// Threshold of concavity, rising this will make the shape simpler.
        /// </userdoc>
        [DataMember(110)]
        public float Threshold = 0.01f;
    }
}