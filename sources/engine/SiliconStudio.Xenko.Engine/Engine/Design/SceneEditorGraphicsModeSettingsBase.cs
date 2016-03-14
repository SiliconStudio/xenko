// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Rendering.Images;

namespace SiliconStudio.Xenko.Engine.Design
{
    /// <summary>
    /// Base implementation of <see cref="ISceneEditorGraphicsModeSettings"/>
    /// </summary>
    public abstract class SceneEditorGraphicsModeSettingsBase : ISceneEditorGraphicsModeSettings
    {
        /// <summary>
        /// Gets or sets the color of the background.
        /// </summary>
        /// <userdoc>The color used as the scene editor background.</userdoc>
        [DataMember(10)]
        public Color3 BackgroundColor { get; set; }

        public abstract bool RequiresHDRRenderFrame();

        public abstract PostProcessingEffects GetEditorPostProcessingEffects();
    }
}