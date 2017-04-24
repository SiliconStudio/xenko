// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Assembly attribute used to mark assembly that has been preprocessed using the <see cref="ParameterKeyProcessor"/>.
    /// Assemblies without this attribute will have all of their type members tagged with <see cref="EffectKeysAttribute"/> scanned for <see cref="ParameterKey"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class AssemblyEffectKeysAttribute : Attribute
    {
    }
}
