// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.Entities
{
    /// <summary>
    /// Represents a reference to an <see cref="Entity"/>, using its Id (and Name for easier matching and readability).
    /// </summary>
    [DataContract]
    [DataStyle(DataStyle.Compact)]
    [DataSerializer(typeof(EntityReference.Serializer))]
    public sealed class EntityReference : IIdentifiable
    {
        private Guid id;

        public Guid Id
        {
            get { return id; }
            set { id = value; }
        }

        [DataMemberIgnore]
        public Entity Value { get; set; }

        public static explicit operator Entity(EntityReference contentReference)
        {
            if (contentReference == null)
                return null;
            return contentReference.Value;
        }

        public static implicit operator EntityReference(Entity value)
        {
            return new EntityReference { Value = value, Id = value.Id };
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", Id, Value);
        }

        internal class Serializer : DataSerializer<EntityReference>
        {
            public override void Serialize(ref EntityReference obj, ArchiveMode mode, SerializationStream stream)
            {
                if (obj == null)
                    obj = new EntityReference();

                stream.Serialize(ref obj.id, mode);
            }
        }
    }
}