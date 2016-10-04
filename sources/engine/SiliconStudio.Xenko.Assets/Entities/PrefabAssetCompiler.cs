using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.Entities
{
    public class PrefabAssetCompiler : EntityHierarchyCompilerBase<PrefabAsset>
    {
        protected override EntityHierarchyCommandBase Create(string url, PrefabAsset assetParameters)
        {
            return new PrefabCommand(url, assetParameters);
        }

        private class PrefabCommand : EntityHierarchyCommandBase
        {
            public PrefabCommand(string url, PrefabAsset parameters) : base(url, parameters)
            {
            }

            protected override PrefabBase Create(PrefabAsset prefabAsset)
            {
                return new Prefab();
            }
        }
    }
}
