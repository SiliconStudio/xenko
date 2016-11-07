// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

namespace SiliconStudio.Core.Yaml.Serialization
{
    /// <summary>
    /// Attribute use to tag a class that is implementing a <see cref="IYamlSerializable"/> or <see cref="IYamlSerializableFactory"/>
    /// and will be used for asset serialization.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class YamlSerializerFactoryAttribute : Attribute
    {
        private static readonly string[] EmptyList = new string[0];

        public YamlSerializerFactoryAttribute(params string[] profiles)
        {
            Profiles = profiles ?? EmptyList;
        }

        public IReadOnlyCollection<string> Profiles { get; }
    }
}
