// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Assets.Compiler
{
    /// <summary>
    /// Attribute to define an asset compiler for a <see cref="Asset"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    [BaseTypeRequired(typeof(IAssetCompiler))]
    public class AssetCompilerAttribute : DynamicTypeAttributeBase
    {
        public Type CompilationContext { get; private set; }

        public AssetCompilerAttribute(Type type, Type compilationContextType)
            : base(type)
        {
            CompilationContext = compilationContextType;
        }

        public AssetCompilerAttribute(string typeName, Type compilationContextType)
            : base(typeName)
        {
            CompilationContext = compilationContextType;
        }
    }
}
