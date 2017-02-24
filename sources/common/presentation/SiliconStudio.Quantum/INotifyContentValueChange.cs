using System;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Quantum
{
    public interface INotifyContentValueChange
    {
        /// <summary>
        /// Raised just before a change to this node occurs.
        /// </summary>
        event EventHandler<MemberNodeChangeEventArgs> Changing;

        /// <summary>
        /// Raised when a change to this node has occurred.
        /// </summary>
        event EventHandler<MemberNodeChangeEventArgs> Changed;

    }
}