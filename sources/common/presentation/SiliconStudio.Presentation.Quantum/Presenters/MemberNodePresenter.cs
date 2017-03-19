using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum.Presenters
{
    public class MemberNodePresenter : NodePresenterBase
    {
        protected readonly IMemberNode Member;
        private readonly List<Attribute> memberAttributes = new List<Attribute>();

        public MemberNodePresenter([NotNull] INodePresenterFactoryInternal factory, [NotNull] INodePresenter parent, [NotNull] IMemberNode member)
            : base(factory, parent)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (member == null) throw new ArgumentNullException(nameof(member));
            Member = member;
            Name = member.Name;

            memberAttributes.AddRange(TypeDescriptorFactory.Default.AttributeRegistry.GetAttributes(member.MemberDescriptor.MemberInfo));

            member.Changing += OnMemberChanging;
            member.Changed += OnMemberChanged;

            var displayAttribute = memberAttributes.OfType<DisplayAttribute>().FirstOrDefault();
            Order = displayAttribute?.Order ?? member.MemberDescriptor.Order;

            AttachCommands();
        }

        public override void Dispose()
        {
            Member.Changing -= OnMemberChanging;
            Member.Changed -= OnMemberChanged;
        }

        public override string Name { get; }

        public sealed override List<INodePresenterCommand> Commands { get; } = new List<INodePresenterCommand>();

        public override Type Type => Member.Type;

        public override bool IsPrimitive => Member.IsPrimitive;

        public override bool IsEnumerable => Member.Target?.IsEnumerable ?? false;

        public override Index Index => Index.Empty;

        public override ITypeDescriptor Descriptor => Member.Descriptor;

        public override int? Order { get; }

        public override object Value => Member.Retrieve();

        public IMemberDescriptor MemberDescriptor => Member.MemberDescriptor;

        public IReadOnlyList<Attribute> MemberAttributes => memberAttributes;

        protected override IObjectNode ParentingNode => Member.Target;

        public override event EventHandler<ValueChangingEventArgs> ValueChanging;

        public override event EventHandler<ValueChangedEventArgs> ValueChanged;

        public override void UpdateValue(object newValue)
        {
            try
            {
                Member.Update(newValue);
                Refresh();
            }
            catch (Exception e)
            {
                throw new NodePresenterException("An error occurred while updating the value of the node, see the inner exception for more information.", e);
            }
        }

        public override void AddItem(object value)
        {
            if (Member.Target == null || !Member.Target.IsEnumerable)
                throw new NodePresenterException($"{nameof(MemberNodePresenter)}.{nameof(AddItem)} cannot be invoked on members that are not collection.");

            try
            {
                Member.Target.Add(value);
                Refresh();
            }
            catch (Exception e)
            {
                throw new NodePresenterException("An error occurred while adding an item to the node, see the inner exception for more information.", e);
            }
        }

        public override void AddItem(object value, Index index)
        {
            if (Member.Target == null || !Member.Target.IsEnumerable)
                throw new NodePresenterException($"{nameof(MemberNodePresenter)}.{nameof(AddItem)} cannot be invoked on members that are not collection.");

            try
            {
                Member.Target.Add(value, index);
                Refresh();
            }
            catch (Exception e)
            {
                throw new NodePresenterException("An error occurred while adding an item to the node, see the inner exception for more information.", e);
            }
        }

        public override void RemoveItem(object value, Index index)
        {
            if (Member.Target == null || !Member.Target.IsEnumerable)
                throw new NodePresenterException($"{nameof(MemberNodePresenter)}.{nameof(RemoveItem)} cannot be invoked on members that are not collection.");

            try
            {
                Member.Target.Remove(value, index);
                Refresh();
            }
            catch (Exception e)
            {
                throw new NodePresenterException("An error occurred while removing an item to the node, see the inner exception for more information.", e);
            }
        }

        public override void UpdateItem(object newValue, Index index)
        {
            if (Member.Target == null || !Member.Target.IsEnumerable)
                throw new NodePresenterException($"{nameof(MemberNodePresenter)}.{nameof(UpdateItem)} cannot be invoked on members that are not collection.");

            try
            {
                Member.Target.Update(newValue, index);
                Refresh();
            }
            catch (Exception e)
            {
                throw new NodePresenterException("An error occurred while updating an item of the node, see the inner exception for more information.", e);
            }
        }

        public override NodeAccessor GetNodeAccessor()
        {
            return new NodeAccessor(Member, Index.Empty);
        }
        private void OnMemberChanging(object sender, MemberNodeChangeEventArgs e)
        {
            ValueChanging?.Invoke(this, new ValueChangingEventArgs(e.NewValue));
        }

        private void OnMemberChanged(object sender, MemberNodeChangeEventArgs e)
        {
            Refresh();
            ValueChanged?.Invoke(this, new ValueChangedEventArgs(e.OldValue));
        }
    }
}
