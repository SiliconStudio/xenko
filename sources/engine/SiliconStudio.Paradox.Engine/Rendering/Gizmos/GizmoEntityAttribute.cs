// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Paradox.Rendering.Gizmos
{
    /// <summary>
    /// Specify the GizmoEntity class to use for designated type.
    /// </summary>
    public class GizmoEntityAttribute : DynamicTypeAttributeBase
    {
        public GizmoEntityAttribute(Type type)
            : base(type)
        {
        }

        public GizmoEntityAttribute(string typeName)
            : base(typeName)
        {
        }
    }
}