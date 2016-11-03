using System;
using System.ComponentModel;
using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.Entities
{
    /// <summary>
    /// Associate an <see cref="Entity"/> with design-time data.
    /// </summary>
    [DataContract("EntityDesign")]
    public class EntityDesign : IAssetPartDesign<Entity>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="EntityDesign"/>.
        /// </summary>
        public EntityDesign()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="EntityDesign"/>.
        /// </summary>
        /// <param name="entity">The entity</param>
        public EntityDesign(Entity entity)
        {
            Entity = entity;
        }

        /// <summary>
        /// Gets or sets the folder where the entity is attached (folder is relative to parent folder). If null, the entity doesn't belong to a folder.
        /// </summary>
        [DataMember(10)]
        [DefaultValue(null)]
        public string Folder { get; set; }

        /// <summary>
        /// Gets or sets the entity
        /// </summary>
        [DataMember(10)]
        public Entity Entity { get; set; }

        /// <inheritdoc/>
        [DataMember(20)]
        [DefaultValue(null)]
        public BasePart Base { get; set; }

        /// <inheritdoc/>
        Entity IAssetPartDesign<Entity>.Part => Entity;

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"EntityDesign [{Entity.Name}]";
        }
    }
}