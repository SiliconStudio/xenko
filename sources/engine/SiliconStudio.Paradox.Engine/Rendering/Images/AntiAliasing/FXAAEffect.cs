// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Paradox.Rendering.Images
{
    /// <summary>
    /// A FXAA anti-aliasing pass.
    /// </summary>
    [DataContract("FXAAEffect")]
    public class FXAAEffect : ImageEffectShader, IScreenSpaceAntiAliasingEffect
    {
        private const int DefaultQuality = 15;
        internal static readonly ParameterKey<int> QualityKey = ParameterKeys.New(15);

        /// <summary>
        /// Initializes a new instance of the <see cref="FXAAEffect"/> class.
        /// </summary>
        public FXAAEffect() : this("FXAAShaderEffect")
        {
            Quality = DefaultQuality;
        }

        /// <summary>
        /// Animates the film grain.
        /// </summary>
        [DataMember(10)]
        [DefaultValue(DefaultQuality)]
        [DataMemberRange(10, 39, 1, 5, 0)]
        public int Quality { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FXAAEffect"/> class.
        /// </summary>
        /// <param name="antialiasShaderName">Name of the antialias shader.</param>
        /// <exception cref="System.ArgumentNullException">antialiasShaderName</exception>
        public FXAAEffect(string antialiasShaderName) : base(antialiasShaderName)
        {
            if (antialiasShaderName == null) throw new ArgumentNullException("antialiasShaderName");
        }

        protected override void PreDrawCore(RenderContext context)
        {
            base.PreDrawCore(context);
            Parameters.Set(QualityKey, Quality);
        }
    }
}