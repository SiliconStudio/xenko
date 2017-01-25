// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.Entities
{
    public class SceneAssetCompiler : EntityHierarchyCompilerBase<SceneAsset>
    {
        protected override EntityHierarchyCommandBase Create(string url, SceneAsset assetParameters, Package package)
        {
            return new SceneCommand(url, assetParameters, package);
        }

        private class SceneCommand : EntityHierarchyCommandBase
        {
            public SceneCommand(string url, SceneAsset parameters, Package package) : base(url, parameters, package)
            {
            }

            protected override PrefabBase Create(SceneAsset prefabAsset)
            {
                return new Scene(prefabAsset.SceneSettings);
            }
        }

    }
}
