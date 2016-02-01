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
        }

        public OverrideType GetContentOverride(object index)
        {
            bool getOnCollectionItem = false;
            if (index != null)
            {
                var value = Retrieve(index);
                // We can set override flags on item of collection if they are not value type and are identifiable
                if (value != null && !value.GetType().IsValueType)
                    getOnCollectionItem = true;
            }

            if (!getOnCollectionItem)
            {
                return Container.Value.GetOverride(Member);
            }
            else
            {
                // TODO
                return Container.Value.GetOverride(Member);
            }
        }

        public void SetContentOverride(OverrideType overrideType, object index)
        {
            bool setOnCollectionItem = false;
            if (index != null)
            {
                var value = Retrieve(index);
                // We can set override flags on item of collection if they are not value type and are identifiable
                if (value != null && !value.GetType().IsValueType)
                    setOnCollectionItem = true;
            }

            if (!setOnCollectionItem)
            {
                Container.Value.SetOverride(Member, overrideType);
            }
            else
            {
                // TODO: Set the override on the item of the list instead of the list itself
                Container.Value.SetOverride(Member, overrideType);
            }
        }
    }
}