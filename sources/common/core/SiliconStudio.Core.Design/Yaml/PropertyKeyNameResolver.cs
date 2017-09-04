// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Text;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Yaml.Serialization;

namespace SiliconStudio.Core.Yaml
{
    public static class PropertyKeyNameResolver
    {
        [NotNull]
        public static string ComputePropertyKeyName([NotNull] ITagTypeResolver tagResolver, [NotNull] PropertyKey propertyKey)
        {
            var className = tagResolver.TagFromType(propertyKey.OwnerType);
            var sb = new StringBuilder(className.Length + 1 + propertyKey.Name.Length);

            sb.Append(className, 1, className.Length - 1); // Ignore initial '!'
            sb.Append('.');
            sb.Append(propertyKey.Name);
            return sb.ToString();
        }
    }
}
