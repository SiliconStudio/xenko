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
        // Bounding box for the generated navigation mesh
        public BoundingBox BoundingBox;

        // Grid settings
        public float CellHeight;
        public float CellSize;
    };


    [DataContract]
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct NavigationAgentSettings
    {
        [DefaultValue(1.0f)]
        public float Height;
        [DefaultValue(1.0f)]
        public float Radius;
        [DefaultValue(0.25f)]
        public float MaxClimb;
        [DataMemberRange(0.0f, 180.0f, 0.1f, 1.0f, AllowNaN = false)]
        [DefaultValue(45.0f)]
        public float MaxSlope;
    }

    [DataContract("NavigationMesh")]
    [DataSerializerGlobal(typeof(ReferenceSerializer<NavigationMesh>), Profile = "Content")]
    [DataSerializer(typeof(NavigationMeshSerializer))]
    [ContentSerializer(typeof(DataContentSerializer<NavigationMesh>))]
    public class NavigationMesh
    {
        public class Layer
        {
            public Vector3[] MeshVertices;
            public byte[] NavmeshData;
        }
        [DataMemberCustomSerializer]
        public Layer[] Layers;

        public Material CreateLayerDebugMaterial(GraphicsDevice device, int idx)
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
            
            Color4 deviceSpaceColor = new ColorHSV((idx * 80.0f + 90.0f) % 360.0f, 0.7f, 0.5f, 0.5f).ToColor().ToColorSpace(device.ColorSpace);

            // set the color to the material
            navmeshMaterial.Parameters.Set(MaterialKeys.DiffuseValue, deviceSpaceColor);
            navmeshMaterial.Parameters.Set(MaterialKeys.EmissiveIntensity, 1.0f);
            navmeshMaterial.Parameters.Set(MaterialKeys.EmissiveValue, deviceSpaceColor);
            navmeshMaterial.IsLightDependent = false;

            navmeshMaterial.HasTransparency = true;
            return navmeshMaterial;
        }
        public ModelComponent CreateDebugModelComponent(GraphicsDevice device)
        {
            Entity entity = new Entity("Navigation Debug Entity");

            Model model = new Model();
            for (int l = 0; l < Layers.Length; l++)
            {
                Vector3 offset = new Vector3(0.0f, 0.025f*l, 0.0f);
                Layer layer = Layers[l];

                // Calculate mesh bounding box from navigation mesh points
                BoundingBox bb = BoundingBox.FromPoints(layer.MeshVertices);

                List<VertexPositionNormalTexture> vertexList = new List<VertexPositionNormalTexture>();
                List<int> indexList = new List<int>();
                for (int i = 0; i < layer.MeshVertices.Length; i++)
                {
                    VertexPositionNormalTexture vert = new VertexPositionNormalTexture();
                    vert.Position = layer.MeshVertices[i] + offset;
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
                    Draw = draw,
                    MaterialIndex = l,
                };
                mesh.BoundingBox = bb;
                model.Add(mesh);
                model.Add(CreateLayerDebugMaterial(device, l));
            }

            // Add a new model component
            return new ModelComponent(model);
        }

        public unsafe bool Build(NavigationMeshBuildSettings settings, NavigationAgentSettings[] agentSettings, Vector3[] inputVertices, int[] inputIndices)
        {
            // Clear data
            Layers = null;

            // No agent settings
            if (agentSettings.Length == 0)
                return true;

            Layers = new Layer[agentSettings.Length];

            // Initialize navigation builder
            IntPtr nav = Navigation.CreateBuilder();
            Navigation.SetSettings(nav, new IntPtr(&settings));

            // Generate each layer
            for (int i = 0; i < agentSettings.Length; i++)
            {
                Layer layer = new Layer();

                NavigationAgentSettings currentAgentSettings = agentSettings[i];
                Navigation.SetAgentSettings(nav, new IntPtr(&currentAgentSettings));

                // Generate mesh
                Navigation.GeneratedData data;
                IntPtr ret = Navigation.Build(nav, inputVertices.ToArray(), inputVertices.Length, inputIndices.ToArray(), inputIndices.Length);
                Navigation.GeneratedData* dataPtr = (Navigation.GeneratedData*)ret;
                data = *dataPtr;
                if (!data.Success)
                    return false;

                List<Vector3> outputVerts = new List<Vector3>();
                if (data.NumNavmeshVertices > 0)
                {
                    Vector3* navmeshVerts = (Vector3*)data.NavmeshVertices;
                    for (int j = 0; j < data.NumNavmeshVertices; j++)
                    {
                        outputVerts.Add(navmeshVerts[j]);
                    }
                    
                    layer.MeshVertices = outputVerts.ToArray();
                }

                // Copy the generated navigationMesh data
                layer.NavmeshData = new byte[data.NavmeshDataLength];
                Marshal.Copy(data.NavmeshData, layer.NavmeshData, 0, data.NavmeshDataLength);

                Layers[i] = layer;
            }

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
                int numLayers = obj.Layers?.Length ?? 0;
                stream.Serialize(ref numLayers);

                for (int l = 0; l < numLayers; l++)
                {
                    NavigationMesh.Layer layer = obj.Layers[l];

                    int vl = layer.MeshVertices?.Length ?? 0;
                    stream.Serialize(ref vl);

                    for (int i = 0; i < vl; i++)
                    {
                        pointSerializer.Serialize(layer.MeshVertices[i], stream);
                    }

                    int nml = layer.NavmeshData?.Length ?? 0;
                    stream.Serialize(ref nml);

                    if (nml > 0)
                        stream.Serialize(layer.NavmeshData, 0, layer.NavmeshData.Length);
                }
            }
            else
            {
                int numLayers = 0;
                stream.Serialize(ref numLayers);

                obj.Layers = new NavigationMesh.Layer[numLayers];
                for (int l = 0; l < numLayers; l++)
                {
                    NavigationMesh.Layer layer = obj.Layers[l] = new NavigationMesh.Layer();

                    int vl = 0;
                    stream.Serialize(ref vl);

                    if (vl > 0)
                    {
                        layer.MeshVertices = new Vector3[vl];
                        for (int i = 0; i < vl; i++)
                        {
                            pointSerializer.Serialize(ref layer.MeshVertices[i], mode, stream);
                        }
                    }

                    int nml = 0;
                    stream.Serialize(ref nml);

                    if (nml > 0)
                    {
                        layer.NavmeshData = new byte[nml];
                        stream.Serialize(layer.NavmeshData, 0, layer.NavmeshData.Length);
                    }
                }
            }
        }
    }
}
