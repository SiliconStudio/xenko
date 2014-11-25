// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Reflection;
using System.Text;
using SharpYaml;
using SharpYaml.Events;
using SharpYaml.Serialization;
using SiliconStudio.Core;
using SiliconStudio.Core.Yaml;

namespace SiliconStudio.Assets.Serializers
{
    [YamlSerializerFactory]
    internal class PropertyKeyYamlSerializer : AssetScalarSerializerBase
    {
        public override bool CanVisit(Type type)
        {
            // Because a PropertyKey<> inherits directly from PropertyKey, we can directly check the base only
            // ParameterKey<> inherits from ParameterKey, so it won't conflict with the custom ParameterKeyYamlSerializer
            // defined in the SiliconStudio.Paradox.Assets assembly

            if (type == typeof(PropertyKey))
            {
                return true;
            }

            for (Type t = type; t != null; t = t.BaseType)
                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(PropertyKey<>))
                    return true;

            return false;
        }

        public override object ConvertFrom(ref ObjectContext objectContext, Scalar fromScalar)
        {
            var lastDot = fromScalar.Value.LastIndexOf('.');
            if (lastDot == -1)
                return null;

            var className = fromScalar.Value.Substring(0, lastDot);

            var containingClass = objectContext.SerializerContext.TypeFromTag("!" + className); // Readd initial '!'
            if (containingClass == null)
            {
                throw new YamlException(fromScalar.Start, fromScalar.End, "Unable to find class from tag [{0}]".ToFormat(className));
            }

            var propertyName = fromScalar.Value.Substring(lastDot + 1);
            var propertyField = containingClass.GetField(propertyName, BindingFlags.Public | BindingFlags.Static);
            if (propertyField == null)
            {
                throw new YamlException(fromScalar.Start, fromScalar.End, "Unable to find property [{0}] in class [{1}]".ToFormat(propertyName, containingClass.Name));
            }

            return propertyField.GetValue(null);
        }

        protected override void WriteScalar(ref ObjectContext objectContext, ScalarEventInfo scalar)
        {
            // TODO: if ParameterKey is written to an object, It will not serialized a tag
            scalar.Tag = null;
            scalar.IsPlainImplicit = true;
            base.WriteScalar(ref objectContext, scalar);
        }

        public override string ConvertTo(ref ObjectContext objectContext)
        {
            var propertyKey = (PropertyKey)objectContext.Instance;

            return AssetPropertyKeyNameResolver.ComputePropertyKeyName(objectContext.SerializerContext, propertyKey);
        }

        
    }
}