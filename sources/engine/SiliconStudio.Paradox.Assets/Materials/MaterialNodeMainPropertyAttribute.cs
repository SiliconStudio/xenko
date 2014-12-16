// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Paradox.Assets.Materials
{
    // TODO: This attribute is probably not necessary - we can simple select the property with the lowest order as main property.
    /// <summary>
    /// This attribute indicates that the associated property is the main property of a <see cref="IMaterialNode"/>
    /// and should be presented as the value of the node.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class MaterialNodeValuePropertyAttribute : Attribute
    {

    }
}