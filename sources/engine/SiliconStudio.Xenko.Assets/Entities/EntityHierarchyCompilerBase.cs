using System.Threading.Tasks;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.Entities
{
    public abstract class EntityHierarchyCompilerBase<T> : AssetCompilerBase where T : EntityHierarchyAssetBase
    {
        protected override void Compile(AssetCompilerContext context, AssetItem assetItem, string targetUrlInStorage, AssetCompilerResult result)
        {
            var asset = (T)assetItem.Asset;
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
                        result.Warning($"The entity [{targetUrlInStorage}:{entityData.Entity.Name}] has a model component that does not reference any model.");
                        continue;
                    }

                    var modelAttachedReference = AttachedReferenceManager.GetAttachedReference(modelComponent.Model);
                    var modelId = modelAttachedReference.Id;

                    // compute the full path to the source asset.
                    var modelAssetItem = assetItem.Package.Session.FindAsset(modelId);
                    if (modelAssetItem == null)
                    {
                        result.Error($"The entity [{targetUrlInStorage}:{entityData.Entity.Name}] is referencing an unreachable model.");
                        continue;
                    }
                }
                if (spriteComponent != null && spriteComponent.SpriteProvider == null)
                {
                    result.Warning($"The entity [{targetUrlInStorage}:{entityData.Entity.Name}] has a sprite component that does not reference any sprite group.");
                }
            }

            result.BuildSteps = new AssetBuildStep(assetItem) { Create(targetUrlInStorage, asset, assetItem.Package) };
        }

        protected abstract EntityHierarchyCommandBase Create(string url, T assetParameters, Package package);

        protected abstract class EntityHierarchyCommandBase : AssetCommand<T>
        {
            protected EntityHierarchyCommandBase(string url, T parameters, Package package) : base(url, parameters, package)
            {
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                var assetManager = new ContentManager();

                var prefab = Create(Parameters);
                foreach (var rootEntity in Parameters.Hierarchy.RootPartIds)
                {
                    prefab.Entities.Add(Parameters.Hierarchy.Parts[rootEntity].Entity);
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
