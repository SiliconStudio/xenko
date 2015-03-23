// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SiliconStudio.Presentation.Quantum
{
    public abstract class VirtualObservableNode : SingleObservableNode
    {
        private readonly Dictionary<string, object> associatedData = new Dictionary<string,object>();
        private readonly int? order;
        private readonly bool isPrimitive;

        protected VirtualObservableNode(ObservableViewModel ownerViewModel, string name, SingleObservableNode parentNode, int? order, bool isPrimitive, object index, NodeCommandWrapperBase valueChangedCommand)
            : base(ownerViewModel, name, parentNode, index)
        {
            this.order = order;
            this.isPrimitive = isPrimitive;
            Name = name;
            ValueChangedCommand = valueChangedCommand;
        }

        internal static VirtualObservableNode Create(ObservableViewModel ownerViewModel, string name, IObservableNode parentNode, int? order, bool isPrimitive, Type contentType, object initialValue, object index, NodeCommandWrapperBase valueChangedCommand)
        {
            var node = (VirtualObservableNode)Activator.CreateInstance(typeof(VirtualObservableNode<>).MakeGenericType(contentType), ownerViewModel, name, parentNode, order, isPrimitive, initialValue, index, valueChangedCommand);
            return node;
        }

        public override Dictionary<string, object> AssociatedData { get { return associatedData; } }

        public override int? Order { get { return order; } }
        
        public override bool HasList { get { return typeof(ICollection).IsAssignableFrom(Type); } }

        public override bool HasDictionary { get { return typeof(IDictionary).IsAssignableFrom(Type); } }

        public override bool IsPrimitive { get { return isPrimitive; } }

        /// <summary>
        /// Gets the command to execute when the value of this node is changed.
        /// </summary>
        public NodeCommandWrapperBase ValueChangedCommand { get; private set; }

        /// <summary>
        /// Clears the list of children from this <see cref="VirtualObservableNode"/>.
        /// </summary>
        public void ClearChildren()
        {
            foreach (var child in Children.ToList())
            {
                RemoveChild(child);
            }
        }

        public new void AddCommand(INodeCommandWrapper command)
        {
            base.AddCommand(command);
        }

        internal void AddAssociatedData(string key, object data)
        {
            associatedData.Add(key, data);
        }
    }

    public class VirtualObservableNode<T> : VirtualObservableNode
    {
        private T value;

        public VirtualObservableNode(ObservableViewModel ownerViewModel, string name, SingleObservableNode parentNode, int? order, bool isPrimitive, object initialValue, object index, NodeCommandWrapperBase valueChangedCommand = null)
            : base(ownerViewModel, name, parentNode, order, isPrimitive, index, valueChangedCommand)
        {
            value = (T)initialValue;
        }

        /// <summary>
        /// Gets or sets the value of this node through a correctly typed property, which is more adapted to binding.
        /// </summary>
        public T TypedValue
        {
            get { return value; }
            set
            {
                bool hasChanged = SetValue(ref this.value, value);
                if (hasChanged && ValueChangedCommand != null)
                {
                    ValueChangedCommand.Execute(value);
                    OnValueChanged();
                }
            }
        }

        /// <inheritdoc/>
        public override Type Type { get { return typeof(T); } }

        /// <inheritdoc/>
        public override sealed object Value { get { return TypedValue; } set { TypedValue = (T)value; } }
    }
}