// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Shaders.Visitor;

namespace SiliconStudio.Shaders.Ast
{
    /// <summary>
    /// Instruct a <see cref="ShaderVisitor"/> to ignore a field
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class VisitorIgnoreAttribute : Attribute
    {
    }
}
