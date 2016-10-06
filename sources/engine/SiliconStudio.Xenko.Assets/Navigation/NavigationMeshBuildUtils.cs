using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Physics;

namespace SiliconStudio.Xenko.Assets.Navigation
{
    public class NavigationMeshBuildUtils
    {
        public static void GenerateTangentBinormal(Vector3 normal, out Vector3 tangent, out Vector3 bitangent)
        {
            if (Math.Abs(normal.Y) < 0.01f)
                tangent = new Vector3(normal.Z, normal.Y, -normal.X);
            else
                tangent = new Vector3(-normal.Y, normal.X, normal.Z);
            tangent.Normalize();
            bitangent = Vector3.Cross(normal, tangent);
            tangent = Vector3.Cross(bitangent, normal);
        }

        public static void BuildPlanePoints(ref Plane plane, float size, out Vector3[] points, out int[] inds)
        {
            Vector3 up = plane.Normal;
            Vector3 right;
            Vector3 forward;
            GenerateTangentBinormal(up, out right, out forward);

            points = new Vector3[4];
            points[0] = -forward * size - right * size + up * plane.D;
            points[1] = -forward * size + right * size + up * plane.D;
            points[2] = forward * size - right * size + up * plane.D;
            points[3] = forward * size + right * size + up * plane.D;

            inds = new int[6];
            // CCW
            inds[0] = 0;
            inds[1] = 2;
            inds[2] = 1;
            inds[3] = 1;
            inds[4] = 2;
            inds[5] = 3;
        }

        public static void ExtendBoundingBox(ref BoundingBox boundingBox, Vector3 offsets)
        {
            boundingBox.Minimum -= offsets;
            boundingBox.Maximum += offsets;
        }



        /// <summary>
        /// Hashes and entity's transform and it's collider shape settings
        /// </summary>
        /// <param name="collider"></param>
        /// <returns></returns>
        public static int HashEntityCollider(StaticColliderComponent collider)
        {
            int hash = 0;
            hash += collider.Entity.Transform.WorldMatrix.GetHashCode();
            hash += 379 * collider.CollisionGroup.GetHashCode();
            foreach (var shape in collider.ColliderShapes)
            {
                hash += shape.GetHashCode();
            }
            return hash;
        }
    }
}
