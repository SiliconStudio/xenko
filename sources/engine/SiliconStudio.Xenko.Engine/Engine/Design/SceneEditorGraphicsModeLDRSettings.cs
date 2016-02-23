// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Rendering.Images;

namespace SiliconStudio.Xenko.Engine.Design
{
    /// <summary>
    /// Settings for a LDR rendering.
    /// </summary>
    [DataContract("SceneEditorGraphicsModeLDRSettings")]
    [Display("Low Dynamic Range")]
    public sealed class SceneEditorGraphicsModeLDRSettings : SceneEditorGraphicsModeSettingsBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SceneEditorGraphicsModeLDRSettings"/> class.
        /// </summary>
        public SceneEditorGraphicsModeLDRSettings()
        {
            BackgroundColor = (Color3)new Color(50, 50, 50);
        }

        public override bool RequiresHDRRenderFrame()
        {
            return false;
        }

        public override PostProcessingEffects GetEditorPostProcessingEffects()
        {
            return null; // By default, no processing effets.
        }
    }
}