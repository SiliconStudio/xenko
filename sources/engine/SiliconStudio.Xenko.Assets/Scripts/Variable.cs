using System;
using System.ComponentModel;
using SiliconStudio.Assets;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Assets.Scripts
{
    [DataContract]
    public class Variable : IIdentifiable, IAssetPartDesign<Variable>
    {
        public Variable()
        {
            Id = Guid.NewGuid();
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
        public virtual string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of that variable.
        /// </summary>
        [DataMember(-40)]
        public virtual string Type { get; set; }

        Variable IAssetPartDesign<Variable>.Part => this;
    }
}