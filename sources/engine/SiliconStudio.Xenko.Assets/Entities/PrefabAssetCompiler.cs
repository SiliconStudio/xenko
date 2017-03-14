using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Analysis;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Assets.Textures;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Assets.Entities
{
    public class PrefabAssetCompiler : EntityHierarchyCompilerBase<PrefabAsset>
    {
        public override IEnumerable<KeyValuePair<Type, BuildDependencyType>> GetInputTypes(AssetCompilerContext context, AssetItem assetItem)
        {
            var compileContext = context.GetCompilationContext();
            if (compileContext == CompilationContext.Preview || compileContext == CompilationContext.Thumbnail)
            {
                foreach (var type in AssetRegistry.GetAssetTypes(typeof(Model)))
                {
                    yield return new KeyValuePair<Type, BuildDependencyType>(type, BuildDependencyType.Runtime | BuildDependencyType.CompileContent); //for models
                }
                yield return new KeyValuePair<Type, BuildDependencyType>(typeof(TextureAsset), BuildDependencyType.Runtime | BuildDependencyType.CompileContent); //for particle components!
            }
        }

        protected override AssetCommand<PrefabAsset> Create(string url, PrefabAsset assetParameters, Package package)
        {
            return new PrefabCommand(url, assetParameters, package);
        }

        private class PrefabCommand : AssetCommand<PrefabAsset>
        {
            public PrefabCommand(string url, PrefabAsset parameters, Package package) : base(url, parameters, package)
            {
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                var assetManager = new ContentManager();

                var prefab = new Prefab();
                foreach (var rootEntity in Parameters.Hierarchy.RootPartIds)
                {
                    prefab.Entities.Add(Parameters.Hierarchy.Parts[rootEntity].Entity);
                }
                assetManager.Save(Url, prefab);

                return Task.FromResult(ResultStatus.Successful);
            }

            public override string ToString()
            {
                return $"Prefab command for asset '{Url}'.";
            }
        }
    }
}
