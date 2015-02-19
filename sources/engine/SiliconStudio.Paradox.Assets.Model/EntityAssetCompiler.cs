// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Threading.Tasks;

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Paradox.Engine;

namespace SiliconStudio.Paradox.Assets.Model
{
    public class EntityAssetCompiler : AssetCompilerBase<EntityAsset>
    {
        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, EntityAsset asset, AssetCompilerResult result)
        {
            foreach (var entityData in asset.Hierarchy.Entities)
            {
                // TODO: How to make this code pluggable?

                // determine the underlying source asset exists
                if (entityData.Components.ContainsKey(ModelComponent.Key))
                {
                    var modelComponent = entityData.Components.Get(ModelComponent.Key);
                    if (modelComponent == null || modelComponent.Model == null)
                    {
                        result.Warning(string.Format("The entity [{0}:{1}] has a model component that does not reference any model.", urlInStorage, entityData.Name));
                        continue;
                    }
                    var modelAttachedReference = AttachedReferenceManager.GetAttachedReference(modelComponent.Model);
                    var modelId = modelAttachedReference.Id;

                    // compute the full path to the source asset.
                    var assetItem = AssetItem.Package.Session.FindAsset(modelId);
                    if (assetItem == null)
                    {
                        result.Error(string.Format("The entity [{0}:{1}] is referencing an unreachable model.", urlInStorage, entityData.Name));
                        continue;
                    }
                }
                if (entityData.Components.ContainsKey(SpriteComponent.Key))
                {
                    var spriteComponent = entityData.Components.Get(SpriteComponent.Key);
                    if (spriteComponent == null || spriteComponent.SpriteGroup == null)
                    {
                        result.Warning(string.Format("The entity [{0}:{1}] has a sprite component that does not reference any sprite group.", urlInStorage, entityData.Name));
                    }
                }
            }

            result.BuildSteps = new AssetBuildStep(AssetItem) { new EntityCombineCommand(urlInStorage, asset) };
        }

        private class EntityCombineCommand : AssetCommand<EntityAsset>
        {
            public EntityCombineCommand(string url, EntityAsset asset) : base(url, asset)
            {
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                var assetManager = new AssetManager();

                var rootEntity = asset.Hierarchy.Entities[asset.Hierarchy.RootEntity];
                assetManager.Save(Url, rootEntity);

                return Task.FromResult(ResultStatus.Successful);
            }

            public override string ToString()
            {
                return "Entity combine command for entity asset '{0}'.".ToFormat(Url);
            }
        }
    }
}