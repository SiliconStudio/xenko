// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Paradox.Effects.Skyboxes
{
    // Serializers needed by Material system
    [DataSerializerGlobal(typeof(ReferenceSerializer<Skybox>), Profile = "Asset")]
    [ContentSerializer(typeof(DataContentSerializer<Skybox>))]
    [DataContract]
    public class Skybox
    {
        public Skybox()
        {
            Parameters = new ParameterCollection();
        }

        public ParameterCollection Parameters { get; set; }
    }
}
