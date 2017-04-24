// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Extensions;

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
                if (modelComponent != null)
                {
                    if (modelComponent.Model == null)
                    {
                        result.Warning($"The entity [{targetUrlInStorage}:{entityData.Entity.Name}] has a model component that does not reference any model.");
                    }
                    else
                    {
                        var modelAttachedReference = AttachedReferenceManager.GetAttachedReference(modelComponent.Model);
                        var modelId = modelAttachedReference.Id;

                        // compute the full path to the source asset.
                        var modelAssetItem = assetItem.Package.Session.FindAsset(modelId);
                        if (modelAssetItem == null)
                        {
                            result.Error($"The entity [{targetUrlInStorage}:{entityData.Entity.Name}] is referencing an unreachable model.");
                        }
                    }
                }

                var spriteComponent = entityData.Entity.Components.Get<SpriteComponent>();
                if (spriteComponent != null && spriteComponent.SpriteProvider.GetSprite() == null)
                {
                    result.Warning($"The entity [{targetUrlInStorage}:{entityData.Entity.Name}] has a sprite component that does not reference any sprite group.");
                }

                var nodeLinkComponent = entityData.Entity.Components.Get<ModelNodeLinkComponent>();
                if (nodeLinkComponent != null)
                {
                    nodeLinkComponent.ValidityCheck();
                    if (!nodeLinkComponent.IsValid)
                    {
                        result.Warning($"The Model Node Link between {entityData.Entity.Name} and {nodeLinkComponent.Target?.Entity.Name} is invalid.");
                        nodeLinkComponent.Target = null;
                    }
                }
            }

            result.BuildSteps = new AssetBuildStep(assetItem) { Create(targetUrlInStorage, asset, assetItem.Package) };
        }

        protected abstract AssetCommand<T> Create(string url, T assetParameters, Package package);
    }
}
