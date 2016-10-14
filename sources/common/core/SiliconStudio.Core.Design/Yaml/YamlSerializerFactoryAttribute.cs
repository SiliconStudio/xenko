// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core.Yaml.Serialization;

namespace SiliconStudio.Core.Yaml
{
    /// <summary>
    /// Attribute use to tag a class that is implementing a <see cref="IYamlSerializable"/> or <see cref="IYamlSerializableFactory"/>
    /// and will be used for asset serialization.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class YamlSerializerFactoryAttribute : Attribute
    {
    }
}