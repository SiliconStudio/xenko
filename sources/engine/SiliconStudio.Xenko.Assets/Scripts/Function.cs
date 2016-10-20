using System;
using System.ComponentModel;
using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;

namespace SiliconStudio.Xenko.Assets.Scripts
{
    [DataContract]
    public class Function : IIdentifiable, IAssetPartDesign<Function>
    {
        public Function()
        {
            Id = Guid.NewGuid();
        }

        public Function(string name) : this()
        {
            Name = name;
        }

        [DataMember(-100), Display(Browsable = false)]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the base entity in case of prefabs. If null, the entity is not a prefab.
        /// </summary>
        [DataMember(-90), Display(Browsable = false)]
        [DefaultValue(null)]
        public Guid? BaseId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the part group in case of prefabs. If null, the entity doesn't belong to a part.
        /// </summary>
        [DataMember(-80), Display(Browsable = false)]
        [DefaultValue(null)]
        public Guid? BasePartInstanceId { get; set; }

        [DataMember(0)]
        [DefaultValue(Accessibility.Public)]
        public Accessibility Accessibility { get; set; } = Accessibility.Public;

        [DataMember(10)]
        [DefaultValue(false)]
        public bool IsStatic { get; set; }

        [DataMember(20)]
        public string Name { get; set; }

        [DataMember(30)]
        [DefaultValue("void")]
        public string ReturnType { get; set; } = "void";

        [DataMember(40)]
        public TrackingCollection<Variable> Parameters { get; } = new TrackingCollection<Variable>();

        [DataMember(50)]
        public AssetPartCollection<Block> Blocks { get; } = new AssetPartCollection<Block>();

        [DataMember(60)]
        public AssetPartCollection<Link> Links { get; } = new AssetPartCollection<Link>();

        Function IAssetPartDesign<Function>.Part => this;
    }
}