// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Rendering.Images;

namespace SiliconStudio.Paradox.Engine.Design
{
    /// <summary>
    /// Base implementation of <see cref="ISceneEditorGraphicsModeSettings"/>
    /// </summary>
    public abstract class SceneEditorGraphicsModeSettingsBase : ISceneEditorGraphicsModeSettings
    {
        [DataMember(10)]
        public Color3 BackgroundColor { get; set; }

        public abstract bool RequiresHDRRenderFrame();

        public abstract PostProcessingEffects GetSceneEditorPostProcessingEffects();
    }
}