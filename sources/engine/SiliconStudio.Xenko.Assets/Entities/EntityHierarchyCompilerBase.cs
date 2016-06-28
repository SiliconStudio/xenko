using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.Entities
{
    public abstract class EntityHierarchyCompilerBase<T> : AssetCompilerBase<T> where T : EntityHierarchyAssetBase
    {
        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, T asset, AssetCompilerResult result)
        {
            foreach (var entityData in asset.Hierarchy.Parts)
            {
                // TODO: How to make this code pluggable?
                var modelComponent = entityData.Entity.Components.Get<ModelComponent>();
                var spriteComponent = entityData.Entity.Components.Get<SpriteComponent>();

                // determine the underlying source asset exists
                if (modelComponent != null)
                {
                    if (modelComponent.Model == null)
                    {
                        result.Warning($"The entity [{urlInStorage}:{entityData.Entity.Name}] has a model component that does not reference any model.");
                        continue;
                    }

                    var modelAttachedReference = AttachedReferenceManager.GetAttachedReference(modelComponent.Model);
                    var modelId = modelAttachedReference.Id;

                    // compute the full path to the source asset.
                    var assetItem = AssetItem.Package.Session.FindAsset(modelId);
                    if (assetItem == null)
                    {
                        result.Error($"The entity [{urlInStorage}:{entityData.Entity.Name}] is referencing an unreachable model.");
                        continue;
                    }
                }
                if (spriteComponent != null && spriteComponent.SpriteProvider == null)
                {
                    result.Warning($"The entity [{urlInStorage}:{entityData.Entity.Name}] has a sprite component that does not reference any sprite group.");
                }
            }

            result.BuildSteps = new AssetBuildStep(AssetItem) { Create(urlInStorage, AssetItem.Package, context, asset) };
        }

        protected abstract EntityHierarchyCommandBase Create(string url, Package package, AssetCompilerContext context, T assetParameters);

        protected abstract class EntityHierarchyCommandBase : AssetCommand<T>
        {
            private readonly Package package;
            private readonly AssetCompilerContext context;

            public EntityHierarchyCommandBase(string url, Package package, AssetCompilerContext context, T assetParameters) : base(url, assetParameters)
            {
                this.package = package;
                this.context = context;
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                var assetManager = new ContentManager();

                var prefab = Create(AssetParameters);
                foreach (var rootEntity in AssetParameters.Hierarchy.RootPartIds)
                {
                    prefab.Entities.Add(AssetParameters.Hierarchy.Parts[rootEntity].Entity);
                }
                assetManager.Save(Url, prefab);

                return Task.FromResult(ResultStatus.Successful);
            }

            protected abstract PrefabBase Create(T prefabAsset);

            public override string ToString()
            {
                return "Prefab command for entity asset '{0}'.".ToFormat(Url);
            }
        }
    }
}
