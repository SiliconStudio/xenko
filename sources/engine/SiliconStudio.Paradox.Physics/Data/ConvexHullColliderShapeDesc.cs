using System.Collections.Generic;
using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Paradox.Physics
{
    [ContentSerializer(typeof(DataContentSerializer<ConvexHullColliderShapeDesc>))]
    [DataContract("ConvexHullColliderShapeDesc")]
    public class ConvexHullColliderShapeDesc : IColliderShapeDesc
    {
        [DataMember(10)]
        [Browsable(false)]
        public List<List<List<Vector3>>> ConvexHulls; // Multiple meshes -> Multiple Hulls -> Hull points

        [DataMember(20)]
        [Browsable(false)]
        public List<List<List<uint>>> ConvexHullsIndices; // Multiple meshes -> Multiple Hulls -> Hull tris

        [DataMember(30)]
        public Core.Serialization.ContentReference<Effects.Data.ModelData> Model;

        [DataMember(40)]
        public bool SimpleWrap = true;

        [DataMember(50)]
        public int Depth = 10;

        [DataMember(60)]
        public int PosSampling = 10;

        [DataMember(70)]
        public int AngleSampling = 10;

        [DataMember(80)]
        public int PosRefine = 5;

        [DataMember(90)]
        public int AngleRefine = 5;

        [DataMember(100)]
        public float Alpha = 0.01f;

        [DataMember(110)]
        public float Threshold = 0.01f;
    }
}