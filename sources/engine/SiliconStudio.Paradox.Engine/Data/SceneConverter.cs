// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Paradox.Engine.Data;
using SiliconStudio.Paradox.EntityModel.Data;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization.Assets;

namespace SiliconStudio.Paradox.Engine
{
    public static class SceneConverter
    {
        public static void ExportSceneData(EntityData entityData, string vfsOutputFilename, AssetManager assetManager, string effectName = null, string exportType = "entity", bool generateAEN = false)
        {
            object exportedObject;

            if (exportType == "animation")
            {
                throw new InvalidOperationException("Deprecated?");
                //var animationData = ((AnimationComponentData)entityData.Components[AnimationComponent.Key]).Animations[0].AnimationData;
                //exportedObject = animationData;
            }
            else if (exportType == "scenedata")
            {
                exportedObject = entityData;
            }
            else if (exportType == "entity")
            {
                var entityGroup = new EntityGroupData();

                // Additional processing
                foreach (var node in EnumerateChildren(entityData))
                {
                    entityGroup.Entities.Add(node);

                    var modelComponent = node.Components.Values.OfType<ModelComponentData>().FirstOrDefault();
                    if (modelComponent == null)
                        continue;

                    foreach (var effectMeshData in modelComponent.Model.Value.Meshes)
                    {
                        //if (effectMeshData.Value.DrawData != null)
                        //{
                        //    //// Generate AEN adjacency index buffer (if requested)
                        //    //if (generateAEN)
                        //    //{
                        //    //    var subMeshDataAEN = effectMeshData.DrawData.Value.SubMeshDatas[Mesh.StandardSubMeshData].Clone();
                        //    //    subMeshDataAEN.GenerateIndexBufferAEN();
                        //    //    effectMeshData.DrawData.Value.SubMeshDatas["TessellationAEN"] = subMeshDataAEN;
                        //    //}
                        //    //
                        //    //// Compact index buffer from 32 bits to 16 bits per index (if possible)
                        //    //foreach (var subMeshData in effectMeshData.DrawData.Value.SubMeshDatas)
                        //    //{
                        //    //    subMeshData.Value.CompactIndexBuffer();
                        //    //}

                        //    // Force effect name (if requested)
                        //    if (effectName != null)
                        //    {
                        //        effectMeshData.Value.EffectData.Value.Name = effectName;
                        //    }
                        //}
                    }
                }

                // Convert to Entity
                exportedObject = entityGroup;
            }
            else
            {
                throw new InvalidOperationException("Unknown export type.");
            }
            
            //contentManager = new ContentManager(new ContentSerializerContextGenerator(vfs, packageManager, ParameterContainerExtensions.DefaultSceneSerializer));
            assetManager.Save(VirtualFileSystem.Drive.RootPath + vfsOutputFilename + "#/root", exportedObject);
        }

        private static IEnumerable<EntityData> EnumerateChildren(EntityData nodeData)
        {
            // Enumerate self
            yield return nodeData;

            // Apply on children
            foreach (var child in ((TransformationComponentData)nodeData.Components[TransformationComponent.Key]).Children)
            {
                throw new NotImplementedException();
                //foreach (var subChild in EnumerateChildren(child.Entity))
                //{
                //    yield return subChild;
                //}
            }
        }
        
        /*private static void Convert(EntityData entity, NodeData nodeData, Context sceneContext)
        {
            entity.Name = nodeData.Name;
            sceneContext.NodeMapping[nodeData] = entity;

            var transformationComponent = new TransformationComponent();
            entity.Set(TransformationComponent.Key, transformationComponent);

            // Create entity
            foreach (var nodeProperty in nodeData.Properties)
            {
                if (nodeProperty is MeshData)
                {
                    var effectMeshData = (MeshData)nodeProperty;
                    var ModelComponent = entity.GetOrCreate(ModelComponent.Key);
                    //ModelComponent.ContentManager = contentSerializerContext.ContentManager;
                    //ModelComponent.SubMeshes.Add(effectMeshData);
                    throw new NotImplementedException();

                    sceneContext.EffectMeshMapping[effectMeshData] = ModelComponent;
                }
                else if (nodeProperty is CameraData)
                {
                    var cameraData = (CameraData)nodeProperty;

                    var cameraComponent = entity.GetOrCreate(CameraComponent.Key);
                    cameraComponent.NearPlane = cameraData.NearPlane;
                    cameraComponent.FarPlane = cameraData.FarPlane;
                    cameraComponent.AspectRatio = cameraData.AspectRatio;
                    cameraComponent.VerticalFieldOfView = cameraData.VerticalFieldOfView;
                }
                else if (nodeProperty is LightData)
                {
                    var lightData = (LightData)nodeProperty;
                    var transformationData = nodeData.Properties.OfType<TransformationTRSData>().FirstOrDefault();

                    var lightComponent = entity.GetOrCreate(LightComponent.Key);
                    lightComponent.Type = lightData.Type;
                    lightComponent.Color = lightData.Color;
                    lightComponent.Intensity = lightData.Intensity;
                    lightComponent.DecayStart = lightData.DecayStart;
                    if (transformationData != null)
                    {
                        lightComponent.LightDirection = lightData.LightDirection;
                    }
                    lightComponent.Deferred = lightData.Deferred;
                }
                else if (nodeProperty is TransformationMatrixData)
                {
                    var transformationData = (TransformationMatrixData)nodeProperty;
                    transformationComponent.Value = new TransformationMatrix { Matrix = transformationData.Transformation };
                }
                else if (nodeProperty is TransformationTRSData)
                {
                    var transformationData = (TransformationTRSData)nodeProperty;
                    transformationComponent.Value = new TransformationTRS
                        {
                            Translation = transformationData.Translation,
                            Rotation = transformationData.Rotation,
                            Scaling = transformationData.Scaling,
                        };
                }
            }

            // Recursively create children
            foreach (var childNodeData in nodeData.Children)
            {
                var childEntity = new Entity();
                Convert(childEntity, childNodeData, sceneContext);

                // Add node hierarchical relationship
                transformationComponent.Children.Add(childEntity.Transformation);
            }
        }

        public static EntityData Convert(SceneData sceneData)
        {
            // Convert to "Entity"
            var rootEntity = new EntityData();
            rootEntity.Components = new Dictionary<Core.PropertyKey, EntityComponentData>();
            var animationComponent = new AnimationComponentData();
            rootEntity.Components.Add(AnimationComponent.Key, animationComponent);

            var sceneContext = new Context();

            // Recursively walk through the graph and create Entity and nodes
            Convert(rootEntity, sceneData.RootNode, sceneContext);

            // Process animation
            var animation = sceneData.Animation;
            if (animation != null)
            {
                animationComponent.Animations.Add(new PlayingAnimation(AnimationData.FromAnimationChannels(animation.AnimationChannels)) { Weight = 0.0f });
            }

            // Process skinning
            foreach (var node in sceneContext.NodeMapping)
            {
                var skinningData = node.Key.Properties.OfType<SkinningData>().FirstOrDefault();
                if (skinningData != null)
                {
                    var entity = node.Value;
                    var skinningComponent = new SkinningComponent();
                    foreach (var clusterData in skinningData.Clusters)
                    {
                        var cluster = new SkinningCluster();
                        cluster.Link = sceneContext.NodeMapping[clusterData.Link].Transformation;
                        cluster.LinkToMeshMatrix = clusterData.MeshMatrix * Matrix.Invert(clusterData.LinkMatrix);

                        skinningComponent.Clusters.Add(cluster);
                    }
                    entity.Set(SkinningComponent.Key, skinningComponent);
                }

                var cameraData = node.Key.Properties.OfType<CameraData>().FirstOrDefault();
                if (cameraData != null)
                {
                    var entity = node.Value;
                    var cameraComponent = entity.Get(CameraComponent.Key);
                    if(cameraData.Target != null)
                        cameraComponent.Target = sceneContext.NodeMapping[cameraData.Target];
                }
            }

            return rootEntity;
        }*/
    }
}