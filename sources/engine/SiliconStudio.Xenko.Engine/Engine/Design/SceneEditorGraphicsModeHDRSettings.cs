// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Xenko.Rendering.Images;

namespace SiliconStudio.Xenko.Engine.Design
{
    /// <summary>
    /// Settings for a HDR rendering
    /// </summary>
    [DataContract("SceneEditorGraphicsModeHDRSettings")]
    [Display("High Dynamic Range")]
    [ObjectFactory(typeof(SceneEditorGraphicsModeHDRSettings.Factory))]
    public sealed class SceneEditorGraphicsModeHDRSettings : SceneEditorGraphicsModeSettingsBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SceneEditorGraphicsModeHDRSettings"/> class.
        /// </summary>
        public SceneEditorGraphicsModeHDRSettings()
        {
            BackgroundColor = (Color3)new Color(120, 120, 120);
            PostProcessingEffects = new PostProcessingEffects();
        }

        /// <summary>
        /// Gets or sets the default post processing effects.
        /// </summary>
        /// <value>The post processing effects.</value>
        /// <userdoc>Default post processing effects applied to the scene in the editor</userdoc>
        [DataMember(20)]
        [NotNull]
        public PostProcessingEffects PostProcessingEffects { get; set; }

        private class Factory : IObjectFactory
        {
            public object New(Type type)
            {
                var settings = new SceneEditorGraphicsModeHDRSettings();

                // By default, only activate ToneMap and Gamma correction
                var fx = settings.PostProcessingEffects;
                fx.LightStreak.Enabled = false;
                fx.ColorTransforms.Transforms.Add(new ToneMap() { LuminanceLocalFactor = 0.0f });
                fx.ColorTransforms.Transforms.Add(new FilmGrain() { Enabled = false });
                fx.ColorTransforms.Transforms.Add(new Vignetting() { Enabled =  false } );
                fx.DepthOfField.Enabled = false;
                fx.Bloom.Enabled = false;
                fx.LensFlare.Enabled = false;

                return settings;
            }
        }

        public override bool RequiresHDRRenderFrame()
        {
            return true;
        }

        public override PostProcessingEffects GetEditorPostProcessingEffects()
        {
            return PostProcessingEffects;
        }
    }
}