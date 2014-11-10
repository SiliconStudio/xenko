﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Paradox.Effects.Data;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Graphics.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

﻿using SiliconStudio.Paradox.Physics;

﻿using VHACDSharp;

namespace SiliconStudio.Paradox.Assets.Physics
{
    internal class ColliderShapeAssetCompiler : AssetCompilerBase<ColliderShapeAsset>
    {
        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, ColliderShapeAsset asset, AssetCompilerResult result)
        {
            result.BuildSteps = new ListBuildStep
            {
                new ColliderShapeCombineCommand(urlInStorage, asset),
            };

            result.ShouldWaitForPreviousBuilds = asset.Data.ColliderShapes.Any(shape => shape.GetType() == typeof(ConvexHullColliderShapeDesc));
        }

        private class ColliderShapeCombineCommand : AssetCommand<ColliderShapeAsset>
        {
            public ColliderShapeCombineCommand(string url, ColliderShapeAsset asset)
                : base(url, asset)
            {
            }

            private ConvexHullMesh convexHullMesh;

            public override void Cancel()
            {
                lock (this)
                {
                    if (convexHullMesh != null) convexHullMesh.Cancel();
                }
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                var assetManager = new AssetManager();
                
                //pre process special types
                foreach (var shape in asset.Data.ColliderShapes)
                {
                    var type = shape.GetType();
                    if (type == typeof(ConvexHullColliderShapeDesc))
                    {
                        var convexHullDesc = (ConvexHullColliderShapeDesc)shape;

                        //decompose and fill vertex data
                        if (convexHullDesc.Model != null)
                        {
                            var loadSettings = new AssetManagerLoaderSettings
                            {
                                ContentFilter = AssetManagerLoaderSettings.NewContentFilterByType(typeof(MeshData))
                            };

                            var modelAsset = assetManager.Load<ModelData>(convexHullDesc.Model.Location, loadSettings);
                            if (modelAsset != null)
                            {
                                convexHullDesc.ConvexHulls = new List<List<List<Vector3>>>();
                                convexHullDesc.ConvexHullsIndices = new List<List<List<uint>>>();

                                commandContext.Logger.Info("Processing convex hull generation, this might take a while!");

                                var nodeTransforms = new List<Matrix>();

                                //pre-compute all node transforms, assuming nodes are ordered... see ModelViewHierarchyUpdater
                                var nodesLength = modelAsset.Hierarchy.Nodes.Length;
                                for (var i = 0; i < nodesLength; i++)
                                {
                                    Matrix localMatrix;
                                    TransformationComponent.CreateMatrixTRS(
                                        ref modelAsset.Hierarchy.Nodes[i].Transform.Translation, 
                                        ref modelAsset.Hierarchy.Nodes[i].Transform.Rotation, 
                                        ref modelAsset.Hierarchy.Nodes[i].Transform.Scaling, out localMatrix);

                                    Matrix worldMatrix;
                                    if (modelAsset.Hierarchy.Nodes[i].ParentIndex != -1)
                                    {
                                        var nodeTransform = nodeTransforms[modelAsset.Hierarchy.Nodes[i].ParentIndex];
                                        Matrix.Multiply(ref localMatrix, ref nodeTransform, out worldMatrix);
                                    }
                                    else
                                    {
                                        worldMatrix = localMatrix;
                                    }

                                    nodeTransforms.Add(worldMatrix);
                                }

                                for (var i = 0; i < nodesLength; i++)
                                {
                                    var i1 = i;
                                    if (modelAsset.Meshes.All(x => x.NodeIndex != i1)) continue; // no geometry in the node

                                    var combinedVerts = new List<float>();
                                    var combinedIndices = new List<uint>();

                                    var hullsList = new List<List<Vector3>>();
                                    convexHullDesc.ConvexHulls.Add(hullsList);

                                    var indicesList = new List<List<uint>>();
                                    convexHullDesc.ConvexHullsIndices.Add(indicesList);

                                    foreach (var meshData in modelAsset.Meshes.Where(x => x.NodeIndex == i1))
                                    {
                                        var indexOffset = (uint)combinedVerts.Count / 3;

                                        var stride = meshData.Draw.VertexBuffers[0].Declaration.VertexStride;
                                        var vertexDataAsset = assetManager.Load<BufferData>(meshData.Draw.VertexBuffers[0].Buffer.Location);

                                        var vertexData = vertexDataAsset.Content;
                                        var vertexIndex = meshData.Draw.VertexBuffers[0].Offset;
                                        for (var v = 0; v < meshData.Draw.VertexBuffers[0].Count; v++)
                                        {    
                                            var posMatrix = Matrix.Translation(new Vector3(BitConverter.ToSingle(vertexData, vertexIndex + 0), BitConverter.ToSingle(vertexData, vertexIndex + 4), BitConverter.ToSingle(vertexData, vertexIndex + 8)));

                                            Matrix rotatedMatrix;
                                            var nodeTransform = nodeTransforms[i];
                                            Matrix.Multiply(ref posMatrix, ref nodeTransform, out rotatedMatrix);

                                            combinedVerts.Add(rotatedMatrix.TranslationVector.X);
                                            combinedVerts.Add(rotatedMatrix.TranslationVector.Y);
                                            combinedVerts.Add(rotatedMatrix.TranslationVector.Z);

                                            vertexIndex += stride;
                                        }

                                        var indexDataAsset = assetManager.Load<BufferData>(meshData.Draw.IndexBuffer.Buffer.Location);

                                        var indexData = indexDataAsset.Content;
                                        var indexIndex = meshData.Draw.IndexBuffer.Offset;                                 
                                        for (var v = 0; v < meshData.Draw.IndexBuffer.Count; v++)
                                        {
                                            if (meshData.Draw.IndexBuffer.Is32Bit)
                                            {
                                                combinedIndices.Add(BitConverter.ToUInt32(indexData, indexIndex) + indexOffset);
                                                indexIndex += 4;
                                            }
                                            else
                                            {
                                                combinedIndices.Add(BitConverter.ToUInt16(indexData, indexIndex) + indexOffset);
                                                indexIndex += 2;
                                            }
                                        }
                                    }

                                    var decompositionDesc = new ConvexHullMesh.DecompositionDesc
                                    {
                                        VertexCount = (uint)combinedVerts.Count / 3,
                                        IndicesCount = (uint)combinedIndices.Count,
                                        Vertexes = combinedVerts.ToArray(),
                                        Indices = combinedIndices.ToArray(),
                                        Depth = convexHullDesc.Depth,
                                        PosSampling = convexHullDesc.PosSampling,
                                        PosRefine = convexHullDesc.PosRefine,
                                        AngleSampling = convexHullDesc.AngleSampling,
                                        AngleRefine = convexHullDesc.AngleRefine,
                                        Alpha = convexHullDesc.Alpha,
                                        Threshold = convexHullDesc.Threshold,
                                        SimpleHull = convexHullDesc.SimpleWrap
                                    };

                                    lock (this)
                                    {
                                        convexHullMesh = new ConvexHullMesh();
                                    }

                                    convexHullMesh.Generate(decompositionDesc);

                                    var count = convexHullMesh.Count;

                                    commandContext.Logger.Info("Node generated " + count + " convex hulls");

                                    var vertexCountHull = 0;

                                    for (uint h = 0; h < count; h++)
                                    {
                                        float[] points;
                                        convexHullMesh.CopyPoints(h, out points);

                                        var pointList = new List<Vector3>();

                                        for (var v = 0; v < points.Length; v += 3)
                                        {
                                            var vert = new Vector3(points[v + 0], points[v + 1], points[v + 2]);
                                            pointList.Add(vert);

                                            vertexCountHull++;
                                        }

                                        hullsList.Add(pointList);

                                        uint[] indices;
                                        convexHullMesh.CopyIndices(h, out indices);

                                        //for (var t = 0; t < indices.Length; t += 3)
                                        //{
                                        //    Utilities.Swap(ref indices[t], ref indices[t + 2]);
                                        //}

                                        var indexList = new List<uint>(indices);

                                        indicesList.Add(indexList);
                                    }

                                    lock (this)
                                    {
                                        convexHullMesh.Dispose();
                                        convexHullMesh = null;
                                    }

                                    commandContext.Logger.Info("For a total of " + vertexCountHull + " vertexes");  
                                }
                            }
                        }

                        //clear up the reference as we don't need this data anymore
                        convexHullDesc.Model = null;
                    }
                }

                assetManager.Save(Url, asset.Data);
                
                return Task.FromResult(ResultStatus.Successful);
            }
        }
    }
}
