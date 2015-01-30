// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Quantum.Commands
{
    public class CreateNewInstanceCommand : ModifyValueCommand
    {
        private static readonly object SetToNullObject = new object();

        /// <summary>
        /// An object that can be passed as parameter to the command, in order to set the value of the node to <c>null</c>.
        /// </summary>
        public static object SetToNull { get { return SetToNullObject; } }

        /// <inheritdoc/>
        public override string Name { get { return "CreateNewInstance"; } }

        /// <inheritdoc/>
        public override CombineMode CombineMode { get { return CombineMode.CombineOnlyForAll; } }

        /// <inheritdoc/>
        public override bool CanAttach(ITypeDescriptor typeDescriptor, MemberDescriptorBase memberDescriptor)
        {
            var isNullableStruct = typeDescriptor.Type.IsNullable() && Nullable.GetUnderlyingType(typeDescriptor.Type).IsStruct();
            var isAbstractOrClass = typeDescriptor.Type.IsAbstract || typeDescriptor.Type.IsClass;
            var isCollection = (typeDescriptor is CollectionDescriptor) || (typeDescriptor is DictionaryDescriptor);

            return isNullableStruct || (isAbstractOrClass && !isCollection);
        }

        /// <inheritdoc/>
        protected override object ModifyValue(object currentValue, ITypeDescriptor descriptor, object parameter)
        {
            if (parameter == SetToNull)
                return null;

            var type = parameter as Type;
            return type != null && (currentValue == null || currentValue.GetType() != type) ? ObjectFactory.NewInstance(type) : currentValue;
        }
    }
}
