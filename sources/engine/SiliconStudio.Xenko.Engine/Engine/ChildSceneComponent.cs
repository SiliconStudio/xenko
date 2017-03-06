// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Specialized;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Engine.Processors;

namespace SiliconStudio.Xenko.Engine
{
    /// <summary>
    /// A link to a scene that is rendered by a parent <see cref="Scene"/>.
    /// </summary>
    [DataContract("ChildSceneComponent")]
    [Display("Child scene", Expand = ExpandRule.Once)]
    [DefaultEntityComponentProcessor(typeof(ChildSceneProcessor), ExecutionMode = ExecutionMode.Runtime)]
    [ComponentOrder(11200)]
    public sealed class ChildSceneComponent : ActivableEntityComponent
    {
        private readonly ChildSceneTransformOperation transformOperation;
        private Scene scene;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildSceneComponent"/> class.
        /// </summary>
        public ChildSceneComponent()
        {
            transformOperation = new ChildSceneTransformOperation(this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildSceneComponent"/> class.
        /// </summary>
        /// <param name="scene">The scene.</param>
        public ChildSceneComponent(Scene scene) : this()
        {
            Scene = scene;
        }

        /// <summary>
        /// Gets or sets the child scene.
        /// </summary>
        /// <value>The scene.</value>
        /// <userdoc>The reference to the scene to render. Any scene can be selected except the containing one.</userdoc>
        [DataMember(10)]
        public Scene Scene
        {
            get { return scene; }
            set
            {
                if (scene == value)
                    return;

                if (scene != null)
                {
                    scene.Parent = null;
                    scene.Entities.CollectionChanged -= Entities_CollectionChanged;

                    foreach (var entity in scene.Entities)
                        UnregisterEntity(entity);
                }

                scene = value;

                if (value != null)
                {
                    foreach (var entity in scene.Entities)
                        RegisterEntity(entity);

                    scene.Entities.CollectionChanged += Entities_CollectionChanged;
                    scene.Parent = Entity?.Scene;
                }
            }
        }

        public void UpdateScene()
        {
            if (scene != null)
            {
                scene.Parent = Entity?.Scene;
            }
        }

        private void Entities_CollectionChanged(object sender, TrackingCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    RegisterEntity((Entity)e.Item);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    UnregisterEntity((Entity)e.Item);
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }

        private void RegisterEntity(Entity entity)
        {
            entity.Transform.PostOperations.Add(transformOperation);
        }

        private void UnregisterEntity(Entity entity)
        {
            entity.Transform.PostOperations.Remove(transformOperation);
        }

        private class ChildSceneTransformOperation : TransformOperation
        {
            private readonly ChildSceneComponent childSceneComponent;

            public ChildSceneTransformOperation(ChildSceneComponent childSceneComponent)
            {
                this.childSceneComponent = childSceneComponent;
            }

            public override void Process(TransformComponent transformComponent)
            {
                Matrix.Multiply(ref transformComponent.WorldMatrix, ref childSceneComponent.Entity.Transform.WorldMatrix, out transformComponent.WorldMatrix);
            }
        }
    }
}
