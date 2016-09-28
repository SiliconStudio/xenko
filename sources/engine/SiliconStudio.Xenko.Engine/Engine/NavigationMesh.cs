// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using SiliconStudio.Xenko.Extensions;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.GeometricPrimitives;
using SiliconStudio.Xenko.Native;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Materials;
using SiliconStudio.Xenko.Rendering.Materials.ComputeColors;

namespace SiliconStudio.Xenko.Engine
{

    [DataContract]
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct NavigationMeshBuildSettings
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

        // Bounding box for the generated navigation mesh
        public BoundingBox BoundingBox;
        // Settings for agent used with this navigationMesh
        public AgentSettings NavigationMeshAgentSettings;
        // Grid settings
        public float CellHeight;
        public float CellSize;
    };

    [DataContract("NavigationMesh")]
    [DataSerializerGlobal(typeof(ReferenceSerializer<NavigationMesh>), Profile = "Content")]
    [DataSerializer(typeof(NavigationMeshSerializer))]
    [ContentSerializer(typeof(DataContentSerializer<NavigationMesh>))]
    public class NavigationMesh
    {
        [DataMemberCustomSerializer]
        public Vector3[] MeshVertices;
        [DataMemberCustomSerializer]
        public byte[] NavmeshData;

        public ModelComponent CreateDebugModelComponent(GraphicsDevice device)
        {
            Material navmeshMaterial = Material.New(device, new MaterialDescriptor
            {
                Attributes =
                {
                    Diffuse = new MaterialDiffuseMapFeature(new ComputeColor()),
                    DiffuseModel = new MaterialDiffuseLambertModelFeature(),
                    Emissive = new MaterialEmissiveMapFeature(new ComputeColor()),
                    Transparency = new MaterialTransparencyBlendFeature(),
                }
            });
            
            Color baseColor = Color.AdjustSaturation(Color.Blue, 0.77f).WithAlpha(128);
            Color4 deviceSpaceColor = new Color4(baseColor).ToColorSpace(device.ColorSpace);
            
            // set the color to the material
            navmeshMaterial.Parameters.Set(MaterialKeys.DiffuseValue, deviceSpaceColor);
            navmeshMaterial.Parameters.Set(MaterialKeys.EmissiveIntensity, 1.0f);
            navmeshMaterial.Parameters.Set(MaterialKeys.EmissiveValue, deviceSpaceColor);
            navmeshMaterial.IsLightDependent = false;
            
            navmeshMaterial.HasTransparency = true;
            Entity entity = new Entity("Navigation Debug Entity");
            
            if(MeshVertices != null)
            {
                // Calculate mesh bounding box from navigation mesh points
                BoundingBox bb = BoundingBox.FromPoints(MeshVertices);

                List<VertexPositionNormalTexture> vertexList = new List<VertexPositionNormalTexture>();
                List<int> indexList = new List<int>();
                for (int i = 0; i < MeshVertices.Length; i++)
                {
                    VertexPositionNormalTexture vert = new VertexPositionNormalTexture();
                    vert.Position = MeshVertices[i];
                    vert.Normal = Vector3.UnitY;
                    vert.TextureCoordinate = new Vector2(0.5f, 0.5f);
                    vertexList.Add(vert);
                    indexList.Add(i);
                }

                var meshData = new GeometricMeshData<VertexPositionNormalTexture>(vertexList.ToArray(), indexList.ToArray(), true);
                GeometricPrimitive primitive = new GeometricPrimitive(device, meshData);
                MeshDraw draw = primitive.ToMeshDraw();
                Mesh mesh = new Mesh
                {
                    Draw = draw
                };
                mesh.BoundingBox = bb;
                
                // Add a new model component
                return new ModelComponent
                {
                    Model = new Model { navmeshMaterial, mesh }
                };
            }
            return null;
        }

        public bool Build(NavigationMeshBuildSettings settings, Vector3[] inputVertices, int[] inputIndices)
        {
            // Clear data
            NavmeshData = null;
            MeshVertices = null;

            // Initialize navigation builder
            IntPtr nav = Navigation.CreateBuilder();

            // Set settings, passed by pointer, since the type is defined here
            IntPtr settingsIntPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(NavigationMeshBuildSettings)));
            Marshal.StructureToPtr(settings, settingsIntPtr, false);
            Navigation.SetSettings(nav, settingsIntPtr);
            Marshal.DestroyStructure(settingsIntPtr, typeof(NavigationMeshBuildSettings));
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
            
            // Copy the generated navigationMesh data
            NavmeshData = new byte[data.NavmeshDataLength];
            Marshal.Copy(data.NavmeshData, NavmeshData, 0, data.NavmeshDataLength);

            Navigation.DestroyBuilder(nav);
            return true;
        }
    }

    internal class NavigationMeshSerializer : DataSerializer<NavigationMesh>, IDataSerializerInitializer
    {
        private DataSerializer<Vector3> pointSerializer;
        
        public void Initialize(SerializerSelector serializerSelector)
        {
            pointSerializer = MemberSerializer<Vector3>.Create(serializerSelector);
        }
        
        public override void Serialize(ref NavigationMesh obj, ArchiveMode mode, SerializationStream stream)
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
