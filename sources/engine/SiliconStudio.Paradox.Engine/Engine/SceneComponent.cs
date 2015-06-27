// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Paradox.Engine.Design;
using SiliconStudio.Paradox.Engine.Processors;
using SiliconStudio.Paradox.Rendering.Composers;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// A component used internally to tag a Scene.
    /// </summary>
    [DataContract("SceneComponent")]
    [DefaultEntityComponentProcessor(typeof(SceneProcessor))]
    [Display(-100, "Scene")]
    public sealed class SceneComponent : EntityComponent
    {
        public readonly static PropertyKey<SceneComponent> Key = new PropertyKey<SceneComponent>("Key", typeof(SceneComponent),
            new AccessorMetadata(OnSceneComponentGet, OnSceneComponentSet));

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneComponent"/> class.
        /// </summary>
        public SceneComponent()
        {
            GraphicsCompositor = new SceneGraphicsCompositorLayers();
            EditorSettings = new SceneEditorSettings();
        }

        /// <summary>
        /// Gets or sets the graphics composer for this scene.
        /// </summary>
        /// <value>The graphics composer.</value>
        [DataMember(10)]
        [Display("Graphics Composition", AlwaysExpand = true)]
        [NotNull]
        [Category]
        public ISceneGraphicsCompositor GraphicsCompositor { get; set; }   // TODO: Should we move this to a special component?

        /// <summary>
        /// Gets or sets the editor settings.
        /// </summary>
        /// <value>The editor settings.</value>
        [DataMember(20)]
        [Display("Editor Settings", AlwaysExpand = true)]
        [Category]
        public SceneEditorSettings EditorSettings { get; set; }

        public override PropertyKey GetDefaultKey()
        {
            return Key;
        }

        private static object OnSceneComponentGet(ref PropertyContainer props)
        {
            var scene = props.Owner as Scene;
            if (scene != null)
            {
                return scene.Settings;
            }
            return null;
        }

        private static void OnSceneComponentSet(ref PropertyContainer props, object value)
        {
            var scene = props.Owner as Scene;
            if (scene == null)
            {
                throw new InvalidOperationException("A SceneComponent is only valid for the Scene object");
            }

            //// TODO: Check if this is possible with serialization? // Not working with Yaml
            //if (scene.Settings != null)
            //{
            //    throw new InvalidOperationException("A SceneComponent cannot be changed");
            //}

            scene.Settings = (SceneComponent)value;
        }
    }
}