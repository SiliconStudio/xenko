// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Paradox.EntityModel;

namespace SiliconStudio.Paradox.Effects.Skyboxes
{
    /// <summary>
    /// Add a <see cref="Skybox"/> to an <see cref="Entity"/>, that will be used during rendering.
    /// </summary>
    [DataContract("SkyboxComponent")]
    public sealed class SkyboxComponent : EntityComponent
    {
        public static PropertyKey<SkyboxComponent> Key = new PropertyKey<SkyboxComponent>("Key", typeof(SkyboxComponent));

        /// <summary>
        /// Initializes a new instance of the <see cref="SkyboxComponent"/> class.
        /// </summary>
        public SkyboxComponent() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SkyboxComponent"/> class.
        /// </summary>
        /// <param name="skybox">The skybox.</param>
        public SkyboxComponent(Skybox skybox)
        {
            // TODO: Move this to base component
            Enabled = true;
        }

        /// <summary>
        /// Gets or sets the skybox.
        /// </summary>
        /// <value>
        /// The skybox.
        /// </value>
        public Skybox Skybox { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether rendering is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if rendering is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool Enabled { get; set; }

        public override PropertyKey DefaultKey
        {
            get { return Key; }
        }
    }
}