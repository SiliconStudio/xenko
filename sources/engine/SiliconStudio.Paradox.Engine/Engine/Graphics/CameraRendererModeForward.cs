// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Engine.Graphics
{
    /// <summary>
    /// A forward rendering mode.
    /// </summary>
    [DataContract("CameraRendererModeForward")]
    [Display("Forward")]
    public sealed class CameraRendererModeForward : CameraRendererMode
    {
        private const string ForwardEffect = "ParadoxForwardShadingEffect";

        /// <summary>
        /// Initializes a new instance of the <see cref="CameraRendererModeForward"/> class.
        /// </summary>
        public CameraRendererModeForward()
        {
            ModelEffect = ForwardEffect;
        }

        /// <summary>
        /// Gets or sets the effect to use to render the models in the scene.
        /// </summary>
        /// <value>The main model effect.</value>
        [DataMember(100)]
        [DefaultValue(ForwardEffect)]
        public string ModelEffect { get; set; }// TODO: This is not a good extensibility point. Check how to improve this
    }
}