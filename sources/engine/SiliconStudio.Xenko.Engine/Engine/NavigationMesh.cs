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
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Core.Serialization.Serializers;
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
    public struct NavigationMeshBuildSettings
    {
        /// <summary>
        /// The Height of a grid cell in the navigation mesh building steps using heightfields
        /// A lower number means higher resolution on the vertical axis but longer build times
        /// </summary>
        public float CellHeight;
        /// <summary>
        /// The Width/Height of a grid cell in the navigation mesh building steps using heightfields
        /// A lower number means higher resolution on the horizontal axes but longer build times
        /// </summary>
        public float CellSize;

        /// <summary>
        /// Tile size used for Navigation mesh tiles, the final size of a grid tile is CellSize*TileSize
        /// </summary>
        public int TileSize;

        public override int GetHashCode()
        {
            return CellHeight.GetHashCode() + CellSize.GetHashCode() + TileSize.GetHashCode();
        }
    };
    
    [DataContract]
    [ObjectFactory(typeof(NavigationAgentSettingsFactory))]
    public struct NavigationAgentSettings
    {
        public float Height;
        public float Radius;

        /// <summary>
        /// Maximum vertical distance this agent can climb
        /// </summary>
        public float MaxClimb;

        /// <summary>
        /// Maximum slope angle this agent can climb (in degrees)
        /// </summary>
        public AngleSingle MaxSlope;

        public override int GetHashCode()
        {
            return Height.GetHashCode() + Radius.GetHashCode() + MaxClimb.GetHashCode() + MaxSlope.GetHashCode();
        }
    }

    public class NavigationAgentSettingsFactory : IObjectFactory
    {
        public object New(Type type)
        {
            return new NavigationAgentSettings
            {
                Height = 1.0f,
                MaxClimb = 0.25f,
                MaxSlope = new AngleSingle(45.0f, AngleType.Degree),
                Radius = 0.5f
            };
        }
    }

    [DataContract("NavigationMesh")]
    [DataSerializerGlobal(typeof(ReferenceSerializer<NavigationMesh>), Profile = "Content")]
    [DataSerializer(typeof(NavigationMeshSerializer))]
    [ContentSerializer(typeof(DataContentSerializer<NavigationMesh>))]
    public class NavigationMesh
    {
        [DataContract("NavigationMeshTile")]
        [DataSerializer(typeof(NavigationMeshTileSerializer))]
        public class Tile
        {
            [DataMemberCustomSerializer]
            public Vector3[] MeshVertices;
            [DataMemberCustomSerializer]
            public byte[] NavmeshData;

            public override int GetHashCode()
            {
                return MeshVertices?.ComputeHash() ?? 0;
            }
        }

        public class Layer
        {
            /// <summary>
            /// Tiles generated for this layer
            /// </summary>
            public Dictionary<Point, Tile> Tiles = new Dictionary<Point, Tile>();

            /// <summary>
            /// Agent settings for generating this layer
            /// </summary>
            internal NavigationAgentSettings AgentSettings;
        }
        
        // Build settings specified while bulding
        [DataMemberIgnore]
        internal NavigationMeshBuildSettings BuildSettings;

        // Multiple layers corresponding to multiple agent settings
        [DataMemberCustomSerializer] public Layer[] Layers;

        // Used internally to detect tile changes
        internal int TileHash;

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
                    Tile tile = p.Value;
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
        /// Check which tiles overlap a given bounding box
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="boundingBox"></param>
        /// <returns></returns>
        public static List<Point> GetOverlappingTiles(NavigationMeshBuildSettings settings, BoundingBox boundingBox)
        {
            List<Point> ret = new List<Point>();
            float tcs = settings.TileSize * settings.CellSize;
            Vector2 start = boundingBox.Minimum.XZ() / tcs;
            Vector2 end = boundingBox.Maximum.XZ() / tcs;
            Point startTile = new Point(
                (int)Math.Floor(start.X),
                (int)Math.Floor(start.Y));
            Point endTile = new Point(
                (int)Math.Ceiling(end.X),
                (int)Math.Ceiling(end.Y));
            for (int y = startTile.Y; y < endTile.Y; y++)
            {
                for (int x = startTile.X; x < endTile.X; x++)
                {
                    ret.Add(new Point(x, y));
                }
            }
            return ret;
        }

        /// <summary>
        /// Clamps X-Z coordinates to a navigation mesh tile
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="boundingBox"></param>
        /// <param name="tileCoord"></param>
        /// <returns></returns>
        public static BoundingBox ClampBoundingBoxToTile(NavigationMeshBuildSettings settings, BoundingBox boundingBox, Point tileCoord)
        {
            float tcs = settings.TileSize * settings.CellSize;
            Vector2 tileMin = new Vector2(tileCoord.X* tcs, tileCoord.Y* tcs);
            Vector2 tileMax = tileMin +  new Vector2(tcs);

            boundingBox.Minimum.X = tileMin.X;
            boundingBox.Minimum.Z = tileMin.Y;
            boundingBox.Maximum.X = tileMax.X;
            boundingBox.Maximum.Z = tileMax.Y;
            return boundingBox;
        }

        public Tile FindTile(int layerIndex, Point tileCoordinate)
        {
            if (layerIndex < 0 || layerIndex >= Layers.Length)
                return null;
            Layer layer = Layers[layerIndex];
            Tile foundTile;
            if (layer.Tiles.TryGetValue(tileCoordinate, out foundTile))
                return foundTile;
            return null;
        }

        public void ClearTiles()
        {
            if (Layers == null)
                return;

            foreach(var layer in Layers)
            {
                layer.Tiles.Clear();
            }
        }

        public void Initialize(NavigationMeshBuildSettings buildSettings, NavigationAgentSettings[] agentSettings)
        {
            this.BuildSettings = buildSettings;
            if (agentSettings.Length > 0)
            {
                Layers = new Layer[agentSettings.Length];
                for (int i = 0; i < agentSettings.Length; i++)
                {
                    Layers[i] = new Layer();
                    Layers[i].AgentSettings = agentSettings[i];
                }
            }
            else
                Layers = null;
        }

        internal void UpdateTileHash()
        {
            TileHash = 0;
            if (Layers == null)
                return;

            foreach (Layer layer in Layers)
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
        public List<Tile> BuildTile(Vector3[] inputVertices, int[] inputIndices,
            BoundingBox boundingBox, Point tileCoordinate)
        {
            List<Tile> builtTiles = new List<Tile>();

            for (int i = 0; i < Layers.Length; i++)
            {
                Layer layer = Layers[i];
                Tile tile = BuildTileInternal(layer, inputVertices, inputIndices, boundingBox, tileCoordinate);
                
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
        public Tile BuildLayerTile(int layerIndex,
            Vector3[] inputVertices, int[] inputIndices,
            BoundingBox boundingBox, Point tileCoordinate)
        {
            Layer layer = Layers[layerIndex];

            // Remove old tile
            if (layer.Tiles.ContainsKey(tileCoordinate))
                layer.Tiles.Remove(tileCoordinate);

            Tile tile = BuildTileInternal(layer, inputVertices, inputIndices, boundingBox, tileCoordinate);
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
        private unsafe Tile BuildTileInternal(Layer layer,
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
            Tile tile = new Tile();

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
            Layer layer = Layers[layerIndex];
            layer.Tiles.Remove(tileCoordinate);
        }
    }

    /// <summary>
    /// Serializes individually build tiles inside navigation meshes
    /// </summary>
    internal class NavigationMeshTileSerializer : DataSerializer<NavigationMesh.Tile>, IDataSerializerInitializer
    {
        private DataSerializer<Vector3> pointSerializer;

        public void Initialize(SerializerSelector serializerSelector)
        {
            pointSerializer = MemberSerializer<Vector3>.Create(serializerSelector);
        }
        public override void Serialize(ref NavigationMesh.Tile tile, ArchiveMode mode, SerializationStream stream)
        {
            if(mode == ArchiveMode.Deserialize)
                tile = new NavigationMesh.Tile();

            int numMeshVertices = tile.MeshVertices?.Length ?? 0;
            stream.Serialize(ref numMeshVertices);
            if (mode == ArchiveMode.Deserialize)
                tile.MeshVertices = new Vector3[numMeshVertices];
            
            for (int i = 0; i < numMeshVertices; i++)
            {
                pointSerializer.Serialize(ref tile.MeshVertices[i], mode, stream);
            }
            
            int dataLength = tile.NavmeshData?.Length ?? 0;
            stream.Serialize(ref dataLength);
            if (mode == ArchiveMode.Deserialize)
                tile.NavmeshData = new byte[dataLength];
                    
            if (dataLength > 0)
                stream.Serialize(tile.NavmeshData, 0, tile.NavmeshData.Length);
        }
    }

    /// <summary>
    /// Serializes navigation meshes
    /// </summary>
    internal class NavigationMeshSerializer : DataSerializer<NavigationMesh>, IDataSerializerInitializer
    {
        private DictionarySerializer<Point, NavigationMesh.Tile> tilesSerializer;

        public void Initialize(SerializerSelector serializerSelector)
        {
            tilesSerializer = new DictionarySerializer<Point, NavigationMesh.Tile>();
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
                obj.Layers = new NavigationMesh.Layer[numLayers];

            for (int l = 0; l < numLayers; l++)
            {
                NavigationMesh.Layer layer;
                if (mode == ArchiveMode.Deserialize)
                    layer = obj.Layers[l] = new NavigationMesh.Layer();
                else
                    layer = obj.Layers[l];
                
                tilesSerializer.Serialize(ref layer.Tiles, mode, stream);
            }

            if(mode == ArchiveMode.Deserialize)
                obj.UpdateTileHash();
        }
    }
}