// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.ActionStack;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Quantum.Commands
{
    public class AddPrimitiveKeyCommand : SimpleNodeCommand
    {
        private class AddPrimitiveKeyActionItem : SimpleNodeCommandActionItem
        {
            private readonly string keyName;
            private object newKey;

            public AddPrimitiveKeyActionItem(string name, IContent content, object index, string keyName, IEnumerable<IDirtiable> dirtiables)
                : base(name, content, index, dirtiables)
            {
                this.keyName = keyName;
            }

            public override bool Do()
            {
                var value = Content.Retrieve(Index);
                var dictionaryDescriptor = (DictionaryDescriptor)TypeDescriptorFactory.Default.Find(value.GetType());
                newKey = dictionaryDescriptor.KeyType != typeof(string) ? Activator.CreateInstance(dictionaryDescriptor.KeyType) : GenerateStringKey(value, dictionaryDescriptor, keyName);
                object newItem = null;
                // TODO: Find a better solution that doesn't require to reference Core.Serialization (and unreference this assembly)
                if (!dictionaryDescriptor.ValueType.GetCustomAttributes(typeof(ContentSerializerAttribute), true).Any())
                    newItem = !dictionaryDescriptor.ValueType.IsAbstract ? Activator.CreateInstance(dictionaryDescriptor.ValueType) : null;
                dictionaryDescriptor.SetValue(value, newKey, newItem);
                Content.Update(value, Index);
                return true;
            }

            protected override void UndoAction()
            {
                var value = Content.Retrieve(Index);
                var dictionaryDescriptor = (DictionaryDescriptor)TypeDescriptorFactory.Default.Find(value.GetType());
                dictionaryDescriptor.Remove(value, newKey);
                Content.Update(value, Index);
            }
        }

        /// <inheritdoc/>
        public override string Name => "AddPrimitiveKey";

        /// <inheritdoc/>
        public override CombineMode CombineMode => CombineMode.CombineOnlyForAll;

        /// <inheritdoc/>
        public override bool CanAttach(ITypeDescriptor typeDescriptor, MemberDescriptorBase memberDescriptor)
        {
            if (memberDescriptor != null)
            {
                var attrib = TypeDescriptorFactory.Default.AttributeRegistry.GetAttribute<MemberCollectionAttribute>(memberDescriptor.MemberInfo);
                if (attrib != null && attrib.ReadOnly)
                    return false;
            }
            
            var dictionaryDescriptor = typeDescriptor as DictionaryDescriptor;
            if (dictionaryDescriptor == null)
                return false;
            return !dictionaryDescriptor.KeyType.IsClass || dictionaryDescriptor.KeyType == typeof(string) || dictionaryDescriptor.KeyType.GetConstructor(new Type[0]) != null;
        }

        protected override SimpleNodeCommandActionItem CreateActionItem(IContent content, object index, object parameter, IEnumerable<IDirtiable> dirtiables)
        {
            return new AddPrimitiveKeyActionItem(Name, content, index, parameter as string, dirtiables);
        }


        private static object GenerateStringKey(object value, ITypeDescriptor descriptor, string baseValue)
        {
            // TODO: use a dialog service and popup a message when the given key is invalid
            string baseName = GenerateBaseName(baseValue);
            int i = 1;

            var dictionary = (DictionaryDescriptor)descriptor;
            while (dictionary.ContainsKey(value, baseName))
            {
                baseName = (baseValue ?? "Key") + " " + ++i;
            }

            return baseName;
        }

        private static string GenerateBaseName(string baseName)
        {
            const string defaultKey = "Key";

            if (string.IsNullOrWhiteSpace(baseName))
                return defaultKey;

            if (baseName.Any(x => !char.IsLetterOrDigit(x) && x == ' ' && x == '_'))
                return defaultKey;

            return baseName;
        }
    }
}
