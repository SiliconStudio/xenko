// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Paradox.EntityModel
{
    public class GizmoEntityFactoryAttribute : DynamicTypeAttributeBase
    {
        public GizmoEntityFactoryAttribute(Type type)
            : base(type)
        {
        }

        public GizmoEntityFactoryAttribute(string typeName)
            : base(typeName)
        {
        }
    }
}