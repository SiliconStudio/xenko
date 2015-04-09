// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Assets.Model
{
    [DataContract]
    [DataStyle(DataStyle.Compact)]
    public sealed class EntityScriptReference : IEntityScriptReference
    {
        public EntityScriptReference()
        {
        }

        public EntityScriptReference(Script script)
        {
            Entity = script.Entity;
            Id = script.Id;
        }

        [DataMember(10)]
        public EntityReference Entity { get; set; }

        [DataMember(20)]
        public Guid Id { get; set; }

        public static EntityScriptReference New(Script script)
        {
            return new EntityScriptReference(script);
        }
    }
}