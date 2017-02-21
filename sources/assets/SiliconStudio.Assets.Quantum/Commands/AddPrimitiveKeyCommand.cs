// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Linq;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Commands;

namespace SiliconStudio.Assets.Quantum.Commands
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

            // Check key type
            if (!CanConstruct(dictionaryDescriptor.KeyType))
                return false;

            // Check value type
            var elementType = dictionaryDescriptor.ValueType;
            return CanConstruct(elementType) || elementType.IsAbstract || elementType.IsNullable() || IsReferenceType(elementType);
        }

        protected override void ExecuteSync(IGraphNode node, Index index, object parameter)
        {
            var objectNode = (IObjectNode)node;
            var value = node.Retrieve(index);
            var dictionaryDescriptor = (DictionaryDescriptor)TypeDescriptorFactory.Default.Find(value.GetType());
            var newKey = dictionaryDescriptor.KeyType != typeof(string) ? new Index(Activator.CreateInstance(dictionaryDescriptor.KeyType)) : GenerateStringKey(value, dictionaryDescriptor, parameter as string);
            // default(T)
            var newItem = dictionaryDescriptor.ValueType.IsValueType ? Activator.CreateInstance(dictionaryDescriptor.ValueType) : null;

            var propertyGraph = (node as IAssetNode)?.PropertyGraph;
            var newInstance = CreateInstance(dictionaryDescriptor.ValueType);
            if (!IsReferenceType(dictionaryDescriptor.ValueType) && (propertyGraph == null || !propertyGraph.IsObjectReference(node, index, newInstance)))
                newItem = newInstance;

            objectNode.Add(newItem, newKey);
        }

        /// <summary>
        /// Creates an instance of the specified type using that type's default constructor.
        /// </summary>
        /// <param name="type">The type of object to create.</param>
        /// <returns>A reference to the newly created object.</returns>
        /// <seealso cref="Activator.CreateInstance(Type)"/>
        /// <exception cref="ArgumentNullException">type is null.</exception>
        private static object CreateInstance(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            // abstract type cannot be instantiated
            if (type.IsAbstract)
                return null;

            // string is a special case
            if (type == typeof(string))
                return string.Empty;

            // note:
            //      Type not having a public parameterless constructor will throw a MissingMethodException at this point.
            //      This is intended as YAML serialization requires this constructor.
            return ObjectFactoryRegistry.NewInstance(type);
        }

        internal static Index GenerateStringKey(object value, ITypeDescriptor descriptor, string baseValue)
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

            if (baseName.Any(x => !char.IsLetterOrDigit(x) && x != ' ' && x != '_'))
                return defaultKey;

            return baseName;
        }

        private static bool CanConstruct(Type type) => !type.IsClass || type.GetConstructor(Type.EmptyTypes) != null || type == typeof(string);

        private static bool IsReferenceType(Type type) => AssetRegistry.IsContentType(type) || typeof(AssetReference).IsAssignableFrom(type);
    }
}
