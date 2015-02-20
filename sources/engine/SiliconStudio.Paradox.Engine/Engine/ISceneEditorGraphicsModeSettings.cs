// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Paradox.Effects.Images;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// Rendering settings.
    /// </summary>
    public interface ISceneEditorGraphicsModeSettings
    {
        bool RequiresHDRRenderFrame();

        PostProcessingEffects GetSceneEditorPostProcessingEffects();
    }
}