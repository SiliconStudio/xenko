// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Linq;

using SiliconStudio.ActionStack;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Quantum.Commands
{
    public class AddPrimitiveKeyCommand : SimpleNodeCommand
    {
        /// <inheritdoc/>
        public override string Name { get { return "AddPrimitiveKey"; } }

        /// <inheritdoc/>
        public override CombineMode CombineMode { get { return CombineMode.CombineOnlyForAll; } }
        
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

        /// <inheritdoc/>
        protected override object Do(object currentValue, object parameter, out UndoToken undoToken)
        {
            var dictionaryDescriptor = (DictionaryDescriptor)TypeDescriptorFactory.Default.Find(currentValue.GetType());
            var newKey = dictionaryDescriptor.KeyType != typeof(string) ? Activator.CreateInstance(dictionaryDescriptor.KeyType) : GenerateStringKey(currentValue, dictionaryDescriptor, parameter);
            object newItem = null;
            // TODO: Find a better solution that doesn't require to reference Core.Serialization (and unreference this assembly)
            if (!dictionaryDescriptor.ValueType.GetCustomAttributes(typeof(ContentSerializerAttribute), true).Any())
                newItem = !dictionaryDescriptor.ValueType.IsAbstract ? Activator.CreateInstance(dictionaryDescriptor.ValueType) : null;
            dictionaryDescriptor.SetValue(currentValue, newKey, newItem);
            undoToken = new UndoToken(true, newKey);
            return currentValue;
        }

        /// <inheritdoc/>
        protected override object Undo(object currentValue, UndoToken undoToken)
        {
            var dictionaryDescriptor = (DictionaryDescriptor)TypeDescriptorFactory.Default.Find(currentValue.GetType());
            var key = undoToken.TokenValue;
            dictionaryDescriptor.Remove(currentValue, key);
            return currentValue;
        }

        private static object GenerateStringKey(object value, ITypeDescriptor descriptor, object baseValue)
        {
            // TODO: use a dialog service and popup a message when the given key is invalid
            string baseName = GenerateBaseName(baseValue);
            int i = 1;

            var dictionary = (DictionaryDescriptor)descriptor;
            while (dictionary.ContainsKey(value, baseName))
            {
                baseName = (baseValue != null ? baseValue.ToString() : "Key") + " " + ++i;
            }

            return baseName;
        }

        private static string GenerateBaseName(object baseValue)
        {
            const string DefaultKey = "Key";

            if (baseValue == null)
                return DefaultKey;
            
            var baseName = baseValue.ToString();
            if (string.IsNullOrWhiteSpace(baseName))
                return DefaultKey;

            if (baseName.Any(x => !Char.IsLetterOrDigit(x) && x == ' ' && x == '_'))
                return DefaultKey;

            return baseName;
        }
    }
}
