// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Assets.Compiler
{
    /// <summary>
    /// Attribute to define an asset compiler for a <see cref="Asset"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    [BaseTypeRequired(typeof(Asset))]
    public sealed class AssetCompilerAttribute : CompilerAttribute
    {
        public AssetCompilerAttribute(Type type)
            : base(type)
        {
        }

        public AssetCompilerAttribute(string typeName)
            : base(typeName)
        {
        }
    }
}
