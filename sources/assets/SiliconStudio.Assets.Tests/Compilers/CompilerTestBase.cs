using System.Collections.Generic;
using NUnit.Framework;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Assets.Tests.Compilers
{
    public class CompilerTestBase
    {
        [SetUp]
        public void Setup()
        {
            TestCompilerBase.CompiledAssets = new HashSet<AssetItem>();
        }

        [TearDown]
        public void TearDown()
        {
            TestCompilerBase.CompiledAssets = null;
        }

        protected static TContentType CreateRef<TContentType>(AssetItem assetItem) where TContentType : class, new()
        {
            return AttachedReferenceManager.CreateProxyObject<TContentType>(assetItem.Id, assetItem.Location);
        }
    }
}