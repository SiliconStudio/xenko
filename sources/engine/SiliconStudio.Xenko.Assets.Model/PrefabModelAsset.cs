using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Xenko.Assets.Entities;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Assets.Model
{
    /// <summary>
    /// The geometric primitive asset.
    /// </summary>
    [DataContract("PrefabModelAsset")]
    [AssetDescription(FileExtension)]
    [AssetCompiler(typeof(PrefabModelAssetCompiler))]
    [Display(185, "Prefab Model")]
    public sealed class PrefabModelAsset : Asset, IModelAsset
    {
        protected override int InternalBuildOrder => 600; //make sure we build after Models

        /// <summary>
        /// The default file extension used by the <see cref="ProceduralModelAsset"/>.
        /// </summary>
        public const string FileExtension = ".xkprefabmodel";

        [DataMember]
        public List<ModelMaterial> Materials { get; } = new List<ModelMaterial>();

        [DataMember]
        public Prefab Prefab { get; set; }
    }

    internal class PrefabModelAssetCompiler : AssetCompilerBase<PrefabModelAsset>
    {
        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, PrefabModelAsset asset, AssetCompilerResult result)
        {
            result.BuildSteps = new ListBuildStep { new PrefabModelAssetCompileCommand(urlInStorage, asset) };
            result.ShouldWaitForPreviousBuilds = true;
        }

        private class PrefabModelAssetCompileCommand : AssetCommand<PrefabModelAsset>
        {
            public PrefabModelAssetCompileCommand(string url, PrefabModelAsset assetParameters) 
                : base(url, assetParameters)
            {
            }

            private void ProcessMaterial(ICollection<Entity> entities, Material material, Rendering.Model prefabModel, bool castsShadows, bool receivesShadows)
            {
                var meshes = new List<Mesh>();

                //add material
                var matIndex = prefabModel.Materials.Count;
                prefabModel.Materials.Add(material);

                //TODO Create meshes

                //fix material index
                foreach (var mesh in meshes)
                {
                    mesh.MaterialIndex = matIndex;
                }

                prefabModel.Meshes.AddRange(meshes);
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                var contentManager = new ContentManager();

                var prefab = contentManager.Load<Prefab>(AttachedReferenceManager.GetUrl(AssetParameters.Prefab));
                if (prefab == null) throw new Exception("Failed to load prefab.");

                var prefabModel = new Rendering.Model();

                //The objective is to create 1 mesh per material/shadow params
                //1. We group by materials
                //2. Create a mesh per material (might need still more meshes if 16bit indexes or more then 32bit)

                var materialsNoShadows = new HashSet<Material>();
                var materialsCastRecvShadows = new HashSet<Material>();
                var materialsRecvShadows = new HashSet<Material>();
                var materialsCastShadows = new HashSet<Material>();

                foreach (var entity in prefab.Entities)
                {
                    //Hard coded for now to entities that have 2 components , transform + model and only Root node
                    var modelComponent = entity.Get<ModelComponent>();
                    if (entity.Components.Count == 2 && modelComponent != null && modelComponent.Skeleton.Nodes.Length == 1)
                    {
                        var modelAsset = contentManager.Load<Rendering.Model>(AttachedReferenceManager.GetUrl(modelComponent.Model));
                        if (modelAsset == null) continue;
                        foreach(var material in modelAsset.Materials)
                        {
                            if (material.IsShadowCaster && material.IsShadowReceiver)
                            {
                                materialsCastRecvShadows.Add(material.Material);
                            }
                            else if (!material.IsShadowCaster && !material.IsShadowReceiver)
                            {
                                materialsNoShadows.Add(material.Material);
                            }
                            else if (material.IsShadowCaster && !material.IsShadowReceiver)
                            {
                                materialsCastShadows.Add(material.Material);
                            }
                            else if (!material.IsShadowCaster && material.IsShadowReceiver)
                            {
                                materialsRecvShadows.Add(material.Material);
                            }
                        }
                    }
                    else
                    {
                        commandContext.Logger.Info($"Ignoring entity {entity.Name} since it is not compatible with PrefabModel.");
                    }
                }

                foreach (var material in materialsNoShadows)
                {
                    ProcessMaterial(prefab.Entities, material, prefabModel, false, false);
                }

                foreach (var material in materialsCastRecvShadows)
                {
                    ProcessMaterial(prefab.Entities, material, prefabModel, true, true);
                }

                foreach (var material in materialsRecvShadows)
                {
                    ProcessMaterial(prefab.Entities, material, prefabModel, false, true);
                }

                foreach (var material in materialsRecvShadows)
                {
                    ProcessMaterial(prefab.Entities, material, prefabModel, true, false);
                }

                return Task.FromResult(ResultStatus.Successful);
            }
        }
    }
}
