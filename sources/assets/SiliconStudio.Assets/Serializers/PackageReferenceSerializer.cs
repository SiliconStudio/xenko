// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Core.Yaml.Events;
using SiliconStudio.Core.Yaml.Serialization;

namespace SiliconStudio.Assets.Serializers
{
    /// <summary>
    /// A Yaml serializer for <see cref="PackageReference"/>
    /// </summary>
    [YamlSerializerFactory(YamlAssetProfile.Name)]
    internal class PackageReferenceSerializer : AssetScalarSerializerBase
    {
        public override bool CanVisit(Type type)
        {
            return type == typeof(PackageReference);
        }

        public override object ConvertFrom(ref ObjectContext context, Scalar fromScalar)
        {
            PackageReference packageReference;
            if (!PackageReference.TryParse(fromScalar.Value, out packageReference))
            {
                throw new YamlException(fromScalar.Start, fromScalar.End, "Unable to decode package reference [{0}]. Expecting format GUID:LOCATION".ToFormat(fromScalar.Value));
            }
            return packageReference;
        }

        public override string ConvertTo(ref ObjectContext objectContext)
        {
            return objectContext.Instance.ToString();
        }

        public override void Visit(ref VisitorContext context)
        {
            // For a package reference, we visit its members to allow to update the paths when loading, saving
            context.Visitor.VisitObject(context.Instance, context.Descriptor, true);
        }
    }
}
