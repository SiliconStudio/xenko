using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using SiliconStudio.Assets.Analysis;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Assets.Tests
{
    [TestFixture]
    public class TestCompiler
    {
        [Test]
        public void TestCompilerVisitRuntimeType()
        {
            var package = new Package();
            // ReSharper disable once UnusedVariable - we need a package session to compile
            var packageSession = new PackageSession(package);
            var otherAssets = new List<AssetItem>
            {
                new AssetItem("contentRB", new MyAssetContentType(0), package),
                new AssetItem("contentRA", new MyAssetContentType(1), package),
                new AssetItem("content0B", new MyAssetContentType(2), package),
                new AssetItem("content0M", new MyAssetContentType(3), package),
                new AssetItem("content0A", new MyAssetContentType(4), package),
                new AssetItem("content1B", new MyAssetContentType(5), package),
                new AssetItem("content1M", new MyAssetContentType(6), package),
                new AssetItem("content1A", new MyAssetContentType(7), package),
                new AssetItem("content2B", new MyAssetContentType(8), package),
                new AssetItem("content2M", new MyAssetContentType(9), package),
                new AssetItem("content2A", new MyAssetContentType(10), package),
                new AssetItem("content3B", new MyAssetContentType(11), package),
                new AssetItem("content3M", new MyAssetContentType(12), package),
                new AssetItem("content3A", new MyAssetContentType(13), package),
                new AssetItem("content4B", new MyAssetContentType(14), package),
                new AssetItem("content4M", new MyAssetContentType(15), package),
                new AssetItem("content4A", new MyAssetContentType(16), package),
            };

            var assetToVisit = new MyAsset1();
            assetToVisit.Before = AttachedReferenceManager.CreateProxyObject<MyContentType>(otherAssets[0].Id, otherAssets[0].Location);
            assetToVisit.Zafter = AttachedReferenceManager.CreateProxyObject<MyContentType>(otherAssets[1].Id, otherAssets[1].Location);
            assetToVisit.RuntimeTypes.Add(CreateRuntimeType(otherAssets[2], otherAssets[3], otherAssets[4]));
            assetToVisit.RuntimeTypes.Add(CreateRuntimeType(otherAssets[5], otherAssets[6], otherAssets[7]));
            assetToVisit.RuntimeTypes.Add(CreateRuntimeType(otherAssets[8], otherAssets[9], otherAssets[10]));
            assetToVisit.RuntimeTypes.Add(CreateRuntimeType(otherAssets[11], otherAssets[12], otherAssets[13]));
            assetToVisit.RuntimeTypes.Add(CreateRuntimeType(otherAssets[14], otherAssets[15], otherAssets[16]));
            assetToVisit.RuntimeTypes[0].A = assetToVisit.RuntimeTypes[1];
            assetToVisit.RuntimeTypes[0].B = assetToVisit.RuntimeTypes[2];
            assetToVisit.RuntimeTypes[1].A = assetToVisit.RuntimeTypes[3];
            assetToVisit.RuntimeTypes[1].B = assetToVisit.RuntimeTypes[4];

            otherAssets.ForEach(x => package.Assets.Add(x));
            var assetItem = new AssetItem("asset", assetToVisit, package);
            package.Assets.Add(assetItem);
            package.RootAssets.Add(new AssetReference(assetItem.Id, assetItem.Location));

            // Create context
            var context = new AssetCompilerContext();

            // Builds the project
            var assetBuilder = new PackageCompiler(new RootPackageAssetEnumerator(package));
            context.Properties.Set(BuildAssetNode.VisitRuntimeTypes, true);
            var assetBuildResult = assetBuilder.Prepare(context);
            Assert.AreEqual(16, assetBuildResult.BuildSteps.Count);
        }

        private static MyRuntimeType CreateRuntimeType(AssetItem beforeReference, AssetItem middleReference, AssetItem afterReference)
        {
            var result = new MyRuntimeType
            {
                Before = AttachedReferenceManager.CreateProxyObject<MyContentType>(beforeReference.Id, beforeReference.Location),
                Middle = AttachedReferenceManager.CreateProxyObject<MyContentType>(middleReference.Id, middleReference.Location),
                Zafter = AttachedReferenceManager.CreateProxyObject<MyContentType>(afterReference.Id, afterReference.Location),
            };
            return result;
        }
    }

    [DataContract]
    [ReferenceSerializer, DataSerializerGlobal(typeof(ReferenceSerializer<MyContentType>), Profile = "Content")]
    public class MyContentType
    {
        public int Var;
    }

    [DataContract]
    public class MyRuntimeType
    {
        public MyContentType Before;
        public MyRuntimeType A;
        public MyContentType Middle;
        public MyRuntimeType B;
        public MyContentType Zafter;
    }

    [DataContract]
    [AssetDescription(FileExtension)]
    [AssetContentType(typeof(MyContentType))]
    public class MyAssetContentType : Asset
    {
        public const string FileExtension = ".xkmact";
        public int Var;
        public MyAssetContentType(int i) { Var = i; }
        public MyAssetContentType() { }
    }

    [DataContract]
    [AssetDescription(".xkmytest")]
    public class MyAsset1 : Asset
    {
        public MyContentType Before;
        public List<MyRuntimeType> RuntimeTypes = new List<MyRuntimeType>();
        public MyContentType Zafter;
    }

    public class MyTestCompilerCompiler<T> : IAssetCompiler where T : Asset
    {
        private class EmptyCommand : AssetCommand<T>
        {
            public EmptyCommand(string url, T parameters, Package package)
                : base(url, parameters, package)
            {
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                return Task.FromResult(ResultStatus.Successful);
            }
        }

        public AssetCompilerResult Prepare(AssetCompilerContext context, AssetItem assetItem)
        {
            return new AssetCompilerResult(GetType().Name)
            {
                BuildSteps = new AssetBuildStep(assetItem) { new EmptyCommand(assetItem.Location, (T)assetItem.Asset, assetItem.Package) }
            };
        }

        public IEnumerable<Type> GetRuntimeTypes(AssetCompilerContext context, AssetItem assetItem)
        {
            yield return typeof(MyRuntimeType);
        }

        public IEnumerable<ObjectUrl> GetInputFiles(AssetCompilerContext context, AssetItem assetItem) { yield break; }

        public IEnumerable<KeyValuePair<Type, BuildDependencyType>> GetInputTypes(AssetCompilerContext context, AssetItem assetItem) { yield break; }

        public IEnumerable<Type> GetInputTypesToExclude(AssetCompilerContext context, AssetItem assetItem) { yield break; }

        public bool AlwaysCheckRuntimeTypes { get; } = false;
    }

    [AssetCompiler(typeof(MyAsset1), typeof(AssetCompilationContext))]
    public class MyAsset1Compiler : MyTestCompilerCompiler<MyAsset1>
    {
    }

    [AssetCompiler(typeof(MyAssetContentType), typeof(AssetCompilationContext))]
    public class MyAssetContentTypeCompiler : MyTestCompilerCompiler<MyAssetContentType>
    {
    }
}