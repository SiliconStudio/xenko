using System;

namespace SiliconStudio.Core.Updater
{
    public abstract class UpdateMemberResolver
    {
        internal UpdateMemberResolver Next { get; set; }

        public abstract Type SupportedType { get; }

        public virtual UpdatableMember ResolveProperty(string propertyName)
        {
            return null;
        }

        public virtual UpdatableMember ResolveIndexer(string indexerName)
        {
            return null;
        }
    }
}