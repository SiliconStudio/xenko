// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

namespace SiliconStudio.Shaders.Ast
{
    /// <summary>
    /// An abstract class for attribute definition.
    /// </summary>
    public abstract partial class AttributeBase : Node
    {
    }

    /// <summary>
    /// An abstract class for a post attribute definition.
    /// </summary>
    public abstract partial class PostAttributeBase : AttributeBase
    {
    }
}
