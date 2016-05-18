// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Quantum.Commands
{
    public class AddPrimitiveKeyCommand : SyncNodeCommandBase
    {
        public const string CommandName = "AddPrimitiveKey";

        /// <inheritdoc/>
        public override string Name => CommandName;

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

            return !dictionaryDescriptor.KeyType.IsClass || dictionaryDescriptor.KeyType == typeof(string) || dictionaryDescriptor.KeyType.GetConstructor(Type.EmptyTypes) != null;
        }

        protected override void ExecuteSync(IContent content, Index index, object parameter)
        {
            var value = content.Retrieve(index);
            var dictionaryDescriptor = (DictionaryDescriptor)TypeDescriptorFactory.Default.Find(value.GetType());
            var newKey = dictionaryDescriptor.KeyType != typeof(string) ? new Index(Activator.CreateInstance(dictionaryDescriptor.KeyType)) : GenerateStringKey(value, dictionaryDescriptor, parameter as string);
            object newItem = null;
            // TODO: Find a better solution that doesn't require to reference Core.Serialization (and unreference this assembly)
            if (!dictionaryDescriptor.ValueType.GetCustomAttributes(typeof(ContentSerializerAttribute), true).Any())
                newItem = !dictionaryDescriptor.ValueType.IsAbstract ? Activator.CreateInstance(dictionaryDescriptor.ValueType) : null;
            content.Add(newItem, newKey);
        }

        private static Index GenerateStringKey(object value, ITypeDescriptor descriptor, string baseValue)
        {
            // TODO: use a dialog service and popup a message when the given key is invalid
            var baseName = GenerateBaseName(baseValue);
            var i = 1;

            var dictionary = (DictionaryDescriptor)descriptor;
            while (dictionary.ContainsKey(value, baseName))
            {
                baseName = (baseValue ?? "Key") + " " + ++i;
            }

            return new Index(baseName);
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
