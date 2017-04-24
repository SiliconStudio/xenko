// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.ComponentModel;
using System.Reflection;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Core
{
    public class PropertyChangedExtendedEventArgs : PropertyChangedEventArgs
    {
        public PropertyChangedExtendedEventArgs([NotNull] PropertyInfo propertyInfo, object oldValue, object newValue) : base(propertyInfo.Name)
        {
            PropertyInfo = propertyInfo;
            OldValue = oldValue;
            NewValue = newValue;
        }

        public PropertyInfo PropertyInfo { get; private set; }
        public object NewValue { get; private set; }
        public object OldValue { get; private set; }
    }
}
