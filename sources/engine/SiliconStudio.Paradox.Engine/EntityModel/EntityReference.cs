using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Paradox.EntityModel
{
    /// <summary>
    /// Represents a reference to an <see cref="Entity"/>, using its Id (and Name for easier matching and readability).
    /// </summary>
    [DataContract]
    [DataStyle(DataStyle.Compact)]
    [DataSerializer(typeof(EntityReference.Serializer))]
    public sealed class EntityReference
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

        public static readonly PropertyKey<EntityAnalysisResult> EntityAnalysisResultKey = new PropertyKey<EntityAnalysisResult>("EntityAnalysisResult", typeof(EntityReference));

        internal class Serializer : DataSerializer<EntityReference>
        {
            public override void Serialize(ref EntityReference obj, ArchiveMode mode, SerializationStream stream)
            {
                if (obj == null)
                    obj = new EntityReference();

                if (mode == ArchiveMode.Deserialize)
                {
                    var entityReferenceContext = stream.Context.Get(EntityAnalysisResultKey);
                    if (entityReferenceContext != null)
                    {
                        entityReferenceContext.EntityReferences.Add(obj);
                    }
                }

                stream.Serialize(ref obj.id, mode);
            }
        }

        public class EntityAnalysisResult
        {
            public List<IEntityComponentReference> EntityComponentReferences = new List<IEntityComponentReference>();
            public List<EntityReference> EntityReferences = new List<EntityReference>();
        }
    }
}