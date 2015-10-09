// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Paradox.Rendering
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