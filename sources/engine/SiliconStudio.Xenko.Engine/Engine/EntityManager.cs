// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.ReferenceCounting;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Engine
{
    /// <summary>
    /// Manage a collection of entities.
    /// </summary>
    public abstract class EntityManager : ComponentBase, IReadOnlySet<Entity>
    {
        // TODO: Make this class threadsafe (current locks aren't sufficients)

        public ExecutionMode ExecutionMode { get; protected set; } = ExecutionMode.Runtime;

        // List of all entities, with their respective processors
        private readonly HashSet<Entity> entities;

        // Enabled entities
        private readonly TrackingHashSet<Entity> enabledEntities;

        private readonly FastCollection<EntityProcessor> processors;

        private readonly List<EntityProcessor> newProcessors;
        private readonly Dictionary<Type, ProcessorList> mapComponentTypeToProcessors;

        private readonly List<EntityProcessor> currentDependentProcessors;
        private readonly HashSet<Type> componentTypes;
        private readonly HashSet<Type> processorTypes;
        private int addEntityLevel = 0;

        /// <summary>
        /// Occurs when an entity is added.
        /// </summary>
        public event EventHandler<Entity> EntityAdded;

        /// <summary>
        /// Occurs when an entity is removed.
        /// </summary>
        public event EventHandler<Entity> EntityRemoved;

        /// <summary>
        /// Occurs when a new component type is added.
        /// </summary>
        public event EventHandler<Type> ComponentTypeAdded;

        /// <summary>
        /// Occurs when a component changed for an entity (Added or removed)
        /// </summary>
        public event EventHandler<EntityComponentEventArgs> ComponentChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityManager"/> class.
        /// </summary>
        /// <param name="registry">The registry.</param>
        /// <exception cref="System.ArgumentNullException">registry</exception>
        protected EntityManager(IServiceRegistry registry)
        {
            if (registry == null) throw new ArgumentNullException("registry");
            Services = registry;

            //entities = new TrackingDictionary<Entity, List<EntityProcessor>>();
            enabledEntities = new TrackingHashSet<Entity>();

            processors = new FastCollection<EntityProcessor>();
            newProcessors = new List<EntityProcessor>();

            componentTypes = new HashSet<Type>();
            processorTypes = new HashSet<Type>();

            mapComponentTypeToProcessors = new Dictionary<Type, ProcessorList>();

            entities = new HashSet<Entity>();

            currentDependentProcessors = new List<EntityProcessor>();
        }

        /// <summary>
        /// Gets the services.
        /// </summary>
        /// <value>The services.</value>
        public IServiceRegistry Services { get; private set; }

        /// <summary>
        /// Gets the entity Processors.
        /// </summary>
        public IReadOnlyCollection<EntityProcessor> Processors
        {
            get { return processors; }
        }

        /// <summary>
        /// Gets the list of component types from the entities..
        /// </summary>
        /// <value>The registered component types.</value>
        public IEnumerable<Type> ComponentTypes
        {
            get
            {
                return componentTypes;
            }
        }

        /// <summary>
        /// Adds a processor to this instance.
        /// </summary>
        /// <param name="processor">The processor.</param>
        /// <exception cref="System.ArgumentNullException">processor</exception>
        public void AddProcessor(EntityProcessor processor)
        {
            if (processor == null) throw new ArgumentNullException(nameof(processor));
            var processorType = processor.GetType();
            if (!processors.Contains(processor) && !processorTypes.Contains(processorType))
            {
                processors.Add(processor);
                processorTypes.Add(processorType);
                processors.Sort(EntityProcessorComparer.Default);
                OnProcessorAdded(processor);
            }
        }

        /// <summary>
        /// Removes a processor from this instance.
        /// </summary>
        /// <param name="processor">The processor.</param>
        /// <exception cref="System.ArgumentNullException">processor</exception>
        public void RemoveProcessor(EntityProcessor processor)
        {
            if (processor == null) throw new ArgumentNullException(nameof(processor));
            if (processors.Contains(processor))
            {
                processors.Remove(processor);
                OnProcessorRemoved(processor);
            }
        }

        public virtual void Update(GameTime gameTime)
        {
            foreach (var processor in processors)
            {
                if (processor.Enabled)
                {
                    using (Profiler.Begin(processor.UpdateProfilingKey, "Entities: {0}", enabledEntities.Count))
                    {
                        processor.Update(gameTime);
                    }
                }
            }
        }

        internal virtual void Draw(RenderContext context)
        {
            foreach (var processor in processors)
            {
                if (processor.Enabled)
                {
                    using (Profiler.Begin(processor.DrawProfilingKey))
                    {
                        processor.Draw(context);
                    }
                }
            }
        }

        /// <summary>
        /// Adds the entity.
        /// If the <see cref="Entity" /> has a parent, its parent should be added (or <see cref="TransformComponent.Children" />) should be used.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <exception cref="System.ArgumentException">Entity shouldn't have a parent.;entity</exception>
        internal void Add(Entity entity)
        {
            // Entity can't be a root because it already has a parent?
            if (entity.Transform != null && entity.Transform.Parent != null)
                throw new ArgumentException("Entity shouldn't have a parent.", nameof(entity));

            InternalAddEntity(entity);
        }

        /// <summary>
        /// Removes the entity from the <see cref="EntityManager" />.
        /// It works weither entity has a parent or not.
        /// In conjonction with <see cref="HierarchicalProcessor" />, it will remove children entities as well.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public void Remove(Entity entity)
        {
            InternalRemoveEntity(entity, true);
        }

        /// <summary>
        /// Removes all entities from the <see cref="EntityManager"/>.
        /// </summary>
        protected internal virtual void Reset()
        {
            // TODO: Not sure this method is correctly implemented
            // TODO: Check that we are correctly removing all indirect collection watchers (in processors...etc.)

            foreach (var entity in entities)
            {
                InternalRemoveEntity(entity, true);
            }
            var previousProcessors = processors.ToArray();
            foreach (var processor in previousProcessors)
            {
                RemoveProcessor(processor);
            }

            processorTypes.Clear();
            componentTypes.Clear();

            enabledEntities.Clear();
            entities.Clear();
        }

        /// <summary>
        /// Gets the processor.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>T.</returns>
        public T GetProcessor<T>() where T : EntityProcessor
        {
            // TODO: Cache in a Dictionary
            for (int i = 0; i < processors.Count; i++)
            {
                var system = processors[i] as T;
                if (system != null)
                    return system;
            }

            return null;
        }


        /// <summary>
        /// Adds the specified entity.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        internal void InternalAddEntity(Entity entity)
        {
            // Already added?
            if (entities.Contains(entity))
                return;

            if (entity.Manager != null)
            {
                throw new InvalidOperationException("Cannot add an entity to this entity manager when it is already used by another entity manager");
            }

            addEntityLevel++;

            entity.Manager = this;

            entities.Add(entity);
            enabledEntities.Add(entity);
            entity.AddReferenceInternal();

            // Check which processor want this entity
            CheckEntityWithProcessors(entity, false);

            // Grab the list of new processors to registers
            CollectNewProcessors(entity);

            addEntityLevel--;

            // Register all new processors
            RegisterNewProcessors();

            OnEntityAdded(entity);
        }

        private void RegisterNewProcessors()
        {
            // Auto-register all new processors
            if (addEntityLevel == 0)
            {
                // Add all new processors
                foreach (var newProcessor in newProcessors)
                {
                    processors.Add(newProcessor);
                }
                // Make sure they are always sorted
                if (newProcessors.Count > 0)
                {
                    processors.Sort(EntityProcessorComparer.Default);
                }

                // Notify
                foreach (var newProcessor in newProcessors)
                {
                    OnProcessorAdded(newProcessor);
                }
                newProcessors.Clear();
            }
        }

        /// <summary>
        /// Removes the specified entity.
        /// </summary>
        /// <param name="entity">The entity to remove.</param>
        /// <param name="removeParent">Indicate if entity should be removed from its parent</param>
        internal void InternalRemoveEntity(Entity entity, bool removeParent)
        {
            // Entity wasn't already added
            if (!entities.Contains(entity))
                return;

            entities.Remove(entity);
            enabledEntities.Remove(entity);

            if (removeParent)
            {
                // Force parent to be null, so that it is removed even if it is not a root node
                entity.Transform.Parent = null;
            }

            // Notify Processors this entity has been removed
            CheckEntityWithProcessors(entity, true);

            entity.ReleaseInternal();

            entity.Manager = null;

            OnEntityRemoved(entity);
        }

        private void CollectNewProcessors(Entity entity)
        {
            foreach (var component in entity.Components)
            {
                CollectNewProcessorsByComponentType(component.GetType());
            }
        }

        private void CollectNewProcessorsByComponentType(Type componentType)
        {
            if (componentType == null) throw new ArgumentNullException(nameof(componentType));

            if (componentTypes.Contains(componentType))
            {
                return;
            }

            componentTypes.Add(componentType);
            OnComponentTypeAdded(componentType);

            // Automatically collect processors that are used by this component
            var processorAttributes = componentType.GetTypeInfo().GetCustomAttributes<DefaultEntityComponentProcessorAttribute>();
            foreach (var processorAttributeType in processorAttributes)
            {
                var processorType = AssemblyRegistry.GetType(processorAttributeType.TypeName);
                if (processorType == null || !typeof(EntityProcessor).GetTypeInfo().IsAssignableFrom(processorType.GetTypeInfo()))
                {
                    // TODO: log an error
                    continue;
                }

                // Filter using ExecutionMode
                if ((ExecutionMode & processorAttributeType.ExecutionMode) != ExecutionMode.None)
                {
                    if (!processorTypes.Contains(processorType))
                    {
                        processorTypes.Add(processorType);
                        var processor = (EntityProcessor)Activator.CreateInstance(processorType);

                        foreach (var subComponentType in processor.RequiredTypes)
                        {
                            CollectNewProcessorsByComponentType(subComponentType);
                        }

                        newProcessors.Add(processor);
                    }
                }
            }
        }

        private void OnProcessorAdded(EntityProcessor processor)
        {
            processorTypes.Add(processor.GetType());

            processor.EntityManager = this;
            processor.Services = Services;
            processor.OnSystemAdd();

            // Update processor per types and dependencies
            foreach (var componentTypeAndProcessors in mapComponentTypeToProcessors)
            {
                var componentType = componentTypeAndProcessors.Key;
                var processorList = componentTypeAndProcessors.Value;

                if (processor.Accept(componentType))
                {
                    componentTypeAndProcessors.Value.Add(processor);
                }

                // Add dependent component
                if (processor.IsDependentOnComponentType(componentType))
                {
                    if (processorList.Dependencies == null)
                    {
                        processorList.Dependencies = new List<EntityProcessor>();
                    }
                    processorList.Dependencies.Add(processor);
                }
            }

            foreach (var entity in entities)
            {
                CheckEntityWithProcessors(entity, false);
            }
        }

        private void OnProcessorRemoved(EntityProcessor processor)
        {
            processor.OnSystemRemove();
            processor.Services = null;
            processor.EntityManager = null;
        }

        internal void NotifyComponentChanged(Entity entity, int index, EntityComponent oldValue, EntityComponent newValue)
        {
            // No real update   
            if (oldValue == newValue)
                return;

            // If we have a new component we can try to collect processors for it
            if (newValue != null)
            {
                CollectNewProcessorsByComponentType(newValue.GetType());
                RegisterNewProcessors();
            }

            // Remove previous component from processors
            currentDependentProcessors.Clear(); 
            if (oldValue != null)
            {
                CheckEntityWithProcessors(entity, oldValue, true, currentDependentProcessors);
            }

            // Add new component to processors
            if (newValue != null)
            {
                CheckEntityWithProcessors(entity, newValue, false, currentDependentProcessors);
            }

            // Update all dependencies
            if (currentDependentProcessors.Count > 0)
            {
                UpdateDependentProcessors(entity, oldValue, newValue, currentDependentProcessors);
            }

            // Notify component changes
            OnComponentChanged(entity, index, oldValue, newValue);
        }

        private void UpdateDependentProcessors(Entity entity, EntityComponent skipComponent1, EntityComponent skipComponent2, List<EntityProcessor> dependencies)
        {
            var components = entity.Components;
            for (int i = 0; i < components.Count; i++)
            {
                var component = components[i];
                if (component == skipComponent1 || component == skipComponent2)
                {
                    continue;
                }

                var componentType = component.GetType();
                var processorsForComponent = mapComponentTypeToProcessors[componentType];
                {
                    for (int j = 0; j < processorsForComponent.Count; j++)
                    {
                        var processor = processorsForComponent[j];
                        if (dependencies.Contains(processor))
                        {
                            processor.ProcessEntityComponent(entity, component, false);
                        }
                    }
                }
            }
        }

        private void CheckEntityWithProcessors(Entity entity, bool forceRemove)
        {
            var components = entity.Components;
            for (int i = 0; i < components.Count; i++)
            {
                CheckEntityWithProcessors(entity, components[i], forceRemove);
            }
        }

        private void CheckEntityWithProcessors(Entity entity, EntityComponent component, bool forceRemove, List<EntityProcessor> dependentProcessors = null)
        {
            var componentType = component.GetType();
            ProcessorList processorsForComponent;

            if (mapComponentTypeToProcessors.TryGetValue(componentType, out processorsForComponent))
            {
                for (int i = 0; i < processorsForComponent.Count; i++)
                {
                    processorsForComponent[i].ProcessEntityComponent(entity, component, forceRemove);
                }
            }
            else
            {
                processorsForComponent = new ProcessorList();
                for (int j = 0; j < processors.Count; j++)
                {
                    var processor = processors[j];
                    if (processor.Accept(componentType))
                    {
                        processorsForComponent.Add(processor);
                        processor.ProcessEntityComponent(entity, component, forceRemove);
                    }

                    if (processor.IsDependentOnComponentType(componentType))
                    {
                        if (processorsForComponent.Dependencies == null)
                        {
                            processorsForComponent.Dependencies = new List<EntityProcessor>();
                        }
                        processorsForComponent.Dependencies.Add(processor);
                    }
                }
                mapComponentTypeToProcessors.Add(componentType, processorsForComponent);
            }

            // Collect dependent processors
            var processorsForComponentDependencies = processorsForComponent.Dependencies;
            if (dependentProcessors != null && processorsForComponentDependencies != null)
            {
                for (int i = 0; i < processorsForComponentDependencies.Count; i++)
                {
                    var processor = processorsForComponentDependencies[i];
                    if (!dependentProcessors.Contains(processor))
                    {
                        dependentProcessors.Add(processor);
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether this instance contains the specified entity.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns><c>true</c> if this instance contains the specified entity; otherwise, <c>false</c>.</returns>
        public bool Contains(Entity item)
        {
            return entities.Contains(item);
        }

        public IEnumerator<Entity> GetEnumerator()
        {
            return entities.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count
        {
            get
            {
                return entities.Count;
            }
        }

        protected virtual void OnComponentTypeAdded(Type obj)
        {
            var handler = ComponentTypeAdded;
            if (handler != null) handler(this, obj);
        }

        protected virtual void OnEntityAdded(Entity e)
        {
            var handler = EntityAdded;
            if (handler != null) handler(this, e);
        }

        protected virtual void OnEntityRemoved(Entity e)
        {
            var handler = EntityRemoved;
            if (handler != null) handler(this, e);
        }

        protected virtual void OnComponentChanged(Entity entity, int index, EntityComponent previousComponent, EntityComponent newComponent)
        {
            var handler = ComponentChanged;
            if (handler != null) handler(this, new EntityComponentEventArgs(entity, index, previousComponent, newComponent));
        }

        private class EntityProcessorComparer : Comparer<EntityProcessor>
        {
            public new static readonly EntityProcessorComparer Default = new EntityProcessorComparer();

            public override int Compare(EntityProcessor x, EntityProcessor y)
            {
                return x.Order.CompareTo(y.Order);
            }
        }

        /// <summary>
        /// List of processors for a particular type
        /// </summary>
        private class ProcessorList : List<EntityProcessor>
        {
            public List<EntityProcessor> Dependencies;
        }
    }
}