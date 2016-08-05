using System;
using System.ComponentModel;
using SiliconStudio.Assets;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Assets.Rendering
{
    [DataContract]
    public class Link : IIdentifiable, IAssetPartDesign<Link>
    {
        public Link()
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

        public Block Source { get; set; }

        public string SourceSlot { get; set; }

        public Block Target { get; set; }

        public string TargetSlot { get; set; }

        /// <inheritdoc/>
        Link IAssetPartDesign<Link>.Part => this;
    }
}