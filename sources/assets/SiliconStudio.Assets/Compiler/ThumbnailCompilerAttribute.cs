// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Assets.Compiler
{
    /// <summary>
    /// Attribute to define for a thumbnail compiler for an <see cref="Asset"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ThumbnailCompilerAttribute : CompilerAttribute
    {
        public ThumbnailCompilerAttribute(Type type)
            : base(type)
        {
        }

        public ThumbnailCompilerAttribute(string typeName)
            : base(typeName)
        {
        }
    }
}