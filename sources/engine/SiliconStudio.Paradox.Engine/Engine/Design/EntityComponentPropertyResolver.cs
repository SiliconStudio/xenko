// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Updater;

namespace SiliconStudio.Paradox.Engine.Design
{
    class EntityComponentPropertyResolver : UpdateMemberResolver
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
            var dotIndex = propertyName.LastIndexOf(propertyName);
            if (dotIndex == -1)
                return null;

            // TODO: Temporary hack to get static field of the requested type/property name
            // Need to have access to DataContract name<=>type mapping in the runtime (only accessible in SiliconStudio.Core.Design now)
            var type = AssemblyRegistry.GetType(propertyName.Substring(0, dotIndex));
            var field = type.GetField(propertyName.Substring(dotIndex + 1));

            return new EntityComponentPropertyAccessor((PropertyKey)field.GetValue(null));
        }

        private class EntityComponentPropertyAccessor : UpdatableCustomAccessor
        {
            private readonly PropertyKey propertyKey;

            public EntityComponentPropertyAccessor(PropertyKey propertyKey)
            {
                this.propertyKey = propertyKey;
            }

            /// <inheritdoc/>
            public override Type MemberType => propertyKey.PropertyType;

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
                return entity.Components[propertyKey];
            }

            /// <inheritdoc/>
            public override void SetObject(IntPtr obj, object data)
            {
                var entity = UpdateEngineHelper.PtrToObject<Entity>(obj);
                entity.Components[propertyKey] = data;
            }
        }
    }
}