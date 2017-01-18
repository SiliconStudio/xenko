using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Quantum.Contents
{
    /// <summary>
    /// Arguments of the <see cref="GraphNodeChangeListener"/> events.
    /// </summary>
    public class GraphMemberNodeChangeEventArgs : MemberNodeChangeEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphMemberNodeChangeEventArgs"/> class.
        /// </summary>
        /// <param name="e">A <see cref="MemberNodeChangeEventArgs"/> instance corresponding to this event.</param>
        public GraphMemberNodeChangeEventArgs([NotNull] MemberNodeChangeEventArgs e)
            : this(e.Member, e.Index, e.ChangeType, e.OldValue, e.NewValue)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphMemberNodeChangeEventArgs"/> class.
        /// </summary>
        /// <param name="member">The content that has changed.</param>
        /// <param name="index">The index in the content where the change occurred.</param>
        /// <param name="changeType">The type of change that occurred.</param>
        /// <param name="oldValue">The old value of the content.</param>
        /// <param name="newValue">The new value of the content.</param>
        public GraphMemberNodeChangeEventArgs([NotNull] IMemberNode member, Index index, ContentChangeType changeType, object oldValue, object newValue)
            : base(member, index, changeType, oldValue, newValue)
        {
        }
    }
}
