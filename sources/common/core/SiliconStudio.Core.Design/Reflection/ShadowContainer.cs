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
        private Guid? id;
        private readonly bool isIdentifiable;

        public ShadowContainer(Type type)
        {
            isIdentifiable = IdentifiableHelper.IsIdentifiable(type);
        }

        public Guid GetId(object instance)
        {
            // If the object is not identifiable, early exit
            if (!isIdentifiable)
            {
                return Guid.Empty;
            }

            // Don't use  local id if the object is already identifiable
            var @component = instance as IIdentifiable;
            if (@component != null)
            {
                return @component.Id;
            }

            // If we don't have yet an id, create one.
            if (!id.HasValue)
            {
                id = Guid.NewGuid();
            }

            return id.Value;
        }

        public void SetId(object instance, Guid id)
        {
            // If the object is not identifiable, early exit
            if (!isIdentifiable)
            {
                return;
            }

            // If the object instance is already identifiable, store id into it directly
            var @component = instance as IIdentifiable;
            if (@component != null)
            {
                @component.Id = id;
            }
            else
            {
                this.id = id;
            }
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

        public ShadowContainer Clone(object toInstance)
        {
            if (attachedAttributesPerKey == null)
            {
                return null;
            }

            var container = new ShadowContainer(toInstance.GetType()) { attachedAttributesPerKey = new Dictionary<object, ShadowAttributes>() };

            // Copy only Id if it is declared as local and it is an identifiable type
            if (isIdentifiable && id.HasValue)
            {
                container.id = id;
            }

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