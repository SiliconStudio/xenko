// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Updater;

namespace SiliconStudio.Paradox.Engine.Design
{
    class EntityChildPropertyResolver : UpdateMemberResolver
    {
        [ModuleInitializer]
        internal static void __Initialize__()
        {
            UpdateEngine.RegisterMemberResolver(new EntityComponentPropertyResolver());
        }

        public override Type SupportedType
        {
            get { return typeof(Entity); }
        }

        public override UpdatableMember ResolveProperty(string propertyName)
        {
            return new EntityChildPropertyAccessor(propertyName);
        }

        class EntityChildPropertyAccessor : UpdatableCustomAccessor
        {
            private readonly string childName;

            public EntityChildPropertyAccessor(string childName)
            {
                this.childName = childName;
            }

            /// <inheritdoc/>
            public override Type MemberType => typeof(Entity);

            /// <inheritdoc/>
            public override void GetBlittable(IntPtr obj, IntPtr data)
            {
                throw new NotSupportedException();
            }

            /// <inheritdoc/>
            public override void SetBlittable(IntPtr obj, IntPtr data)
            {
                throw new NotSupportedException();
            }

            /// <inheritdoc/>
            public override void SetStruct(IntPtr obj, object data)
            {
                throw new NotSupportedException();
            }

            /// <inheritdoc/>
            public override IntPtr GetStructAndUnbox(IntPtr obj, object data)
            {
                throw new NotSupportedException();
            }

            /// <inheritdoc/>
            public override object GetObject(IntPtr obj)
            {
                var entity = UpdateEngineHelper.PtrToObject<Entity>(obj);
                foreach (var child in entity.Transform.Children)
                {
                    var childEntity = child.Entity;
                    if (childEntity.Name == childName)
                    {
                        return childEntity;
                    }
                }

                // TODO: Instead of throwing an exception, we could just skip it
                // If we do that, we need to add how many entries to skip in the state machine
                throw new InvalidOperationException(string.Format("Could not find child entity named {0}", childName));
            }

            /// <inheritdoc/>
            public override void SetObject(IntPtr obj, object data)
            {
                throw new NotSupportedException();
            }
        }
    }
}