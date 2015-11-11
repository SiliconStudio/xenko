// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;

namespace SiliconStudio.Core.Reflection
{
    internal sealed class ShadowContainer
    {
        private static readonly IEnumerable<ShadowAttributes> EmptyAttributes = Enumerable.Empty<ShadowAttributes>();
        private Dictionary<object, ShadowAttributes> attachedAttributesPerKey;

        public ShadowContainer()
        {
        }

        public IEnumerable<ShadowAttributes> Members
        {
            get
            {
                if (attachedAttributesPerKey == null)
                    return EmptyAttributes;

                return attachedAttributesPerKey.Values;
            }
        }

        public ShadowContainer Clone()
        {
            if (attachedAttributesPerKey == null)
            {
                return null;
            }

            var container = new ShadowContainer();
            
            container.attachedAttributesPerKey = new Dictionary<object, ShadowAttributes>();
            foreach (var keyValue in attachedAttributesPerKey)
            {
                container.attachedAttributesPerKey.Add(keyValue.Key, keyValue.Value.Clone());
            }

            return container;
        }

        public bool Contains(object memberKey)
        {
            if (memberKey == null) throw new ArgumentNullException("memberKey");
            if (attachedAttributesPerKey == null)
                return false;
            return attachedAttributesPerKey.ContainsKey(memberKey);
        }

        public bool TryGetAttributes(object memberKey, out ShadowAttributes shadowAttributes)
        {
            if (memberKey == null) throw new ArgumentNullException("memberKey");
            shadowAttributes = null;
            if (attachedAttributesPerKey == null)
                return false;

            return attachedAttributesPerKey.TryGetValue(memberKey, out shadowAttributes);
        }

        public ShadowAttributes GetAttributes(object memberKey)
        {
            if (memberKey == null) throw new ArgumentNullException("memberKey");
            if (attachedAttributesPerKey == null)
                attachedAttributesPerKey = new Dictionary<object, ShadowAttributes>();

            ShadowAttributes shadowAttributes;
            if (!attachedAttributesPerKey.TryGetValue(memberKey, out shadowAttributes))
            {
                shadowAttributes = new ShadowAttributes(this, memberKey);
                attachedAttributesPerKey.Add(memberKey, shadowAttributes);
            }
            return shadowAttributes;
        }

        public ShadowAttributes this[object memberKey]
        {
            get
            {
                return GetAttributes(memberKey);
            }
        }
    }
}