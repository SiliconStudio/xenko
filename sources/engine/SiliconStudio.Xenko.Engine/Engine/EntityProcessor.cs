// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Engine
{
    /// <summary>Entity processor, triggered on various <see cref="EntityManager"/> events such as Entity and Component additions and removals.</summary>
    public abstract class EntityProcessor
    {
        private bool enabled = true;

        internal ProfilingKey UpdateProfilingKey;
        internal ProfilingKey DrawProfilingKey;
        private readonly List<Type> requiredTypes;

        /// <summary>
        /// Tags associated to this entity processor
        /// </summary>
        public PropertyContainer Tags;

        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        public EntityManager EntityManager { get; internal set; }

        public IServiceRegistry Services { get; internal set; }

        protected EntityProcessor(Type mainComponentType, Type[] additionalTypes)
        {
            if (mainComponentType == null) throw new ArgumentNullException(nameof(mainComponentType));
            if (additionalTypes == null) throw new ArgumentNullException(nameof(additionalTypes));

            MainComponentType = mainComponentType;
            requiredTypes = new List<Type>() { MainComponentType };
            requiredTypes.AddRange(additionalTypes);

            // Check that types are valid
            foreach (var requiredType in requiredTypes)
            {
                if (!typeof(EntityComponent).IsAssignableFrom(requiredType))
                {
                    throw new ArgumentException($"Invalid required type [{requiredType}]. Expecting only an EntityComponent type");
                }
            }

            UpdateProfilingKey = new ProfilingKey(GameProfilingKeys.GameUpdate, this.GetType().Name);
            DrawProfilingKey = new ProfilingKey(GameProfilingKeys.GameDraw, this.GetType().Name);
        }

        /// <summary>
        /// Gets the primary component type handled by this processor
        /// </summary>
        public Type MainComponentType { get; }

        /// <summary>Gets the required components for an entity to be added to this entity processor.</summary>
        /// <value>The required keys.</value>
        public List<Type> RequiredTypes => requiredTypes;

        /// <summary>
        /// Gets or sets the order of this processor.
        /// </summary>
        /// <value>The order.</value>
        public int Order { get; protected set; }

        /// <summary>
        /// Performs work related to this processor.
        /// </summary>
        /// <param name="time"></param>
        public virtual void Update(GameTime time)
        {
        }

        /// <summary>
        /// Performs work related to this processor.
        /// </summary>
        /// <param name="context"></param>
        public virtual void Draw(RenderContext context)
        {
        }

        /// <summary>
        /// Run when this <see cref="EntityProcessor" /> is added to an <see cref="EntityManager" />.
        /// </summary>
        protected internal abstract void OnSystemAdd();

        /// <summary>
        /// Run when this <see cref="EntityProcessor" /> is removed from an <see cref="EntityManager" />.
        /// </summary>
        protected internal abstract void OnSystemRemove();

        /// <summary>
        /// Specifies weither an entity is enabled or not.
        /// </summary>
        /// <param name="entity">The entity.</param>
        protected internal abstract void SetEnabled(Entity entity, bool enabled);

        protected virtual void OnEnabledChanged(Entity entity, bool enabled)
        {
            
        }

        /// <summary>
        /// Checks if <see cref="Entity"/> needs to be either added or removed.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="forceRemove">if set to <c>true</c> [force remove].</param>
        protected internal abstract void EntityCheck(Entity entity, List<EntityProcessor> processors, bool forceRemove = false);

        /// <summary>
        /// Adds the entity to the internal list of the <see cref="EntityManager"/>.
        /// Exposed for inheriting class that has no access to SceneInstance as internal.
        /// </summary>
        /// <param name="entity">The entity.</param>
        protected internal void InternalAddEntity(Entity entity)
        {
            EntityManager.InternalAddEntity(entity);
        }

        /// <summary>
        /// Removes the entity to the internal list of the <see cref="EntityManager"/>.
        /// Exposed for inheriting class that has no access to SceneInstance as internal.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="removeParent">Indicate if entity should be removed from its parent</param>
        protected internal void InternalRemoveEntity(Entity entity, bool removeParent)
        {
            EntityManager.InternalRemoveEntity(entity, removeParent);
        }
    }

    /// <summary>Helper class for <see cref="EntityProcessor"/>, that will keep track of <see cref="Entity"/> matching certain <see cref="EntityComponent"/> requirements.</summary>
    /// Additional precomputed data will be stored alongside the <see cref="Entity"/> to offer faster accesses and iterations.
    /// <typeparam name="T">Generic type parameter.</typeparam>
    public abstract class EntityProcessor<TData, TComponent> : EntityProcessor  where TData : class, IEntityComponentNode where TComponent : EntityComponent
    {
        protected readonly Dictionary<Entity, TData> enabledEntities = new Dictionary<Entity, TData>();
        protected readonly Dictionary<Entity, TData> matchingEntities = new Dictionary<Entity, TData>();
        protected readonly HashSet<Entity> reentrancyCheck = new HashSet<Entity>();
        private readonly List<EntityComponent> tempComponents = new List<EntityComponent>();
        private readonly List<TData> tempDatas = new List<TData>();

        protected EntityProcessor(params Type[] requiredAdditionalTypes) : base(typeof(TComponent), requiredAdditionalTypes)
        {
        }

        /// <inheritdoc/>
        protected internal override void OnSystemAdd()
        {
        }

        /// <inheritdoc/>
        protected internal override void OnSystemRemove()
        {
        }

        /// <inheritdoc/>
        protected internal override void SetEnabled(Entity entity, bool enabled)
        {
            if (enabled)
            {
                TData entityData;
                if (!matchingEntities.TryGetValue(entity, out entityData))
                    throw new InvalidOperationException("EntityProcessor: Tried to enable an unknown entity.");

                enabledEntities.Add(entity, matchingEntities[entity]);
            }
            else
            {
                if (!enabledEntities.Remove(entity))
                    throw new InvalidOperationException("Invalid Entity Enabled state");
            }

            OnEnabledChanged(entity, enabled);
        }

        /// <inheritdoc/>
        protected internal override void EntityCheck(Entity entity, List<EntityProcessor> processors, bool forceRemove)
        {
            // If forceRemove is true, no need to check if entity matches.
            bool entityMatch = !forceRemove && EntityMatch(entity);
            TData entityData;
            bool entityAdded = matchingEntities.TryGetValue(entity, out entityData);

            if (entityMatch && !entityAdded)
            {
                // Adding entity is not reentrant, so let's skip if already being called for current entity
                // (could happen if either GenerateAssociatedData, OnEntityPrepare or OnEntityAdd changes
                // any Entity components
                lock (reentrancyCheck)
                {
                    if (!reentrancyCheck.Add(entity))
                        return;
                }

                TData previousData = null;
                var components = entity.Components;
                for (int i = 0; i < components.Count; i++)
                {
                    var component = components[i] as TComponent;
                    if (component == null)
                    {
                        continue;
                    }

                    // Need to add entity
                    var data = GenerateAssociatedData(entity, component);

                    if (entityData == null)
                    {
                        entityData = data;
                    }

                    if (previousData != null)
                    {
                        previousData.Next = data;
                    }
                    OnEntityAdding(entity, data);

                    previousData = data;
                }
                // Clear the last next entry
                if (previousData != null)
                {
                    previousData.Next = null;
                }

                processors.Add(this);
                matchingEntities.Add(entity, entityData);

                // If entity was enabled, add it to enabled entity list
                if (EntityManager.IsEnabled(entity))
                    enabledEntities.Add(entity, entityData);

                lock (reentrancyCheck)
                {
                    reentrancyCheck.Remove(entity);
                }
            }
            else if (entityAdded && !entityMatch)
            {
                // Need to be removed
                var current = entityData;
                while (current != null)
                {
                    OnEntityRemoved(entity, current);
                    current = (TData)current.Next;
                }
                processors.SwapRemove(this);

                // Remove from enabled and matching entities
                enabledEntities.Remove(entity);
                matchingEntities.Remove(entity);
            }
            else if (entityMatch) // && entityMatch
            {
                tempDatas.Clear();
                tempComponents.Clear();

                // Compute the list of associated data datas 
                var components = entity.Components;
                for (int i = 0; i < components.Count; i++)
                {
                    var component = components[i] as TComponent;
                    if (component == null)
                    {
                        continue;
                    }
                    tempComponents.Add(component);
                    tempDatas.Add(null);
                }

                // Iterate on the list of previous datas and match them with current components
                int minIndex = 0;
                var dataItem = entityData;
                while (dataItem != null)
                {
                    var index = tempComponents.IndexOf(dataItem.Component, minIndex);
                    if (index < 0)
                    {
                        OnEntityRemoved(entity, dataItem);
                    }
                    else
                    {
                        tempDatas[index] = dataItem;
                        if (minIndex == index)
                        {
                            minIndex++;
                        }
                    }
                    dataItem = (TData)dataItem.Next;
                }

                // Fill the gaps for new components, check if we need to update associated data if component was updated
                var count = tempDatas.Count;
                TData previousData = null;
                for (int i = 0; i < count; i++)
                {
                    var component = (TComponent)tempComponents[i];
                    var data = tempDatas[i];
                    if (data == null)
                    {
                        data = GenerateAssociatedData(entity, component);
                        OnEntityAdding(entity, data);
                    }
                    else
                    {
                        if (!IsAssociatedDataValid(entity, component, data))
                        {
                            OnEntityRemoved(entity, data);
                            data = GenerateAssociatedData(entity, component);
                            tempDatas[i] = data;
                            OnEntityAdding(entity, data);
                        }
                    }
                    if (previousData != null)
                    {
                        previousData.Next = data;
                    }
                    previousData = data;
                }
                // Clear the last next entry
                if (previousData != null)
                {
                    previousData.Next = null;
                }

                var firstData = tempDatas.FirstOrDefault();
                matchingEntities[entity] = firstData;
                if (EntityManager.IsEnabled(entity))
                    enabledEntities[entity] = firstData;

                // Don't keep any references
                tempDatas.Clear();
                tempComponents.Clear();
            }
        }

        /// <summary>Generates associated data to the given entity.</summary>
        /// Called right before <see cref="OnEntityAdding"/>.
        /// <param name="entity">The entity.</param>
        /// <param name="component"></param>
        /// <returns>The associated data.</returns>
        protected abstract TData GenerateAssociatedData(Entity entity, TComponent component);

        /// <summary>Checks if the current associated data is valid, or if readding the entity is required.</summary>
        /// <param name="entity">The entity.</param>
        /// <param name="component"></param>
        /// <param name="associatedData">The associated data.</param>
        /// <returns>True if the change in associated data requires the entity to be readded, false otherwise.</returns>
        protected virtual bool IsAssociatedDataValid(Entity entity, TComponent component, TData associatedData)
        {
            return GenerateAssociatedData(entity, component).Equals(associatedData);
        }

        protected virtual bool EntityMatch(Entity entity)
        {
            return RequiredTypes.All(x => entity.Components.Any(t => x.IsAssignableFrom(t.GetType())));
        }
       
        /// <summary>Run when a matching entity is added to this entity processor.</summary>
        /// <param name="entity">The entity.</param>
        /// <param name="data">  The associated data.</param>
        protected virtual void OnEntityAdding(Entity entity, TData data)
        {
        }

        /// <summary>Run when a matching entity is removed from this entity processor.</summary>
        /// <param name="entity">The entity.</param>
        /// <param name="data">  The associated data.</param>
        protected virtual void OnEntityRemoved(Entity entity, TData data)
        {
        }
    }
}