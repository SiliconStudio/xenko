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
        /// <inheritdoc/>
        public virtual IContent CreateObjectContent(object obj, ITypeDescriptor descriptor, bool isPrimitive, ReferenceEnumerable reference)
        {
            return new ObjectContent(obj, descriptor, isPrimitive, reference);
        }

        /// <inheritdoc/>
        public virtual IContent CreateBoxedContent(object structure, ITypeDescriptor descriptor, bool isPrimitive)
        {
            return new BoxedContent(structure, descriptor, isPrimitive);
        }

        /// <inheritdoc/>
        public virtual IContent CreateMemberContent(IContent container, IMemberDescriptor member, ITypeDescriptor descriptor, bool isPrimitive, IReference reference)
        {
            return new MemberContent(container, member, descriptor, isPrimitive, reference);
        }
    }
}