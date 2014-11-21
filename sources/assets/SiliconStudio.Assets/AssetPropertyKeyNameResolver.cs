// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Text;

using SharpYaml.Serialization;

using SiliconStudio.Core;

namespace SiliconStudio.Assets
{
    public static class AssetPropertyKeyNameResolver
    {
        public static string ComputePropertyKeyName(ITagTypeResolver tagResolver, PropertyKey propertyKey)
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