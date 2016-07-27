// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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