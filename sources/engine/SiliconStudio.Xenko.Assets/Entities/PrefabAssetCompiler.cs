using SiliconStudio.Assets;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.Entities
{
    public class PrefabAssetCompiler : EntityHierarchyCompilerBase<PrefabAsset>
    {
        protected override EntityHierarchyCommandBase Create(string url, PrefabAsset assetParameters, Package package)
        {
            return new PrefabCommand(url, assetParameters, package);
        }

        private class PrefabCommand : EntityHierarchyCommandBase
        {
            public PrefabCommand(string url, PrefabAsset parameters, Package package) : base(url, parameters, package)
            {
            }

            protected override PrefabBase Create(PrefabAsset prefabAsset)
            {
                return new Prefab();
            }
        }
    }
}
