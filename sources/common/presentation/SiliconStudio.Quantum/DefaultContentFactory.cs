using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.Contents;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Quantum
{
    /// <summary>
    /// This class is an implementation of the <see cref="IContentFactory"/> interface that can construct <see cref="ObjectContent"/>, <see cref="BoxedContent"/>
    /// and <see cref="MemberContent"/> instances.
    /// </summary>
    public class DefaultContentFactory : IContentFactory
    {
        private readonly INodeBuilder nodeBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultContentFactory"/> class.
        /// </summary>
        /// <param name="nodeBuilder">The node builder.</param>
        public DefaultContentFactory(INodeBuilder nodeBuilder)
        {
            this.nodeBuilder = nodeBuilder;
        }

        /// <inheritdoc/>
        public virtual IContent CreateObjectContent(object obj, ITypeDescriptor descriptor, bool isPrimitive)
        {
            var reference = nodeBuilder.CreateReferenceForNode(descriptor.Type, obj) as ReferenceEnumerable;
            return new ObjectContent(nodeBuilder, obj, descriptor, isPrimitive, reference);
        }

        /// <inheritdoc/>
        public virtual IContent CreateBoxedContent(object structure, ITypeDescriptor descriptor, bool isPrimitive)
        {
            return new BoxedContent(nodeBuilder, structure, descriptor, isPrimitive);
        }

        /// <inheritdoc/>
        public virtual IContent CreateMemberContent(IContent container, IMemberDescriptor member, bool isPrimitive, object value)
        {
            var reference = nodeBuilder.CreateReferenceForNode(member.Type, value);
            return new MemberContent(nodeBuilder, container, member, isPrimitive, reference);
        }
    }
}