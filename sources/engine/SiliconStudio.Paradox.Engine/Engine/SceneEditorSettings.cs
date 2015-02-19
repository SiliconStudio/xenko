// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Paradox.Effects.Images;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// Settings for the editor when viewing a scene in the scene editor.
    /// </summary>
    [DataContract("SceneEditorSettings")]
    public sealed class SceneEditorSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SceneEditorSettings"/> class.
        /// </summary>
        public SceneEditorSettings()
        {
            // TODO: We should have a mechanism to share settings for all scene or override them here
            PostProcessingEffects = new PostProcessingEffects
            {
                Enabled = false,
                Antialiasing = { Enabled = false },
                DepthOfField = { Enabled = false },
                Bloom = { Enabled = false },
            };
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use HDR when displaying a scene in the editor.
        /// </summary>
        /// <value><c>true</c> if [use HDR]; otherwise, <c>false</c>.</value>
        /// <userdoc>Use HDR rendering when displaying a scene in the editor</userdoc>
        [DataMember(10)]
        [DefaultValue(false)]
        [Display("Use HDR for editor?")]
        public bool UseHDR { get; set; }

        /// <summary>
        /// Gets or sets the default post processing effects.
        /// </summary>
        /// <value>The post processing effects.</value>
        /// <userdoc>Default post processing effects applied to the scene in the editor</userdoc>
        [DataMember(20)]
        [NotNull]
        public PostProcessingEffects PostProcessingEffects { get; set; }
    }
}