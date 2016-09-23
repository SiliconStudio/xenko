using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Audio;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.GeometricPrimitives;
using SiliconStudio.Xenko.Native;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Engine
{
    [DataContract]
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct AgentSettings
    {
        public float Height;
        public float Radius;
        public float MaxClimb;
        [DataMemberRange(0.0f, 180.0f, 0.1f, 1.0f, AllowNaN = false)]
        public float MaxSlope;
    }

    [DataContract]
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct NavmeshBuildSettings
    {
        // Bounding box for the generated navigation mesh
        public BoundingBox BoundingBox;
        // Settings for agent used with this navmesh
        public AgentSettings AgentSettings;
        // Grid settings
        public float CellHeight;
        public float CellSize;
    };

    [DataContract("Navmesh")]
    [DataSerializerGlobal(typeof(ReferenceSerializer<Navmesh>), Profile = "Content")]
    [DataSerializer(typeof(NavmeshSerializer))]
    [ContentSerializer(typeof(DataContentSerializer<Navmesh>))]
    public class Navmesh
    {
        [DataMemberCustomSerializer]
        public Vector3[] MeshVertices;
        [DataMemberCustomSerializer]
        public byte[] NavmeshData;

        public bool Build(NavmeshBuildSettings settings, Vector3[] inputVertices, int[] inputIndices)
        {
            // Clear data
            NavmeshData = null;
            MeshVertices = null;

            // Initialize navigation builder
            IntPtr nav = Navigation.CreateBuilder();

            // Set settings, passed by pointer, since the type is defined here
            IntPtr settingsIntPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(NavmeshBuildSettings)));
            Marshal.StructureToPtr(settings, settingsIntPtr, false);
            Navigation.SetSettings(nav, settingsIntPtr);
            Marshal.DestroyStructure(settingsIntPtr, typeof(NavmeshBuildSettings));
            Marshal.FreeCoTaskMem(settingsIntPtr);

            // Generate mesh
            Navigation.GeneratedData data;
            unsafe
            {
                IntPtr ret = Navigation.Build(nav, inputVertices.ToArray(), inputVertices.Length, inputIndices.ToArray(), inputIndices.Length);
                Navigation.GeneratedData* dataPtr = (Navigation.GeneratedData*)ret;
                data = *dataPtr;
            }
            if (!data.Success)
                return false;

            //List<VertexPositionNormalTexture> outputVerts = new List<VertexPositionNormalTexture>();
            List<Vector3> outputVerts = new List<Vector3>();
            List<int> indices = new List<int>();
            if (data.NumNavmeshVertices > 0)
            {
                unsafe
                {
                    Vector3* navmeshVerts = (Vector3*)data.NavmeshVertices;
                    for (int i = 0; i < data.NumNavmeshVertices; i++)
                    {
                        outputVerts.Add(navmeshVerts[i]);
                        indices.Add(i);
                    }
                }

                // Mark as left handed since this is the recast output
                MeshVertices = outputVerts.ToArray();
            }
            
            // Copy the generated navmesh data
            NavmeshData = new byte[data.NavmeshDataLength];
            Marshal.Copy(data.NavmeshData, NavmeshData, 0, data.NavmeshDataLength);

            Navigation.DestroyBuilder(nav);
            return true;
        }
    }

    internal class NavmeshSerializer : DataSerializer<Navmesh>, IDataSerializerInitializer
    {
        private DataSerializer<string> stringSerializer;
        private DataSerializer<Vector3> pointSerializer;

        /// <inheritdoc/>
        public void Initialize(SerializerSelector serializerSelector)
        {
            stringSerializer = MemberSerializer<string>.Create(serializerSelector);
            pointSerializer = MemberSerializer<Vector3>.Create(serializerSelector);
        }
        
        public override void Serialize(ref Navmesh obj, ArchiveMode mode, SerializationStream stream)
        {
            if(mode == ArchiveMode.Serialize)
            {
                int vl = obj.MeshVertices?.Length ?? 0;
                stream.Serialize(ref vl);

                for(int i = 0; i < vl; i++)
                {
                    pointSerializer.Serialize(obj.MeshVertices[i], stream);
                }
                
                int nml = obj.NavmeshData?.Length ?? 0;
                stream.Serialize(ref nml);

                if(nml > 0)
                    stream.Serialize(obj.NavmeshData, 0, obj.NavmeshData.Length);
            }
            else
            {
                int vl = 0;
                stream.Serialize(ref vl);

                if(vl > 0)
                {
                    obj.MeshVertices = new Vector3[vl];

                    for(int i = 0; i < vl; i++)
                    {
                        pointSerializer.Serialize(ref obj.MeshVertices[i], mode, stream);
                    }
                }

                int nml = 0;
                stream.Serialize(ref nml);

                if(nml > 0)
                {
                    obj.NavmeshData = new byte[nml];
                    stream.Serialize(obj.NavmeshData, 0, obj.NavmeshData.Length);
                }
            }
        }
    }
}
