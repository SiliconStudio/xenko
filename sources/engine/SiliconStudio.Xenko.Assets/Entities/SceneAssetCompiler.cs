// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Extensions;

namespace SiliconStudio.Xenko.Assets.Entities
{
    public class SceneAssetCompiler : EntityHierarchyCompilerBase<SceneAsset>
    {
        protected override EntityHierarchyCommandBase Create(string url, SceneAsset assetParameters)
        {
            return new SceneCommand(url, assetParameters);
        }

        private class SceneCommand : EntityHierarchyCommandBase
        {
            public SceneCommand(string url, SceneAsset parameters) : base(url, parameters)
            {               
            }

            protected override PrefabBase Create(SceneAsset prefabAsset)
            {
                return new Scene(prefabAsset.SceneSettings);
            }
        }

    }
}
