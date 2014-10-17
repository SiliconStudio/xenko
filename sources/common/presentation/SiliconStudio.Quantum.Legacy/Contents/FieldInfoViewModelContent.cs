// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Reflection;

namespace SiliconStudio.Quantum.Legacy.Contents
{
    /// <summary>
    /// Implements IViewModel for a PropertyInfo property.
    /// </summary>
    public class FieldInfoViewModelContent : UnaryViewModelContentBase
    {
        private readonly FieldInfo fieldInfo;

        public FieldInfoViewModelContent(IContent operand, FieldInfo fieldInfo)
            : base(fieldInfo.FieldType, operand)
        {
            this.fieldInfo = fieldInfo;
        }

        /// <inheritdoc/>
        public override object Value
        {
            get
            {
                return fieldInfo.GetValue(Operand.Value);
            }
            set
            {
                var parentValue = Operand.Value;
                fieldInfo.SetValue(parentValue, value);

                // If container type is a structure, propagate up the updated structure (since copy only works by value)
                // TODO: really required?
                if (Operand.Type.GetTypeInfo().IsValueType)
                    Operand.Value = parentValue;
            }
        }
    }
}