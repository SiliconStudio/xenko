// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Paradox.Engine.Graphics.Composers;
using SiliconStudio.Paradox.EntityModel;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// A component used internally to tag a Scene.
    /// </summary>
    public sealed class SceneComponent : EntityComponent
    {
        public static PropertyKey<SceneComponent> Key = new PropertyKey<SceneComponent>("Key", typeof(SceneComponent),
            new AccessorMetadata(OnSceneComponentGet, OnSceneComponentSet));

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneComponent"/> class.
        /// </summary>
        public SceneComponent()
        {
            GraphicsCompositor = new SceneGraphicsCompositorLayers();
        }

        /// <summary>
        /// Gets or sets the graphics composer for this scene.
        /// </summary>
        /// <value>The graphics composer.</value>
        [DataMember(10)]
        [Display("Graphics Composition")]
        [NotNull]
        public ISceneGraphicsCompositor GraphicsCompositor { get; set; }   // TODO: Should we move this to a special component?

        public override PropertyKey DefaultKey
        {
            get
            {
                return Key;
            }
        }

        private static readonly Type[] DefaultProcessors = new Type[] { typeof(SceneProcessor) };
        protected internal override IEnumerable<Type> GetDefaultProcessors()
        {
            return DefaultProcessors;
        }

        private static object OnSceneComponentGet(ref PropertyContainer props)
        {
            return ((Scene)props.Owner).Settings;
        }

        private static void OnSceneComponentSet(ref PropertyContainer props, object value)
        {
            var scene = props.Owner as Scene;
            if (scene == null)
            {
                throw new InvalidOperationException("A SceneComponent is only valid for the Scene object");
            }

            // TODO: Check if this is possible with serialization?
            if (scene.Settings != null)
            {
                throw new InvalidOperationException("A SceneComponent cannot be changed");
            }

            scene.Settings = (SceneComponent)value;
        }
    }
}