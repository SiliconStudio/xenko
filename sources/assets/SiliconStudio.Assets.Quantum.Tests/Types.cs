using System;
using System.Collections.Generic;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Assets.Quantum.Tests
{
    public static class Types
    {
        public const string FileExtension = ".xktest";

        [DataContract]
        public abstract class MyAssetBase : Asset
        {
        }

        [DataContract]
        [AssetDescription(FileExtension)]
        public class MyAsset1 : MyAssetBase
        {
            public string MyString { get; set; }
        }

        [DataContract]
        [AssetDescription(FileExtension)]
        public class MyAsset2 : MyAssetBase
        {
            public List<string> MyStrings { get; set; } = new List<string>();
            public StructWithList Struct = new StructWithList { MyStrings = new List<string>() };
        }

        [DataContract]
        [AssetDescription(FileExtension)]
        public class MyAsset3 : MyAssetBase
        {
            public Dictionary<string, string> MyDictionary { get; set; } = new Dictionary<string, string>();
        }

        [DataContract]
        [AssetDescription(FileExtension)]
        public class MyAsset4 : MyAssetBase
        {
            public List<SomeObject> MyObjects { get; set; } = new List<SomeObject>();
        }

        [DataContract]
        [AssetDescription(FileExtension)]
        public class MyAsset5 : MyAssetBase
        {
            public List<IMyInterface> MyInterfaces { get; set; } = new List<IMyInterface>();
            public IMyInterface MyInterface { get; set; }
        }

        [DataContract]
        [AssetDescription(FileExtension)]
        public class MyAsset6 : MyAssetBase
        {
            public Dictionary<string, IMyInterface> MyDictionary { get; set; } = new Dictionary<string, IMyInterface>();
        }

        [DataContract]
        [AssetDescription(FileExtension)]
        public class MyAsset7 : MyAssetBase
        {
            public MyAsset2 MyAsset2 { get; set; }
            public MyAsset3 MyAsset3 { get; set; }
            public MyAsset4 MyAsset4 { get; set; }
        }

        [DataContract]
        [AssetDescription(FileExtension)]
        public class MyAsset8 : MyAssetBase
        {
            [NonIdentifiableCollectionItems]
            public List<SomeObject> MyObjects { get; set; } = new List<SomeObject>();
        }

        [DataContract]
        [AssetDescription(FileExtension)]
        public class MyAsset9 : MyAssetBase
        {
            public SomeObject MyObject { get; set; }
        }

        [DataContract]
        [AssetDescription(FileExtension)]
        public class MyAsset10 : MyAssetBase
        {
            [DefaultValue(true)]
            public bool MyBool { get; set; } = true;
        }

        [DataContract]
        public class MyReferenceable : IIdentifiable
        {
            public MyReferenceable() { Id = Guid.NewGuid(); }
            public string Value { get; set; }
            [NonOverridable]
            public Guid Id { get; set; }
            public override string ToString() => $"[{Id}] {Value}";
        }

        [DataContract]
        [AssetDescription(FileExtension)]
        public class MyAssetWithRef : MyAssetBase
        {
            public MyReferenceable MyObject1 { get; set; }

            public MyReferenceable MyObject2 { get; set; }

            [DefaultValue(null)]
            public MyReferenceable MyObject3 { get; set; }

            public List<MyReferenceable> MyObjects { get; set; } = new List<MyReferenceable>();

            [NonIdentifiableCollectionItems]
            public List<MyReferenceable> MyNonIdObjects { get; set; } = new List<MyReferenceable>();
        }

        [DataContract]
        [AssetDescription(FileExtension)]
        public class MyAssetWithRef2 : MyAssetBase
        {
            public MyReferenceable NonReference { get; set; }

            public MyReferenceable Reference { get; set; }

            public List<MyReferenceable> References { get; set; } = new List<MyReferenceable>();

            public static int MemberCount => 4 + 3; // 4 (number of members in Asset) + 3 (number of members in Types.MyAssetWithRef2)
        }


        [DataContract]
        public struct StructWithList
        {
            public List<string> MyStrings { get; set; }
        }

        public interface IMyInterface
        {
            string Value { get; set; }
        }

        [DataContract]
        public class SomeObject : IMyInterface
        {
            public string Value { get; set; }
        }

        [DataContract]
        public class SomeObject2 : IMyInterface
        {
            public string Value { get; set; }
            public int Number { get; set; }
        }

    }

    [AssetPropertyGraph(typeof(Types.MyAssetBase))]
    public class MyAssetBasePropertyGraph : AssetPropertyGraph
    {
        private readonly Dictionary<IGraphNode, IGraphNode> customBases = new Dictionary<IGraphNode, IGraphNode>();

        public MyAssetBasePropertyGraph(AssetPropertyGraphContainer container, AssetItem assetItem, ILogger logger)
            : base(container, assetItem, logger)
        {
        }

        public void RegisterCustomBaseLink(IGraphNode node, IGraphNode baseNode)
        {
            customBases.Add(node, baseNode);
        }

        public override IGraphNode FindTarget(IGraphNode sourceNode, IGraphNode target)
        {
            IGraphNode baseNode;
            return customBases.TryGetValue(sourceNode, out baseNode) ? baseNode : base.FindTarget(sourceNode, target);
        }
    }
}
