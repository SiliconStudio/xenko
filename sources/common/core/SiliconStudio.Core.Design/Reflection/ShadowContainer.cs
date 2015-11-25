// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;

namespace SiliconStudio.Core.Reflection
{
    internal sealed class ShadowContainer
    {
        // TODO: this class is not threadsafe. 

        private static readonly IEnumerable<ShadowAttributes> EmptyAttributes = Enumerable.Empty<ShadowAttributes>();
        private Dictionary<object, ShadowAttributes> attachedAttributesPerKey;
        private Guid? id;
        private bool isIdentifiable;

        internal ShadowContainer()
        {
        }

        public ShadowContainer(Type type)
        {
            isIdentifiable = IdentifiableHelper.IsIdentifiable(type);
        }

        public ShadowContainer(ShadowContainer copy)
        {
            copy.CopyTo(this);
        }

        public bool HasId(object instance)
        {
            return instance is IIdentifiable || id.HasValue;
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

        public ShadowContainer Clone()
        {
            var container = new ShadowContainer(this);
            return container;
        }

        internal void CopyTo(ShadowContainer copy)
        {
            copy.id = id;
            copy.isIdentifiable = isIdentifiable;

            if (attachedAttributesPerKey != null)
            {
                copy.attachedAttributesPerKey = new Dictionary<object, ShadowAttributes>();
                foreach (var keyValue in attachedAttributesPerKey)
                {
                    copy.attachedAttributesPerKey.Add(keyValue.Key, keyValue.Value.Clone());
                }
            }
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