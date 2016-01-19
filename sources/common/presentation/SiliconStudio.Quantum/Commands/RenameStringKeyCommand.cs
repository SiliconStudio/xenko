// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.ActionStack;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Quantum.Commands
{
    public class RenameStringKeyCommand : ActionItemNodeCommand
    {
        private class RenameStringKeyActionItem : SimpleNodeCommandActionItem
        {
            private readonly string oldName;
            private readonly string newName;

            public RenameStringKeyActionItem(string name, IContent content, object index, string newName, IEnumerable<IDirtiable> dirtiables)
                : base(name, content, index, dirtiables)
            {
                oldName = (string)index;
                this.newName = newName;
            }

            public override bool Do()
            {
                RenameKey(oldName, newName);
                return true;
            }

            protected override void UndoAction()
            {
                RenameKey(newName, oldName);
            }

            private void RenameKey(string oldKey, string newKey)
            {
                var value = Content.Retrieve();
                var dictionaryDescriptor = TypeDescriptorFactory.Default.Find(value.GetType()) as DictionaryDescriptor;
                if (dictionaryDescriptor == null)
                    throw new InvalidOperationException("This command cannot be executed on the given object.");

                var removedObject = dictionaryDescriptor.GetValue(value, oldKey);
                dictionaryDescriptor.Remove(value, oldKey);
                dictionaryDescriptor.SetValue(value, newKey, removedObject);
                Content.Update(value);
            }
        }

        /// <inheritdoc/>
        public override string Name => "RenameStringKey";

        /// <inheritdoc/>
        public override CombineMode CombineMode => CombineMode.AlwaysCombine;

        /// <inheritdoc/>
        public override bool CanAttach(ITypeDescriptor descriptor, MemberDescriptorBase memberDescriptor)
        {
            if (memberDescriptor != null)
            {
                var attrib = TypeDescriptorFactory.Default.AttributeRegistry.GetAttribute<MemberCollectionAttribute>(memberDescriptor.MemberInfo);
                if (attrib != null && attrib.ReadOnly)
                    return false;
            }

            var dictionaryDescriptor = descriptor as DictionaryDescriptor;
            return dictionaryDescriptor != null && dictionaryDescriptor.KeyType == typeof(string);
        }

        protected override NodeCommandActionItem CreateActionItem(IContent content, object index, object parameter, IEnumerable<IDirtiable> dirtiables)
        {
            return new RenameStringKeyActionItem(Name, content, index, (string)parameter, dirtiables);
        }
    }
}