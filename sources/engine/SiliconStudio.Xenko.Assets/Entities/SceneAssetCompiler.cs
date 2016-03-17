// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.Entities
{
    public class SceneAssetCompiler : EntityHierarchyCompilerBase<SceneAsset>
    {
        protected override EntityHierarchyCommandBase Create(string url, Package package, AssetCompilerContext context, SceneAsset assetParameters)
        {
            return new SceneCommand(url, package, context, assetParameters);
        }

        private class SceneCommand : EntityHierarchyCommandBase
        {
            public SceneCommand(string url, Package package, AssetCompilerContext context, SceneAsset assetParameters) : base(url, package, context, assetParameters)
            {
            }

            protected override PrefabBase Create(SceneAsset prefabAsset)
            {
                return new Scene(prefabAsset.SceneSettings);
            }
        }

    }
}