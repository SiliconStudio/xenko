using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Commands;

namespace SiliconStudio.Presentation.Quantum.Presenters
{
    public class MemberNodePresenter : NodePresenterBase
    {
        private readonly IMemberNode member;
        private readonly List<Attribute> memberAttributes = new List<Attribute>();

        public MemberNodePresenter([NotNull] INodePresenterFactoryInternal factory, [NotNull] INodePresenter parent, [NotNull] IMemberNode member)
            : base(factory, parent)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (member == null) throw new ArgumentNullException(nameof(member));
            this.member = member;
            Name = member.Name;

            memberAttributes.AddRange(TypeDescriptorFactory.Default.AttributeRegistry.GetAttributes(member.MemberDescriptor.MemberInfo));

            member.Changing += OnMemberChanging;
            member.Changed += OnMemberChanged;


            if (member.Target != null)
            {
                // If we have a target node, register commands attached to it. They will override the commands of the member node by name.
                Commands.AddRange(member.Target.Commands);
            }

            // Register commands from the member node, skipping those already registered from the target node.
            var targetCommandNames = Commands.Select(x => x.Name).ToList();
            Commands.AddRange(member.Commands.Where(x => !targetCommandNames.Contains(x.Name)));

            var displayAttribute = memberAttributes.OfType<DisplayAttribute>().FirstOrDefault();
            Order = displayAttribute?.Order ?? member.MemberDescriptor.Order;
        }

        public override void Dispose()
        {
            member.Changing -= OnMemberChanging;
            member.Changed -= OnMemberChanged;
        }

        public override string Name { get; }

        public sealed override List<INodeCommand> Commands { get; } = new List<INodeCommand>();

        public override Type Type => member.Type;

        public override bool IsPrimitive => member.IsPrimitive;

        public override bool IsEnumerable => member.Target?.IsEnumerable ?? false;

        public override Index Index => Index.Empty;

        public override ITypeDescriptor Descriptor => member.Descriptor;

        public override int? Order { get; }

        public override object Value => member.Retrieve();

        public IMemberDescriptor MemberDescriptor => member.MemberDescriptor;

        public IReadOnlyList<Attribute> MemberAttributes => memberAttributes;

        protected override IObjectNode ParentingNode => member.Target;

        public override event EventHandler<ValueChangingEventArgs> ValueChanging;

        public override event EventHandler<ValueChangedEventArgs> ValueChanged;

        public override void UpdateValue(object newValue)
        {
            try
            {
                member.Update(newValue);
                Refresh();
            }
            catch (Exception e)
            {
                throw new NodePresenterException("An error occurred while updating the value of the node, see the inner exception for more information.", e);
            }
        }

        public override void AddItem(object value)
        {
            if (member.Target == null || !member.Target.IsEnumerable)
                throw new NodePresenterException($"{nameof(MemberNodePresenter)}.{nameof(AddItem)} cannot be invoked on members that are not collection.");

            try
            {
                member.Target.Add(value);
                Refresh();
            }
            catch (Exception e)
            {
                throw new NodePresenterException("An error occurred while adding an item to the node, see the inner exception for more information.", e);
            }
        }

        public override void AddItem(object value, Index index)
        {
            if (member.Target == null || !member.Target.IsEnumerable)
                throw new NodePresenterException($"{nameof(MemberNodePresenter)}.{nameof(AddItem)} cannot be invoked on members that are not collection.");

            try
            {
                member.Target.Add(value, index);
                Refresh();
            }
            catch (Exception e)
            {
                throw new NodePresenterException("An error occurred while adding an item to the node, see the inner exception for more information.", e);
            }
        }

        public override void RemoveItem(object value, Index index)
        {
            if (member.Target == null || !member.Target.IsEnumerable)
                throw new NodePresenterException($"{nameof(MemberNodePresenter)}.{nameof(RemoveItem)} cannot be invoked on members that are not collection.");

            try
            {
                member.Target.Remove(value, index);
                Refresh();
            }
            catch (Exception e)
            {
                throw new NodePresenterException("An error occurred while removing an item to the node, see the inner exception for more information.", e);
            }
        }

        public override void UpdateItem(object newValue, Index index)
        {
            if (member.Target == null || !member.Target.IsEnumerable)
                throw new NodePresenterException($"{nameof(MemberNodePresenter)}.{nameof(UpdateItem)} cannot be invoked on members that are not collection.");

            try
            {
                member.Target.Update(newValue, index);
                Refresh();
            }
            catch (Exception e)
            {
                throw new NodePresenterException("An error occurred while updating an item of the node, see the inner exception for more information.", e);
            }
        }

        internal override Task RunCommand(INodeCommand command, object parameter)
        {
            return command.Execute(member, Index.Empty, parameter);
        }

        private void OnMemberChanging(object sender, MemberNodeChangeEventArgs e)
        {
            ValueChanging?.Invoke(this, new ValueChangingEventArgs(e.NewValue));
        }

        private void OnMemberChanged(object sender, MemberNodeChangeEventArgs e)
        {
            ValueChanged?.Invoke(this, new ValueChangedEventArgs(e.OldValue));
        }
    }
}
