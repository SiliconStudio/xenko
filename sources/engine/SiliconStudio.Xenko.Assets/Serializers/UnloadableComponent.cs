// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SharpYaml.Events;
using SiliconStudio.Assets;
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
    [DataSerializerGlobal(typeof(UnloadableComponentSerializer), Profile = "AssetClone")]
    [Display("Error: unable to load this script")]
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

        /// <summary>
        /// Speciailize serializer only valid when cloning
        /// </summary>
        internal class UnloadableComponentSerializer : DataSerializer<UnloadableComponent>
        {
            public override void Serialize(ref UnloadableComponent obj, ArchiveMode mode, SerializationStream stream)
            {
                var invariantObjectList = stream.Context.Get(AssetCloner.InvariantObjectListProperty);
                if (mode == ArchiveMode.Serialize)
                {
                    stream.Write(invariantObjectList.Count);
                    invariantObjectList.Add(obj);
                }
                else
                {
                    var index = stream.Read<int>();

                    if (index >= invariantObjectList.Count)
                    {
                        throw new InvalidOperationException($"The type [{nameof(UnloadableComponent)}] cannot be only be used for clone serialization");
                    }

                    var invariant = invariantObjectList[index] as UnloadableComponent;
                    if (invariant == null)
                    {
                        throw new InvalidOperationException($"Unexpected null {nameof(UnloadableComponent)} while cloning");
                    }
                    // Create a new UnloadableComponent to avoid exception when adding the previous component to another entity(EntityComponentCollection is validating that a component cannot be added more than once to an entity)
                    obj = new UnloadableComponent(new List<ParsingEvent>(invariant.ParsingEvents), invariant.TypeName);
                }
            }
        }
    }
}