// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Paradox.Effects.Images;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// Settings for a LDR rendering.
    /// </summary>
    [DataContract("SceneEditorGraphicsModeLDRSettings")]
    [Display("Low Dynamic Range")]
    public sealed class SceneEditorGraphicsModeLDRSettings : ISceneEditorGraphicsModeSettings
    {
        public bool RequiresHDRRenderFrame()
        {
            return false;
        }

        public PostProcessingEffects GetSceneEditorPostProcessingEffects()
        {
            return null; // By default, no processing effets.
        }
    }
}