// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Core.Serialization.Converters;
using SiliconStudio.Paradox.Engine;

namespace SiliconStudio.Paradox.EntityModel
{
    /// <summary>
    /// A group of <see cref="Entity"/> that can refers to each others.
    /// They can be loaded together as a chunk.
    /// </summary>
    [ContentSerializer(typeof(DataContentConverterSerializer<EntityGroup>))]
    [DataConverter(AutoGenerate = true, ContentReference = true)]
    public class EntityGroup
    {
        // TODO:FX We probably want to expose only Root entities and let hierarchy do the rest, so that it's easier to manipulate.
        [DataMemberConvert]
        public List<Entity> Entities { get; private set; }

        public EntityGroup()
        {
            Entities = new List<Entity>();
        }
    }
}