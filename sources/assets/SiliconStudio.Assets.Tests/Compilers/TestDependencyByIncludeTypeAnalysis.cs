using NUnit.Framework;
using SiliconStudio.Assets.Analysis;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Assets.Tests.Compilers
{
    [TestFixture]
    public class TestDependencyByIncludeTypeAnalysis : CompilerTestBase
    {
        [Test]
        public void CompilerDependencyByIncludeTypeAnalysis()
        {
            var package = new Package();
            // ReSharper disable once UnusedVariable - we need a package session to compile
            var packageSession = new PackageSession(package);
            var asset1 = new AssetItem("content1", new MyAsset1(), package); // Should be compiled (root)
            var asset2 = new AssetItem("content2", new MyAsset2(), package); // Should be compiled (Runtime for Asset1)
            var asset3_1 = new AssetItem("content3_1", new MyAsset3(), package); // Should NOT be compiled (CompileAsset for Asset1)
            var asset3_2 = new AssetItem("content3_2", new MyAsset3(), package); // Should be compiled (Runtime for Asset2)

            ((MyAsset1)asset1.Asset).MyContent2 = AttachedReferenceManager.CreateProxyObject<MyContent2>(asset2.Id, asset2.Location);
            ((MyAsset1)asset1.Asset).MyContent3 = AttachedReferenceManager.CreateProxyObject<MyContent3>(asset3_1.Id, asset3_1.Location);
            ((MyAsset2)asset2.Asset).MyContent3 = AttachedReferenceManager.CreateProxyObject<MyContent3>(asset3_2.Id, asset3_2.Location);

            package.Assets.Add(asset1);
            package.Assets.Add(asset2);
            package.Assets.Add(asset3_1);
            package.Assets.Add(asset3_2);
            package.RootAssets.Add(new AssetReference(asset1.Id, asset1.Location));

            // Create context
            var context = new AssetCompilerContext();

            // Builds the project
            var assetBuilder = new PackageCompiler(new RootPackageAssetEnumerator(package));
            context.Properties.Set(BuildAssetNode.VisitRuntimeTypes, true);
            var assetBuildResult = assetBuilder.Prepare(context);
            // Total number of asset to compile = 3
            Assert.AreEqual(3, assetBuildResult.BuildSteps.Count);
        }

    }
}