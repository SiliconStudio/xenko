// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Effects.Lights
{
    /// <summary>
    /// Base implementation of <see cref="IDirectLight"/>.
    /// </summary>
    public abstract class DirectLightBase : ColorLightBase, IDirectLight
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectLightBase"/> class.
        /// </summary>
        protected DirectLightBase()
        {
        }

        /// <summary>
        /// Gets or sets the shadow.
        /// </summary>
        /// <value>The shadow.</value>
        [DataMember(200)]
        [DefaultValue(null)]
        public ILightShadow Shadow { get; set; }
    }
}