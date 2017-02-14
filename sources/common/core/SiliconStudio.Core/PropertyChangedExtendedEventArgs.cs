// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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