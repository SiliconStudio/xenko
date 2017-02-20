namespace SiliconStudio.Quantum
{
    internal interface IInitializingObjectNode : IInitializingGraphNode, IObjectNode
    {
        /// <summary>
        /// Add a member to this node. This node and the member node must not have been sealed yet.
        /// </summary>
        /// <param name="member">The member to add to this node.</param>
        /// <param name="allowIfReference">if set to <c>false</c> throw an exception if <see cref="IGraphNode.TargetReference"/> or <see cref="IGraphNode.ItemReferences"/> is not null.</param>
        void AddMember(IMemberNode member, bool allowIfReference = false);
    }
}
