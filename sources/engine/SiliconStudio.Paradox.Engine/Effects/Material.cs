// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Paradox.Effects
{
    // Serializers needed by Material system
    [DataSerializerGlobal(typeof(ReferenceSerializer<Material>), Profile = "Asset")]
    [ContentSerializer(typeof(DataContentSerializer<Material>))]
    [DataContract]
    public class Material
    {
        public Material()
        {
            Parameters = new ParameterCollection();
        }

        public ParameterCollection Parameters { get; set; }

        /// <summary>
        /// The tessellation method used by the material.
        /// </summary>
        public ParadoxTessellationMethod TessellationMethod;
    }
}
