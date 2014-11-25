// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Quantum.Commands
{
    public class SetToNullCommand : ModifyValueCommand
    {
        /// <inheritdoc/>
        public override string Name { get { return "SetToNull"; } }

        /// <inheritdoc/>
        public override CombineMode CombineMode { get { return CombineMode.AlwaysCombine; } }

        /// <inheritdoc/>
        public override bool CanAttach(ITypeDescriptor typeDescriptor, MemberDescriptorBase memberDescriptor)
        {
            return ((typeDescriptor.Type.IsNullable() && typeDescriptor.Type.IsStruct()) || typeDescriptor.Type.IsClass || typeDescriptor.Type.IsAbstract)
                && !(typeDescriptor is CollectionDescriptor) && !(typeDescriptor is DictionaryDescriptor);
        }

        /// <inheritdoc/>
        protected override object ModifyValue(object currentValue, ITypeDescriptor descriptor, object parameter)
        {
            return null;
        }
    }
}
