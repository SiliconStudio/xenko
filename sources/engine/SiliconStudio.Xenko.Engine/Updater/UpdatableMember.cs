using System;

namespace SiliconStudio.Xenko.Updater
{
    /// <summary>
    /// Describes how to access an object member so that it can be updated by the <see cref="UpdateEngine"/>.
    /// </summary>
    public abstract class UpdatableMember
    {
        public abstract Type MemberType { get; }
    }
}