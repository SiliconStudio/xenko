// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.EntityModel;

namespace SiliconStudio.Paradox.Effects.Lights
{
    /// <summary>
    /// Add a light to an <see cref="Entity"/>, that will be used during rendering.
    /// </summary>
    [DataContract("LightComponent")]
    [Display(120, "Light")]
    public sealed class LightComponent : EntityComponent
    {
        public static PropertyKey<LightComponent> Key = new PropertyKey<LightComponent>("Key", typeof(LightComponent));

        /// <summary>
        /// The default direction of a light vector is (x,y,z) = (0,0,-1)
        /// </summary>
        public static readonly Vector3 DefaultDirection = new Vector3(0, 0, -1);

        /// <summary>
        /// Initializes a new instance of the <see cref="LightComponent"/> class.
        /// </summary>
        public LightComponent()
        {
            Type = new LightDirectional();
            Intensity = 1.0f;
            Layers = RenderLayers.All;
        }

        /// <summary>
        /// Gets or sets the type of the light.
        /// </summary>
        /// <value>The type of the light.</value>
        [DataMember(10)]
        [NotNull]
        [Display("Light", AlwaysExpand = true)]
        public ILight Type { get; set; }

        /// <summary>
        /// Gets or sets the light intensity.
        /// </summary>
        /// <value>The light intensity.</value>
        [DataMember(30)]
        [DefaultValue(1.0f)]
        public float Intensity { get; set; }

        /// <summary>
        /// Get or sets the layers that the light influences
        /// </summary>
        /// <value>
        /// The layer mask.
        /// </value>
        [DataMember(40)]
        [DefaultValue(RenderLayers.All)]
        public RenderLayers Layers { get; set; }

        public override PropertyKey DefaultKey
        {
            get { return Key; }
        }
    }
}