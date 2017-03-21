using System;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Assets.Compiler
{
    /// <summary>
    /// Attribute to define an asset compiler for a <see cref="Asset"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    [BaseTypeRequired(typeof(IAssetCompiler))]
    public class CompatibleAssetAttribute : CompilerAttribute
    {
        public Type CompilationContext { get; private set; }

        public CompatibleAssetAttribute(Type type, Type compilationContextType)
            : base(type)
        {
            CompilationContext = compilationContextType;
        }

        public CompatibleAssetAttribute(string typeName, Type compilationContextType)
            : base(typeName)
        {
            CompilationContext = compilationContextType;
        }
    }
}