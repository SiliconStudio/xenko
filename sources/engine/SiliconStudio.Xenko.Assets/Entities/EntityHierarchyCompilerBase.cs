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
        protected override void Prepare(AssetCompilerContext context, AssetItem assetItem, string targetUrlInStorage, AssetCompilerResult result)
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

        protected abstract AssetCommand<T> Create(string url, T assetParameters, Package package);
    }
}
