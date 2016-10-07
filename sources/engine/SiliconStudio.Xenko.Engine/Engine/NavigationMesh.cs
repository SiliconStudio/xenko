// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Core.Serialization.Serializers;
using SiliconStudio.Xenko.Extensions;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.GeometricPrimitives;
using SiliconStudio.Xenko.Native;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Materials;
using SiliconStudio.Xenko.Rendering.Materials.ComputeColors;

namespace SiliconStudio.Xenko.Engine
{
    [DataContract("NavigationMesh")]
    [DataSerializerGlobal(typeof(ReferenceSerializer<NavigationMesh>), Profile = "Content")]
    [DataSerializer(typeof(NavigationMeshSerializer))]
    [ContentSerializer(typeof(DataContentSerializer<NavigationMesh>))]
    public class NavigationMesh
    {
        // Initialized build settings, only used at build time
        [DataMemberIgnore]
        internal NavigationMeshBuildSettings BuildSettings;

        // Multiple layers corresponding to multiple agent settings
        [DataMemberCustomSerializer] public NavigationMeshLayer[] Layers;

        // Used internally to detect tile changes
        internal int TileHash;

        // Used internally to display the visuals in the GameStudio
        // these values are only used by the NavigationGizmo
        internal int DebugMeshTileHash;
        internal ModelComponent DebugMesh;
        
        public static Material CreateLayerDebugMaterial(GraphicsDevice device, int idx)
        {
            Material navmeshMaterial = Material.New(device, new MaterialDescriptor
            {
                Attributes =
                {
                    Diffuse = new MaterialDiffuseMapFeature(new ComputeColor()),
                    DiffuseModel = new MaterialDiffuseLambertModelFeature(),
                    Emissive = new MaterialEmissiveMapFeature(new ComputeColor()),
                }
            });

            Color4 deviceSpaceColor = new ColorHSV((idx * 80.0f + 90.0f) % 360.0f, 0.95f, 0.75f, 1.0f).ToColor().ToColorSpace(device.ColorSpace);

            // set the color to the material
            navmeshMaterial.Parameters.Set(MaterialKeys.DiffuseValue, deviceSpaceColor);
            navmeshMaterial.Parameters.Set(MaterialKeys.EmissiveIntensity, 1.0f);
            navmeshMaterial.Parameters.Set(MaterialKeys.EmissiveValue, deviceSpaceColor);
            navmeshMaterial.IsLightDependent = false;
            navmeshMaterial.HasTransparency = false;

            return navmeshMaterial;
        }

        public ModelComponent CreateDebugModelComponent(GraphicsDevice device, 
            List<GeometricPrimitive> generatedPrimitives)
        {
            Entity entity = new Entity("Navigation Debug Entity");

            Model model = new Model();
            for (int l = 0; l < Layers.Length; l++)
            {
                Material layerMaterial = CreateLayerDebugMaterial(device, l);
                model.Add(layerMaterial);
                foreach (var p in Layers[l].Tiles)
                {
                    NavigationMeshTile tile = p.Value;
                    if (tile.MeshVertices == null || tile.MeshVertices.Length == 0)
                        continue;

                    Vector3 offset = new Vector3(0.0f, 0.05f*l, 0.0f);

                    // Calculate mesh bounding box from navigation mesh points
                    BoundingBox bb = BoundingBox.FromPoints(tile.MeshVertices);

                    List<VertexPositionNormalTexture> vertexList = new List<VertexPositionNormalTexture>();
                    List<int> indexList = new List<int>();
                    for (int i = 0; i < tile.MeshVertices.Length; i++)
                    {
                        VertexPositionNormalTexture vert = new VertexPositionNormalTexture();
                        vert.Position = tile.MeshVertices[i] + offset;
                        vert.Normal = Vector3.UnitY;
                        vert.TextureCoordinate = new Vector2(0.5f, 0.5f);
                        vertexList.Add(vert);
                        indexList.Add(i);
                    }

                    MeshDraw draw;
                    using (var meshData = new GeometricMeshData<VertexPositionNormalTexture>(vertexList.ToArray(), indexList.ToArray(), true))
                    {
                        GeometricPrimitive primitive = new GeometricPrimitive(device, meshData);
                        generatedPrimitives.Add(primitive);
                        draw = primitive.ToMeshDraw();
                    }

                    Mesh mesh = new Mesh
                    {
                        Draw = draw,
                        MaterialIndex = l,
                    };
                    mesh.BoundingBox = bb;
                    model.Add(mesh);
                }
            }

            // Add a new model component
            return new ModelComponent(model);
        }

        /// <summary>
        /// Tries to find a built tile inside the navigation mesh
        /// </summary>
        /// <param name="layerIndex">The layer on which you want to search</param>
        /// <param name="tileCoordinate"></param>
        /// <returns>The found tile or null</returns>
        public NavigationMeshTile FindTile(int layerIndex, Point tileCoordinate)
        {
            if (layerIndex < 0 || layerIndex >= Layers.Length)
                return null;
            NavigationMeshLayer layer = Layers[layerIndex];
            NavigationMeshTile foundTile;
            if (layer.Tiles.TryGetValue(tileCoordinate, out foundTile))
                return foundTile;
            return null;
        }
        
        /// <summary>
        /// Used to initialize the navigation mesh so it will allow building tiles
        /// This will store the build settings and create the amount of layers corresponding to the number of NavigationAgentSettings
        /// </summary>
        /// <param name="buildSettings"></param>
        /// <param name="agentSettings">Agent setting to use</param>
        public void Initialize(NavigationMeshBuildSettings buildSettings, NavigationAgentSettings[] agentSettings)
        {
            this.BuildSettings = buildSettings;
            if (agentSettings.Length > 0)
            {
                Layers = new NavigationMeshLayer[agentSettings.Length];
                for (int i = 0; i < agentSettings.Length; i++)
                {
                    Layers[i] = new NavigationMeshLayer();
                    Layers[i].AgentSettings = agentSettings[i];
                }
            }
            else
                Layers = null;
        }

        /// <summary>
        /// Updates <see cref="TileHash"/> to be "unique" to this specific combination of navigation mesh
        /// </summary>
        internal void UpdateTileHash()
        {
            TileHash = 0;
            if (Layers == null)
                return;

            foreach (NavigationMeshLayer layer in Layers)
            {
                foreach (var tile in layer.Tiles)
                {
                    TileHash += tile.GetHashCode() + tile.Value.GetHashCode();
                }
            }
        }

        /// <summary>
        /// Build a specific navigation mesh tile for all layers
        /// </summary>
        /// <param name="tile">The tile to build</param>
        /// <param name="boundingBox">the bound</param>
        /// <param name="buildSettings"></param>
        /// <returns>A list of built tiles</returns>
        public List<NavigationMeshTile> BuildTile(Vector3[] inputVertices, int[] inputIndices,
            BoundingBox boundingBox, Point tileCoordinate)
        {
            List<NavigationMeshTile> builtTiles = new List<NavigationMeshTile>();

            for (int i = 0; i < Layers.Length; i++)
            {
                NavigationMeshLayer layer = Layers[i];
                NavigationMeshTile tile = BuildTileInternal(layer, inputVertices, inputIndices, boundingBox, tileCoordinate);
                
                // Remove old tile
                if (layer.Tiles.ContainsKey(tileCoordinate))
                    layer.Tiles.Remove(tileCoordinate);

                if (tile != null && tile.NavmeshData != null)
                {
                    // Add
                    layer.Tiles.Add(tileCoordinate, tile);
                    builtTiles.Add(tile);
                }
            }

            // Update tile hash
            UpdateTileHash();

            return builtTiles;
        }

        /// <summary>
        /// Build a single tile for a single layer
        /// </summary>
        /// <returns></returns>
        public NavigationMeshTile BuildLayerTile(int layerIndex,
            Vector3[] inputVertices, int[] inputIndices,
            BoundingBox boundingBox, Point tileCoordinate)
        {
            NavigationMeshLayer layer = Layers[layerIndex];

            // Remove old tile
            if (layer.Tiles.ContainsKey(tileCoordinate))
                layer.Tiles.Remove(tileCoordinate);

            NavigationMeshTile tile = BuildTileInternal(layer, inputVertices, inputIndices, boundingBox, tileCoordinate);
            if (tile != null && tile.NavmeshData != null)
            {
                // Add
                layer.Tiles.Add(tileCoordinate, tile);
            }
            
            // Update tile hash
            UpdateTileHash();

            return tile;
        }
        
        /// <summary>
        /// Builds a single tile for a given layer without adding it
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="inputVertices"></param>
        /// <param name="inputIndices"></param>
        /// <param name="boundingBox"></param>
        /// <param name="tileCoordinate"></param>
        /// <returns></returns>
        private unsafe NavigationMeshTile BuildTileInternal(NavigationMeshLayer layer,
            Vector3[] inputVertices, int[] inputIndices,
            BoundingBox boundingBox, Point tileCoordinate)
        {
            // Turn settings into native structure format
            NavigationAgentSettings agentSettings = layer.AgentSettings;
            Navigation.AgentSettings internalAgentSettings = new Navigation.AgentSettings
            {
                Height = agentSettings.Height,
                Radius = agentSettings.Radius,
                MaxClimb = agentSettings.MaxClimb,
                MaxSlope = agentSettings.MaxSlope.Degrees
            };
            NavigationMeshTile tile = new NavigationMeshTile();

            // Initialize navigation builder
            IntPtr nav = Navigation.CreateBuilder();

            // Turn build settings into native structure format
            Navigation.BuildSettings internalBuildSettings = new Navigation.BuildSettings
            {
                BoundingBox = boundingBox,
                TilePosition = tileCoordinate,
                CellHeight =  BuildSettings.CellHeight,
                CellSize = BuildSettings.CellSize,
                TileSize =  BuildSettings.TileSize
            };
            Navigation.SetSettings(nav, new IntPtr(&internalBuildSettings));
            Navigation.SetAgentSettings(nav, new IntPtr(&internalAgentSettings));
            
            // Generate mesh
            Navigation.GeneratedData data;
            IntPtr ret = Navigation.Build(nav, inputVertices.ToArray(), inputVertices.Length, inputIndices.ToArray(), inputIndices.Length);
            Navigation.GeneratedData* dataPtr = (Navigation.GeneratedData*)ret;
            data = *dataPtr;

            // Copy output data on success
            if (data.Success)
            {
                List<Vector3> outputVerts = new List<Vector3>();
                if (data.NumNavmeshVertices > 0)
                {
                    Vector3* navmeshVerts = (Vector3*)data.NavmeshVertices;
                    for (int j = 0; j < data.NumNavmeshVertices; j++)
                    {
                        outputVerts.Add(navmeshVerts[j]);
                    }

                    tile.MeshVertices = outputVerts.ToArray();
                }

                // Copy the generated navigationMesh data
                tile.NavmeshData = new byte[data.NavmeshDataLength];
                Marshal.Copy(data.NavmeshData, tile.NavmeshData, 0, data.NavmeshDataLength);
            }

            // Cleanup builder
            Navigation.DestroyBuilder(nav);

            return tile;
        }
        
        /// <summary>
        /// Removes a tile from a layer with given coordinate
        /// </summary>
        /// <param name="layerIndex"></param>
        /// <param name="tileCoordinate"></param>
        public void RemoveLayerTile(int layerIndex, Point tileCoordinate)
        {
            if (layerIndex < 0 || layerIndex >= Layers.Length)
                return;
            NavigationMeshLayer layer = Layers[layerIndex];
            layer.Tiles.Remove(tileCoordinate);
        }
    }
    
    internal class NavigationMeshSerializer : DataSerializer<NavigationMesh>, IDataSerializerInitializer
    {
        private DictionarySerializer<Point, NavigationMeshTile> tilesSerializer;

        public void Initialize(SerializerSelector serializerSelector)
        {
            tilesSerializer = new DictionarySerializer<Point, NavigationMeshTile>();
            tilesSerializer.Initialize(serializerSelector);
        }
        public override void Serialize(ref NavigationMesh obj, ArchiveMode mode, SerializationStream stream)
        {
            // Serialize tile size because it is needed
            stream.Serialize(ref obj.BuildSettings.TileSize);
            stream.Serialize(ref obj.BuildSettings.CellSize);

            int numLayers = obj.Layers?.Length ?? 0;
            stream.Serialize(ref numLayers);
            if (mode == ArchiveMode.Deserialize)
                obj.Layers = new NavigationMeshLayer[numLayers];

            for (int l = 0; l < numLayers; l++)
            {
                NavigationMeshLayer layer;
                if (mode == ArchiveMode.Deserialize)
                    layer = obj.Layers[l] = new NavigationMeshLayer();
                else
                    layer = obj.Layers[l];
                
                tilesSerializer.Serialize(ref layer.Tiles, mode, stream);
            }

            if(mode == ArchiveMode.Deserialize)
                obj.UpdateTileHash();
        }
    }
}