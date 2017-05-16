// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.Entities
{
    /// <summary>
    /// Base class for entity assets (<see cref="SceneAsset"/> and <see cref="PrefabAsset"/>)
    /// </summary>
    [DataContract]
    public abstract partial class EntityHierarchyAssetBase : AssetCompositeHierarchy<EntityDesign, Entity>
    {
        /// <summary>
        /// Dumps this asset to a writer for debug purposes.
        /// </summary>
        /// <param name="writer">A text writer output</param>
        /// <param name="name">Name of this asset</param>
        /// <returns><c>true</c> if the dump was sucessful, <c>false</c> otherwise</returns>
        public bool DumpTo([NotNull] TextWriter writer, string name)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));

            writer.WriteLine();
            writer.WriteLine("*************************************");
            writer.WriteLine($"{GetType().Name}: {name}");
            writer.WriteLine("=====================================");
            return Hierarchy.DumpTo(writer);
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
    }
}
