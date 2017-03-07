using System;
using System.ComponentModel;
using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Yaml;

namespace SiliconStudio.Xenko.Assets.Scripts
{
    [DataContract]
    public class Method : IIdentifiable, IAssetPartDesign<Method>
    {
        public Method()
        {
            Id = Guid.NewGuid();
        }

        public Method(string name) : this()
        {
            Name = name;
        }

        [DataMember(-100), Display(Browsable = false)]
        public Guid Id { get; set; }

        /// <inheritdoc/>
        [DataMember(-90), Display(Browsable = false)]
        [DefaultValue(null)]
        public BasePart Base { get; set; }

        [DataMember(0)]
        [DefaultValue(Accessibility.Public)]
        public Accessibility Accessibility { get; set; } = Accessibility.Public;

        [DataMember(5)]
        [DefaultValue(VirtualModifier.None)]
        public VirtualModifier VirtualModifier { get; set; } = VirtualModifier.None;

        [DataMember(10)]
        [DefaultValue(false)]
        public bool IsStatic { get; set; }

        [DataMember(20)]
        public string Name { get; set; }

        [DataMember(30)]
        [DefaultValue("void")]
        public string ReturnType { get; set; } = "void";

        [DataMember(40)]
        public TrackingCollection<Parameter> Parameters { get; } = new TrackingCollection<Parameter>();

        [DataMember(50)]
        [NonIdentifiableCollectionItems]
        public AssetPartCollection<Block> Blocks { get; } = new AssetPartCollection<Block>();

        [DataMember(60)]
        [NonIdentifiableCollectionItems]
        public AssetPartCollection<Link> Links { get; } = new AssetPartCollection<Link>();

        Method IAssetPartDesign<Method>.Part { get { return this; } set { throw new InvalidOperationException(); } }
    }
}