using System;
using System.Collections.Generic;
using SiliconStudio.Assets.Analysis;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Assets.Tests.Compilers
{
    public abstract class TestCompilerBase : IAssetCompiler
    {
        [ThreadStatic]
        public static HashSet<AssetItem> CompiledAssets;

        public abstract AssetCompilerResult Prepare(AssetCompilerContext context, AssetItem assetItem);

        public virtual IEnumerable<Type> GetRuntimeTypes(AssetItem assetItem) { yield break; }

        public virtual IEnumerable<ObjectUrl> GetInputFiles(AssetItem assetItem) { yield break; }

        public virtual IEnumerable<KeyValuePair<Type, BuildDependencyType>> GetInputTypes(AssetItem assetItem) { yield break; }

        public virtual IEnumerable<Type> GetInputTypesToExclude(AssetItem assetItem) { yield break; }

        public virtual bool AlwaysCheckRuntimeTypes { get; } = false;
    }
}