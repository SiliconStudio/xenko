// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Games;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// Base processor for <see cref="SceneComponent"/> and <see cref="SceneChildComponent"/>.
    /// </summary>
    public abstract class SceneProcessorBase : EntityProcessor<SceneInstance>
    {
        private readonly Scene sceneEntityRoot;

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneProcessorBase"/> class.
        /// </summary>
        protected SceneProcessorBase(params PropertyKey[] requiredKeys) : base(requiredKeys)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneProcessorBase" /> class.
        /// </summary>
        /// <param name="sceneEntityRoot">The scene entity root.</param>
        /// <param name="requiredKeys">The required keys.</param>
        /// <exception cref="System.ArgumentNullException">sceneEntityRoot</exception>
        protected SceneProcessorBase(Scene sceneEntityRoot, params PropertyKey[] requiredKeys)
            : this(requiredKeys)
        {
            if (sceneEntityRoot == null) throw new ArgumentNullException("sceneEntityRoot");
            this.sceneEntityRoot = sceneEntityRoot;
            Scenes = new List<SceneInstance>();
        }

        public SceneInstance CurrentState { get; private set; }

        public List<SceneInstance> Scenes { get; private set; }

        protected override SceneInstance GenerateAssociatedData(Entity entity)
        {
            return entity == sceneEntityRoot ? CurrentState = new SceneInstance(EntityManager, entity, sceneEntityRoot) : new SceneInstance(EntityManager.Services, entity, sceneEntityRoot);
        }

        protected override void OnEntityAdding(Entity entity, SceneInstance data)
        {
            if (data != null)
            {
                Scenes.Add(data);
            }
        }

        protected override void OnEntityRemoved(Entity entity, SceneInstance data)
        {
            if (data != null)
            {
                data.Unload();
                Scenes.Remove(data);
            }
        }

        internal override bool ShouldStopProcessorChain(Entity entity)
        {
            // If the entity being added is not the scene entity root, don't run other processors, as this is handled 
            // by a nested EntityManager
            return !ReferenceEquals(entity, sceneEntityRoot);
        }

        public override void Update(GameTime time)
        {
            foreach (var sceneEntityAndState in Scenes)
            {
                sceneEntityAndState.EntityManager.Update(time);
            }
        }

        public override void Draw(GameTime time)
        {
            foreach (var sceneEntityAndState in Scenes)
            {
                sceneEntityAndState.EntityManager.Draw(time);
            }
        }
   }
}