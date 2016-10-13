using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;

namespace FirstPersonShooter.Trigger
{
    [DataContract("TriggerEvent")]
    public class TriggerEvent
    {
        [DataMember(10)]
        [Display("Name")]
        [InlineProperty]
        public string Name { get; set; }

        [DataMember(20)]
        [Display("Source")]
        public Prefab SourcePrefab { get; set; }

        [DataMember(30)]
        public bool FollowEntity { get; set; }

        [DataMember(40)]
        [Display("Duration")]
        public float Duration { get; set; } = 3f;

        [DataMemberIgnore]
        public Matrix LocalMatrix = Matrix.Identity;

        [DataMember(110)]
        [Display("Local translation")]
        public Vector3 Position { get { return translation; } set { translation = value; UpdateMatrix(); } }

        [DataMember(120)]
        [Display("Local rotation")]
        public Quaternion Rotation { get { return rotation; } set { rotation = value; UpdateMatrix(); } }

        [DataMember(130)]
        [Display("Local scale")]
        public Vector3 Scale { get { return scaling; } set { scaling = value; UpdateMatrix(); } }

        private Vector3 translation = Vector3.Zero;
        private Vector3 scaling = Vector3.One;
        private Quaternion rotation = Quaternion.Identity;

        private void UpdateMatrix()
        {
            // Rotation
            float xx = rotation.X * rotation.X;
            float yy = rotation.Y * rotation.Y;
            float zz = rotation.Z * rotation.Z;
            float xy = rotation.X * rotation.Y;
            float zw = rotation.Z * rotation.W;
            float zx = rotation.Z * rotation.X;
            float yw = rotation.Y * rotation.W;
            float yz = rotation.Y * rotation.Z;
            float xw = rotation.X * rotation.W;

            LocalMatrix.M11 = 1.0f - (2.0f * (yy + zz));
            LocalMatrix.M12 = 2.0f * (xy + zw);
            LocalMatrix.M13 = 2.0f * (zx - yw);
            LocalMatrix.M21 = 2.0f * (xy - zw);
            LocalMatrix.M22 = 1.0f - (2.0f * (zz + xx));
            LocalMatrix.M23 = 2.0f * (yz + xw);
            LocalMatrix.M31 = 2.0f * (zx + yw);
            LocalMatrix.M32 = 2.0f * (yz - xw);
            LocalMatrix.M33 = 1.0f - (2.0f * (yy + xx));

            // Position
            LocalMatrix.M41 = translation.X;
            LocalMatrix.M42 = translation.Y;
            LocalMatrix.M43 = translation.Z;

            // Scale
            if (scaling.X != 1.0f)
            {
                LocalMatrix.M11 *= scaling.X;
                LocalMatrix.M12 *= scaling.X;
                LocalMatrix.M13 *= scaling.X;
            }
            if (scaling.Y != 1.0f)
            {
                LocalMatrix.M21 *= scaling.Y;
                LocalMatrix.M22 *= scaling.Y;
                LocalMatrix.M23 *= scaling.Y;
            }
            if (scaling.Z != 1.0f)
            {
                LocalMatrix.M31 *= scaling.Z;
                LocalMatrix.M32 *= scaling.Z;
                LocalMatrix.M33 *= scaling.Z;
            }

            LocalMatrix.M14 = 0.0f;
            LocalMatrix.M24 = 0.0f;
            LocalMatrix.M34 = 0.0f;
            LocalMatrix.M44 = 1.0f;
        }

    }
}
