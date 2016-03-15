// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Rendering.Images;

namespace SiliconStudio.Xenko.Engine.Design
{
    /// <summary>
    /// Rendering settings.
    /// </summary>
    public interface ISceneEditorGraphicsModeSettings
    {
        /// <summary>
        /// Gets or sets the color of the background.
        /// </summary>
        /// <value>The color of the background.</value>
        /// <userdoc>The background color used by the editor view</userdoc>
        Color3 BackgroundColor { get; set; }

        bool RequiresHDRRenderFrame();

        PostProcessingEffects GetEditorPostProcessingEffects();
    }
}