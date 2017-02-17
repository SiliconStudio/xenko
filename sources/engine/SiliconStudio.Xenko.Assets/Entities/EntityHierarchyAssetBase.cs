// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Serializers;
using SiliconStudio.Core;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.IO;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.Entities
{
    /// <summary>
    /// Base class for entity assets (<see cref="SceneAsset"/> and <see cref="PrefabAsset"/>)
    /// </summary>
    [DataContract]
    //[AssetPartReference(typeof(Entity), typeof(EntityComponent), ExistsTopLevel = true)]
    //[AssetPartReference(typeof(EntityComponent), ReferenceType = typeof(EntityComponentReference))]
    public abstract partial class EntityHierarchyAssetBase : AssetCompositeHierarchy<EntityDesign, Entity>
    {
        /// <summary>
        /// Dumps this asset to a writer for debug purposes.
        /// </summary>
        /// <param name="writer">A text writer output</param>
        /// <param name="name">Name of this asset</param>
        /// <returns><c>true</c> if the dump was sucessful, <c>false</c> otherwise</returns>
        public bool DumpTo(TextWriter writer, string name)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));

            writer.WriteLine();
            writer.WriteLine("*************************************");
            writer.WriteLine($"{GetType().Name}: {name}");
            writer.WriteLine("=====================================");
            return Hierarchy?.DumpTo(writer) ?? false;
        }

        /// <inheritdoc/>
        public override Entity GetParent(Entity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            return entity.Transform.Parent?.Entity;
        }

        /// <inheritdoc/>
        public override int IndexOf(Entity part)
        {
            if (part == null) throw new ArgumentNullException(nameof(part));
            var parent = GetParent(part);
            return parent?.Transform.Children.IndexOf(part.Transform) ?? Hierarchy.RootPartIds.IndexOf(part.Id);
        }

        /// <inheritdoc/>
        public override Entity GetChild(Entity part, int index)
        {
            if (part == null) throw new ArgumentNullException(nameof(part));
            return part.Transform.Children[index].Entity;
        }

        /// <inheritdoc/>
        public override int GetChildCount(Entity part)
        {
            if (part == null) throw new ArgumentNullException(nameof(part));
            return part.Transform.Children.Count;
        }

        /// <inheritdoc/>
        public override IEnumerable<Entity> EnumerateChildParts(Entity entity, bool isRecursive)
        {
            if (entity.Transform == null)
                return Enumerable.Empty<Entity>();

            var enumerator = isRecursive ? entity.Transform.Children.DepthFirst(t => t.Children) : entity.Transform.Children;
            return enumerator.Select(t => t.Entity);
        }

        /// <inheritdoc/>
        protected override void PostClonePart(Entity part)
        {
            // disconnect the cloned entity
            part.Transform.Parent = null;
        }

        /// <inheritdoc/>
        protected override object ResolvePartReference(object partReference)
        {
            var entityComponentReference = partReference as EntityComponent;
            if (entityComponentReference != null)
            {
                var containingEntity = entityComponentReference.Entity;
                if (containingEntity == null)
                {
                    throw new InvalidOperationException("Found a reference to a component which doesn't have any entity");
                }

                var realEntity = (Entity)base.ResolvePartReference(containingEntity);
                if (realEntity == null)
                    return null;

                var componentId = entityComponentReference.Id;
                var realComponent = realEntity.Components.FirstOrDefault(c => c.Id == componentId);
                return realComponent;
            }

            return base.ResolvePartReference(partReference);
        }
    }
}
