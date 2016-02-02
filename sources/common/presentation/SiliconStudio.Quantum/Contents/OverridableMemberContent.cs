using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Quantum.Contents
{
    /// <summary>
    /// An implementation of <see cref="MemberContent"/> that is aware of value override of the related member.
    /// </summary>
    public class OverridableMemberContent : MemberContent
    {
        public OverridableMemberContent(INodeBuilder nodeBuilder, IContent container, IMemberDescriptor member, bool isPrimitive, IReference reference)
            : base(nodeBuilder, container, member, isPrimitive, reference)
        {
            Override = container.Value.GetOverride(member);
        }

        public OverrideType Override { get { return Container.Value.GetOverride(Member); } set { Container.Value.SetOverride(Member, value); } }
    }
}