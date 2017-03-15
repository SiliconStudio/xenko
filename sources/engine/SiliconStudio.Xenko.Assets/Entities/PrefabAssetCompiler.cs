using System.Threading.Tasks;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.Entities
{
    public class PrefabAssetCompiler : EntityHierarchyCompilerBase<PrefabAsset>
    {
        protected override AssetCommand<PrefabAsset> Create(string url, PrefabAsset assetParameters)
        {
            return new PrefabCommand(url, assetParameters);
        }

        private class PrefabCommand : AssetCommand<PrefabAsset>
        {
            public PrefabCommand(string url, PrefabAsset parameters) : base(url, parameters)
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
