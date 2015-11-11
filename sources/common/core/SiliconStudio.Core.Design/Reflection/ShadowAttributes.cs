// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Core.Reflection
{
    internal sealed class ShadowAttributes
    {
        private readonly ShadowContainer container;
        private readonly object memberKey;

        public ShadowAttributes(ShadowContainer container, object memberKey)
        {
            if (container == null) throw new ArgumentNullException("container");
            if (memberKey == null) throw new ArgumentNullException("memberKey");

            this.container = container;
            this.memberKey = memberKey;
        }

        public ShadowContainer Container
        {
            get
            {
                return container;
            }
        }

        public object Key
        {
            get
            {
                return memberKey;
            }
        }

        public ShadowAttributes Clone()
        {
            // Shallow clone, assuming that attributes values are vauetypes.
            var newShadowAttributes = new ShadowAttributes(Container, Key);
            Attributes.CopyTo(ref newShadowAttributes.Attributes);
            return newShadowAttributes;
        }

        public bool HasAttribute(PropertyKey key)
        {
            return Attributes.ContainsKey(key);
        }

        public T GetAttribute<T>(PropertyKey<T> key)
        {
            return Attributes.Get(key);
        }

        public bool TryGetAttribute<T>(PropertyKey<T> key, out T value)
        {
            return Attributes.TryGetValue(key, out value);
        }

        public void SetAttribute<T>(PropertyKey<T> key, T value)
        {
            Attributes.SetObject(key, value);
        }

        public PropertyContainer Attributes;
    }
}