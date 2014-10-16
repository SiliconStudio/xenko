// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;

namespace SiliconStudio.Quantum.Legacy.Contents
{
    public class PropertyKeyViewModelContent : UnaryViewModelContentBase
    {
        private readonly PropertyKey propertyKey;

        public PropertyKeyViewModelContent(IContent operand, PropertyKey propertyKey)
            : base(propertyKey.PropertyType, operand)
        {
            this.propertyKey = propertyKey;
        }

        public override object Value
        {
            get { return ((PropertyContainer)Operand.Value).Get(propertyKey); }
            set { ((PropertyContainer)Operand.Value).SetObject(propertyKey, value); }
        }
    }
}