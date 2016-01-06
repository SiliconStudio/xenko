// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;

namespace SiliconStudio.Xenko.Engine
{
    [DataContract("EntityComponentCollection")]
    public class EntityComponentCollection : FastCollection<EntityComponent>
    {
        private readonly Entity entity;

        internal EntityComponentCollection(Entity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            this.entity = entity;
        }

        /// <summary>
        /// This property is only used when merging
        /// </summary>
        internal bool AllowReplaceForeignEntity { get; set; }

        protected override void ClearItems()
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                RemoveItem(i);
            }
            base.ClearItems();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get<T>() where T : EntityComponent
        {
            for (int i = 0; i < this.Count; i++)
            {
                var item = this[i] as T;
                if (item != null)
                {
                    return item;
                }
            }
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove<T>() where T : EntityComponent
        {
            for (int i = 0; i < this.Count; i++)
            {
                var item = this[i] as T;
                if (item != null)
                {
                    RemoveAt(i);
                    break;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAll<T>() where T : EntityComponent
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                var item = this[i] as T;
                if (item != null)
                {
                    RemoveAt(i);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetAll<T>() where T : EntityComponent
        {
            for (int i = 0; i < this.Count; i++)
            {
                var item = this[i] as T;
                if (item != null)
                {
                    yield return item;
                }
            }
        }

        protected override void InsertItem(int index, EntityComponent item)
        {
            ValidateItem(index, item);

            base.InsertItem(index, item);

            // Notify the entity about this component being updated
            entity.ComponentsUpdated(index, null, item);
        }

        protected override void RemoveItem(int index)
        {
            var item = this[index];
            if (index == 0)
            {
                if (Count > 1)
                {
                    throw new InvalidOperationException("Cannot remove TransformComponent when there are still other components attached");
                }
                entity.transform = null;
            }
            item.Entity = null;

            base.RemoveItem(index);

            // Notify the entity about this component being updated
            entity.ComponentsUpdated(index, item, null);
        }

        protected override void SetItem(int index, EntityComponent item)
        {
            ValidateItem(index, item);

            // Detach entity from previous item
            var oldItem = this[index];
            oldItem.Entity = null;

            base.SetItem(index, item);

            // Notify the entity about this component being updated
            entity.ComponentsUpdated(index, oldItem, item);
        }

        private void ValidateItem(int index, EntityComponent item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item), "Cannot add a null component");
            }

            var indexOf = IndexOf(item);
            if (indexOf >= 0)
            {
                if (index == indexOf)
                {
                    return;
                }
                throw new InvalidOperationException($"Cannot add a same component multiple times. Already set at index [{indexOf}]");
            }

            if (!AllowReplaceForeignEntity && item.Entity != null)
            {
                throw new InvalidOperationException($"This component is already attached to entity [{item.Entity}] and cannot be attached to [{entity}]");
            }

            var transform = item as TransformComponent;
            if (transform != null)
            {
                if (index != 0)
                {
                    throw new InvalidOperationException("Only one TransformComponent is allowed");
                }

                entity.transform = transform;
            }
            else if (index == 0)
            {
                throw new InvalidOperationException("Only TransformComponent can be added first");
            }

            item.Entity = entity;
        }
    }
}