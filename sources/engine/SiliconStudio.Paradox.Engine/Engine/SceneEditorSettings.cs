// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// Settings for the editor when viewing a scene in the scene editor.
    /// </summary>
    [DataContract("SceneEditorSettings")]
    public sealed class SceneEditorSettings
    {
        // TODO: This class should contain more scene specific settings for the editor

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneEditorSettings"/> class.
        /// </summary>
        public SceneEditorSettings()
        {
            Mode = new SceneEditorGraphicsModeLDRSettings();
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use HDR when displaying a scene in the editor.
        /// </summary>
        /// <value><c>true</c> if [use HDR]; otherwise, <c>false</c>.</value>
        /// <userdoc>Use HDR rendering when displaying a scene in the editor</userdoc>
        [DataMember(10)]
        [NotNull]
        [Display("Rendering Mode")]
        public ISceneEditorGraphicsModeSettings Mode { get; set; }
    }
}