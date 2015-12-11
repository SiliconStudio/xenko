// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.Entities
{
    [DataContract]
    [DataStyle(DataStyle.Compact)]
    public class EntityScriptReference : IEntityScriptReference, IIdentifiable
    {
        public EntityScriptReference()
        {
        }

        public EntityScriptReference(Script script)
        {
            Id = script.Id;
            Entity = script.Entity;
            ScriptType = script.GetType();
        }

        [DataMember(10)]
        public EntityReference Entity { get; set; }

        [DataMember(20)]
        public Guid Id { get; set; }

        [DataMemberIgnore]
        public Type ScriptType { get; set; }
    }
}