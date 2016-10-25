using System;
using System.ComponentModel;
using SiliconStudio.Assets;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Assets.Scripts
{
    [DataContract]
    public class Symbol : IIdentifiable, IAssetPartDesign<Symbol>
    {
        public Symbol()
        {
            Id = Guid.NewGuid();
        }

        public Symbol(string type, string name)
            : this()
        {
            Name = name;
            Type = type;
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

        /// <summary>
        /// Gets or sets the name of that variable.
        /// </summary>
        [DataMember(-50)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of that variable.
        /// </summary>
        [DataMember(-40)]
        public string Type { get; set; }

        Symbol IAssetPartDesign<Symbol>.Part => this;

        public override string ToString()
        {
            return $"{Type} {Name}";
        }
    }
}