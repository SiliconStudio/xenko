using System;
using System.Collections;
using System.Linq;
using SiliconStudio.Core.Extensions;

namespace SiliconStudio.Presentation.Quantum
{
    public abstract class VirtualObservableNode : SingleObservableNode
    {
        protected readonly Func<object> Getter;
        protected readonly Action<object> Setter;

        static VirtualObservableNode()
        {
            typeof(VirtualObservableNode).GetProperties().Select(x => x.Name).ForEach(x => ReservedNames.Add(x));
        }

        protected VirtualObservableNode(ObservableViewModel owner, string name, bool isPrimitive, int? order, object index, Func<object> getter, Action<object> setter)
            : base(owner, name, index)
        {
            if (getter == null) throw new ArgumentNullException(nameof(getter));
            Getter = getter;
            Setter = setter;
            Order = order;
            IsPrimitive = isPrimitive;
            Name = name;
        }

        internal static VirtualObservableNode Create(ObservableViewModel owner, string name, Type contentType, bool isPrimitive, int? order, object index, Func<object> getter, Action<object> setter)
        {
            var node = (VirtualObservableNode)Activator.CreateInstance(typeof(VirtualObservableNode<>).MakeGenericType(contentType), owner, name, isPrimitive, order, index, getter, setter);
            return node;
        }

        public override int? Order { get; }

        public override bool HasList => typeof(ICollection).IsAssignableFrom(Type);

        public override bool HasDictionary => typeof(IDictionary).IsAssignableFrom(Type);

        public override bool IsPrimitive { get; }

        /// <summary>
        /// Clears the list of children from this <see cref="VirtualObservableNode"/>.
        /// </summary>
        public void ClearChildren()
        {
            foreach (var child in Children.Cast<ObservableNode>().ToList())
            {
                RemoveChild(child);
            }
        }

        public new void AddCommand(INodeCommandWrapper command)
        {
            base.AddCommand(command);
        }
    }

    public class VirtualObservableNode<T> : VirtualObservableNode
    {
        public VirtualObservableNode(ObservableViewModel owner, string name, bool isPrimitive, int? order, object index, Func<object> getter, Action<object> setter)
            : base(owner, name, isPrimitive, order, index, getter, setter)
        {
            DependentProperties.Add(nameof(TypedValue), new[] { nameof(Value) });
        }

        /// <summary>
        /// Gets or sets the value of this node through a correctly typed property, which is more adapted to binding.
        /// </summary>
        public T TypedValue { get { return (T)Getter(); } set { SetValue(() => Setter(value)); } }

        /// <inheritdoc/>
        public override Type Type => typeof(T);

        /// <inheritdoc/>
        public override sealed object Value { get { return TypedValue; } set { TypedValue = (T)value; } }
    }
}