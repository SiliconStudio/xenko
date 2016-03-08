// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Xenko.Rendering.Skyboxes
{
    /// <summary>
    /// The Skybox at runtime.
    /// </summary>
    [DataSerializerGlobal(typeof(ReferenceSerializer<Skybox>), Profile = "Content")]
    [ContentSerializer(typeof(DataContentSerializer<Skybox>))]
    [DataContract]
    public class Skybox
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Skybox"/> class.
        /// </summary>
        public Skybox()
        {
            Parameters = new ParameterCollection();
            DiffuseLightingParameters = new ParameterCollection();
            SpecularLightingParameters = new ParameterCollection();
        }

        /// <summary>
        /// Gets or sets the parameters compiled for the runtime for the skybox.
        /// </summary>
        /// <value>The parameters.</value>
        public ParameterCollection Parameters { get; set; }

        public ParameterCollection DiffuseLightingParameters { get; set; }

        public ParameterCollection SpecularLightingParameters { get; set; }
    }
}
