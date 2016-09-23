using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.Entities
{
    public class PrefabAssetCompiler : EntityHierarchyCompilerBase<PrefabAsset>
    {
        protected override EntityHierarchyCommandBase Create(string url, Package package, AssetCompilerContext context, PrefabAsset assetParameters)
        {
            return new PrefabCommand(url, package, context, assetParameters);
        }

        private class PrefabCommand : EntityHierarchyCommandBase
        {
            public PrefabCommand(string url, Package package, AssetCompilerContext context, PrefabAsset parameters) : base(url, package, context, parameters)
            {
            }

            protected override PrefabBase Create(PrefabAsset prefabAsset)
            {
                return new Prefab();
            }
        }
    }
}