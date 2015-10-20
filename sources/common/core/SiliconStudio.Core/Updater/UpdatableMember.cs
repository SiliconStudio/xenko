using System;

namespace SiliconStudio.Core.Updater
{
    /// <summary>
    /// Describes how to access an object member so that it can be updated by the <see cref="UpdateEngine"/>.
    /// </summary>
    public abstract class UpdatableMember
    {
        public abstract Type MemberType { get; }

        /// <summary>
        /// For arrays, gets the element field.
        /// </summary>
        public virtual UpdatableMember CreateMemberElement()
        {
            throw new NotSupportedException();
        }
    }
}