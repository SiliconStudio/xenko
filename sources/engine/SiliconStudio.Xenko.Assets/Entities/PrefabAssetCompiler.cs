using System.Threading.Tasks;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.Entities
{
    [CompatibleAsset(typeof(PrefabAsset), typeof(AssetCompilationContext))]
    public class PrefabAssetCompiler : EntityHierarchyCompilerBase<PrefabAsset>
    {
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
