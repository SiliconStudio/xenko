// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.ComponentModel;

namespace SiliconStudio.Presentation.Quantum.ComponentModel
{
    public class ObservableNodePropertyDescriptor : PropertyDescriptor
    {
        public IObservableNode Node { get; private set; }

        public string PropertyName { get { return Node.Name; } }

        public ObservableNodePropertyDescriptor(IObservableNode node)
            : base(node.Name, null)
        {
            Node = node;
        }

        public override bool CanResetValue(object component)
        {
            return true;
        }

        public override object GetValue(object component)
        {
            return ((ObservableNode)component).GetChild(Node.Name);
        }

        public override void ResetValue(object component)
        {
            var type = Node.Type;
            Node.Value = type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        public override void SetValue(object component, object value)
        {
            if (!ReferenceEquals(Node, value))
            {
                throw new InvalidOperationException("SetValue can't be invoked on an ObservableNodePropertyDescriptor");
            }
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }

        public override Type ComponentType { get { return typeof(ObservableNode); } }

        public override bool IsReadOnly { get { return false; } }

        public override Type PropertyType { get { return typeof(ObservableNode); } }
    }
}