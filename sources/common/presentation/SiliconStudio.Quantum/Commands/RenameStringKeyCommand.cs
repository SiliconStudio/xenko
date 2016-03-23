// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Quantum.Commands
{
    public class RenameStringKeyCommand : NodeCommandBase
    {
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

        public override void Execute(IContent content, object index, object parameter)
        {
            var oldName = (string)index;
            var newName = (string)parameter;
            var renamedObject = content.Retrieve(oldName);
            content.Remove(oldName, renamedObject);
            content.Add(newName, renamedObject);
        }
    }
}