// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Core.Storage;
using SiliconStudio.Paradox.Assets.Model;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.Data;
using SiliconStudio.Paradox.Engine.Data;
using SiliconStudio.Paradox.EntityModel.Data;
using SiliconStudio.Paradox.Shaders.Compiler;

namespace SiliconStudio.Paradox.Assets.Effect.Generators
{
    class EntityCompilerParametersGenerator : CompilerParameterGeneratorBase
    {
        private static ParameterKey[] shadowKeys =
        {
            LightingKeys.CastShadows,
            LightingKeys.ReceiveShadows
        };

        public struct EntityParameters
        {
            public ParameterCollectionData MaterialParameters;
            public ParameterCollectionData ModelParameters;
            public ParameterCollectionData MeshParameters;
            public ParameterCollectionData LightingParameters;
        };
        
        public override int GeneratorPriority
        {
            get
            {
                return 20;
            }
        }

        public static readonly PropertyKey<ConcurrentDictionary<Guid, List<EntityParameters>>> EntityParametersKey = new PropertyKey<ConcurrentDictionary<Guid, List<EntityParameters>>>("EntityParametersKey", typeof(EntityCompilerParametersGenerator));

        public override IEnumerable<CompilerParameters> Generate(AssetCompilerContext context, CompilerParameters baseParameters, ILogger log)
        {
            // Cache all the entity parameters once
            List<EntityParameters> entityParametersList;
            var entityParametersSet = (ConcurrentDictionary<Guid, List<EntityParameters>>)context.Properties.GetOrAdd(EntityParametersKey, key => new ConcurrentDictionary<Guid, List<EntityParameters>>());
            entityParametersList = entityParametersSet.GetOrAdd(context.Package.Id, key =>
            {
                var assetManager = new AssetManager();
                
                var settings = new AssetManagerLoaderSettings()
                {
                    ContentFilter = AssetManagerLoaderSettings.NewContentFilterByType(typeof(ModelData), typeof(MeshData), typeof(MaterialData), typeof(LightingConfigurationsSetData)),
                };

                var allEntityParameters = new List<EntityParameters>();
                foreach (var entityAssetItem in context.Package.Assets.Where(item => item.Asset is EntityAsset))
                {
                    var assetPath = entityAssetItem.Location.GetDirectoryAndFileName();
                    try
                    {
                        var entity = assetManager.Load<EntityData>(assetPath, settings);
                        
                        foreach (var modelComponent in entity.Components.Select(x => x.Value).OfType<ModelComponentData>())
                        {
                            foreach (var meshData in modelComponent.Model.Value.Meshes)
                            {
                                var lightingParameters = GetLightingParameters(meshData);
                                var materialParameters = GetMeshMaterialParameters(meshData);

                                if (lightingParameters == null || lightingParameters.Count == 0)
                                {
                                    EntityParameters entityParameters;
                                    entityParameters.MaterialParameters = materialParameters;
                                    entityParameters.ModelParameters = modelComponent.Parameters;
                                    entityParameters.MeshParameters = meshData != null ? meshData.Parameters : null;
                                    entityParameters.LightingParameters = null;
                                    allEntityParameters.Add(entityParameters);
                                }
                                else
                                {
                                    foreach (var lightConfig in lightingParameters)
                                    {
                                        EntityParameters entityParameters;
                                        entityParameters.MaterialParameters = materialParameters;
                                        entityParameters.ModelParameters = modelComponent.Parameters;
                                        entityParameters.MeshParameters = meshData != null ? meshData.Parameters : null;
                                        entityParameters.LightingParameters = lightConfig;
                                        allEntityParameters.Add(entityParameters);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("Error while loading model mesh [{0}]", ex, assetPath);
                    }
                }

                return allEntityParameters;
            });

            var useMeshParameters = baseParameters.Get(MeshKeys.UseParameters);
            var useMaterialParameters = baseParameters.Get(MaterialAssetKeys.UseParameters) && !baseParameters.Get(MaterialAssetKeys.GenerateShader);

            if ((useMeshParameters || useMaterialParameters) && entityParametersList.Count != 0)
            {
                var hashParameters = new HashSet<ObjectId>();

                foreach (var entityParameters in entityParametersList)
                {
                    // Add parameters in this order
                    // 1. Material
                    // 2. ModelComponent (Entity)
                    // 3. Mesh
                    // 4. Lighting

                    var newParameters = new ParameterCollection();
                    if (useMaterialParameters)
                        AddToParameters(entityParameters.MaterialParameters, newParameters);

                    AddToParameters(entityParameters.ModelParameters, newParameters);
                    
                    if (useMeshParameters)
                        AddToParameters(entityParameters.MeshParameters, newParameters);

                    AddToParameters(entityParameters.LightingParameters, newParameters);

                    byte[] buffer1;
                    var id = ObjectId.FromObject(newParameters, out buffer1);
                    if (!hashParameters.Contains(id))
                    {
                        hashParameters.Add(id);
                        var compilerParameters = baseParameters.Clone();
                        newParameters.CopyTo(compilerParameters);
                        yield return compilerParameters;
                    }
                }
            }
            else
            {
                yield return baseParameters.Clone();
            }
        }

        /// <summary>
        /// Get the parameters from the material.
        /// </summary>
        /// <param name="meshData">The mesh.</param>
        /// <returns>The material parameters.</returns>
        private ParameterCollectionData GetMeshMaterialParameters(MeshData meshData)
        {
            if (meshData != null && meshData.Material != null && meshData.Material.Value != null)
            {
                return meshData.Material.Value.Parameters;
            }
            return null;
        }

        /// <summary>
        /// Get the parameters from the lighting configurations.
        /// </summary>
        /// <param name="meshData">The mesh.</param>
        /// <returns>The lighting configurations.</returns>
        private List<ParameterCollectionData> GetLightingParameters(MeshData meshData)
        {
            if (meshData != null && meshData.Parameters != null && meshData.Parameters.ContainsKey(LightingKeys.LightingConfigurations))
            {
                var lightingDescContent = meshData.Parameters[LightingKeys.LightingConfigurations];
                if (lightingDescContent != null && lightingDescContent is ContentReference<LightingConfigurationsSetData>)
                {
                    var lightingDesc = ((ContentReference<LightingConfigurationsSetData>)lightingDescContent).Value;
                    if (lightingDesc != null)
                    {
                        var collection = new List<ParameterCollectionData>();
                        foreach (var config in lightingDesc.Configs)
                        {
                            var parameters = config.GetCollection();
                            SetShadowCasterReceiverConfiguration(meshData.Parameters, parameters, shadowKeys);
                            collection.Add(parameters);
                        }
                        return collection;
                    }
                }
                var defaultParameters = new ParameterCollectionData();
                SetShadowCasterReceiverConfiguration(meshData.Parameters, defaultParameters, shadowKeys);
                return new List<ParameterCollectionData> { defaultParameters };
            }
            return null;
        }

        private static void SetShadowCasterReceiverConfiguration(ParameterCollectionData sourceParameters, ParameterCollectionData targetParameters, params ParameterKey[] keys)
        {
            if (sourceParameters != null)
            {
                foreach (var key in keys)
                {
                    if (sourceParameters.ContainsKey(key))
                        targetParameters.Set(key, sourceParameters[key]);
                }
            }
        }

        [ModuleInitializer]
        internal static void Register()
        {
            // Register an instance of this generator to the effect asset compiler.
            EffectLibraryAssetCompiler.RegisterCompilerParametersGenerator(new EntityCompilerParametersGenerator());
        }
    }
}
