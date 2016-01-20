// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using SharpYaml.Events;

using SiliconStudio.Core;
using SiliconStudio.Assets.Serializers;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.Serializers
{
    /// <summary>
    /// Represents a Script that could not be loaded properly (usually due to missing/broken assemblies).
    /// Yaml representation is kept as is, so that it can be properly saved again.
    /// </summary>
    [DataSerializerGlobal(typeof(InvariantObjectCloneSerializer<UnloadableComponent>), Profile = "AssetClone")]
    [Display(1001, "Error: unable to load this script")]
    [AllowMultipleComponents]
    [NonInstantiable]
    public sealed class UnloadableComponent : EntityComponent
    {
        [DataMemberIgnore]
        public List<ParsingEvent> ParsingEvents { get; private set; }

        [DataMemberIgnore]
        public string TypeName { get; private set; }

        public UnloadableComponent()
        {
        }

        public UnloadableComponent(List<ParsingEvent> parsingEvents, string typeName)
        {
            ParsingEvents = parsingEvents;
            TypeName = typeName;
        }
    }
}