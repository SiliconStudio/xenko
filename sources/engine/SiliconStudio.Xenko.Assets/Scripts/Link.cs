using System;
using System.ComponentModel;
using SiliconStudio.Assets;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Assets.Scripts
{
    [DataContract]
    public class Link : IIdentifiable, IAssetPartDesign<Link>
    {
        public Link()
        {
            Id = Guid.NewGuid();
        }

        public Link(Slot source, Slot target)
             : this()
        {
            Source = source;
            Target = target;
        }

        public Link(ExecutionBlock source, Slot target)
             : this()
        {
            Source = source.OutputExecution;
            Target = target;
        }

        public Link(Slot source, ExecutionBlock target)
     : this()
        {
            Source = source;
            Target = target.InputExecution;
        }

        public Link(ExecutionBlock source, ExecutionBlock target)
             : this()
        {
            Source = source.OutputExecution;
            Target = target.InputExecution;
        }

        /// <summary>
        /// The function that contains this link.
        /// </summary>
        [DataMemberIgnore]
        public Method Method { get; internal set; }


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

        public Slot Source { get; set; }

        public Slot Target { get; set; }

        /// <inheritdoc/>
        Link IAssetPartDesign<Link>.Part => this;
    }
}