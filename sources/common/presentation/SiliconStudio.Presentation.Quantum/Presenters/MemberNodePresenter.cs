using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Commands;

namespace SiliconStudio.Presentation.Quantum.Presenters
{
    public class MemberNodePresenter : IInitializingNodePresenter
    {
        private readonly IMemberNode member;
        private readonly List<INodePresenter> children = new List<INodePresenter>();
        private readonly List<Attribute> memberAttributes = new List<Attribute>();

        public MemberNodePresenter(INodePresenter parent, [NotNull] IMemberNode member)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            this.member = member;
            Name = member.Name;
            Parent = parent;

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

        public void Dispose()
        {
            member.Changing -= OnMemberChanging;
            member.Changed -= OnMemberChanged;
        }

        public string Name { get; }

        public INodePresenter Parent { get; }

        public IReadOnlyList<INodePresenter> Children => children;

        public List<INodeCommand> Commands { get; } = new List<INodeCommand>();

        public Type Type => member.Type;

        public bool IsPrimitive => member.IsPrimitive;

        public Index Index => Index.Empty;

        public ITypeDescriptor Descriptor { get; }

        public int? Order { get; }

        public object Value { get { return member.Retrieve(); } set { member.Update(value); } }

        public IReadOnlyList<Attribute> MemberAttributes => memberAttributes;

        public event EventHandler<ValueChangingEventArgs> ValueChanging;

        public event EventHandler<ValueChangedEventArgs> ValueChanged;

        private void OnMemberChanging(object sender, MemberNodeChangeEventArgs e)
        {
            ValueChanging?.Invoke(this, new ValueChangingEventArgs(e.NewValue));
        }

        private void OnMemberChanged(object sender, MemberNodeChangeEventArgs e)
        {
            ValueChanged?.Invoke(this, new ValueChangedEventArgs(e.OldValue));
        }

        void IInitializingNodePresenter.AddChild([NotNull] IInitializingNodePresenter child)
        {
            children.Add(child);
        }

        void IInitializingNodePresenter.FinalizeInitialization()
        {
            children.Sort(GraphNodePresenter.CompareChildren);
        }
    }
}
