using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.Entities
{
    public class PrefabAssetCompiler : PrefabCompilerBase<PrefabAsset>
    {
        protected override PrefabCommandBase Create(string url, Package package, AssetCompilerContext context, PrefabAsset assetParameters)
        {
            return new PrefabCommand(url, package, context, assetParameters);
        }

        private class PrefabCommand : PrefabCommandBase
        {
            public PrefabCommand(string url, Package package, AssetCompilerContext context, PrefabAsset assetParameters) : base(url, package, context, assetParameters)
            {
            }

            protected override Prefab Create(PrefabAsset prefabAsset)
            {
                return new Prefab();
            }
        }
    }
}