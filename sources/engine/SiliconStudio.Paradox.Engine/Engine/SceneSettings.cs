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
    [DataContract("SceneSettings")]
    [Display(10000, "Scene", Expand = ExpandRule.Once)]
    public sealed class SceneSettings : ComponentBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SceneSettings"/> class.
        /// </summary>
        public SceneSettings()
        {
            GraphicsCompositor = new SceneGraphicsCompositorLayers();
            EditorSettings = new SceneEditorSettings();
        }

        /// <summary>
        /// Gets or sets the graphics composer for this scene.
        /// </summary>
        /// <value>The graphics composer.</value>
        /// <userdoc>The compositor in charge of creating the graphic pipeline</userdoc>
        [DataMember(10)]
        [Display("Graphics Composition", Expand = ExpandRule.Always)]
        [NotNull]
        [Category]
        public ISceneGraphicsCompositor GraphicsCompositor { get; set; }   // TODO: Should we move this to a special component?

        /// <summary>
        /// Gets or sets the editor settings.
        /// </summary>
        /// <value>The editor settings.</value>
        /// <userdoc>Settings for the scene editor</userdoc>
        [DataMember(20)]
        [Display("Editor Settings", Expand = ExpandRule.Always)]
        [Category]
        public SceneEditorSettings EditorSettings { get; set; }
    }
}