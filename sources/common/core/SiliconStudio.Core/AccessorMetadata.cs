// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Core
{
    public class AccessorMetadata : PropertyKeyMetadata
    {
        public delegate void SetterDelegate(ref PropertyContainer propertyContainer, object value);
        public delegate object GetterDelegate(ref PropertyContainer propertyContainer);


        private SetterDelegate setter;
        private GetterDelegate getter;

        public AccessorMetadata(GetterDelegate getter, SetterDelegate setter)
        {
            this.getter = getter;
            this.setter = setter;
        }

        public object GetValue(ref PropertyContainer obj)
        {
            return getter(ref obj);
        }

        public void SetValue(ref PropertyContainer obj, object value)
        {
            setter(ref obj, value);
        }
    }
}