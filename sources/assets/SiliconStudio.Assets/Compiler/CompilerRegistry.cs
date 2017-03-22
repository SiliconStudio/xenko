// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using SiliconStudio.Core;

namespace SiliconStudio.Assets.Compiler
{
    public interface ICompilationContext
    {      
    }

    public class AssetCompilationContext : ICompilationContext
    {        
    }

    /// <summary>
    /// A registry containing the compiler associated to all the asset types
    /// </summary>
    /// <typeparam name="T">The type of the class implementing the <see cref="IAssetCompiler"/> interface to register.</typeparam>
    public abstract class CompilerRegistry<T> : ICompilerRegistry<T> where T: class, IAssetCompiler
    {
        private struct CompilerTypeData
        {
            public Type Type;
            public Type Context;

            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                var other = (CompilerTypeData)obj;
                return Type == other.Type && Context == other.Context;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hash = (int)2166136261;
                    hash = (hash * 16777619) ^ Type.GetHashCode();
                    hash = (hash * 16777619) ^ Context.GetHashCode();
                    return hash;
                }
            }

            public static bool operator ==(CompilerTypeData x, CompilerTypeData y)
            {
                return x.Type == y.Type && x.Context == y.Context;
            }

            public static bool operator !=(CompilerTypeData x, CompilerTypeData y)
            {
                return x.Type != y.Type || x.Context != y.Context;
            }
        }

        private readonly Dictionary<CompilerTypeData, T> typeToCompiler = new Dictionary<CompilerTypeData, T>();

        /// <summary>
        /// Gets or sets the default compiler to use when no compiler are explicitly registered for a type.
        /// </summary>
        public T DefaultCompiler { get; set; }

        /// <summary>
        /// Register a compiler for a given <see cref="Asset"/> type.
        /// </summary>
        /// <param name="type">The type of asset the compiler can compile</param>
        /// <param name="compiler">The compiler to use</param>
        /// <param name="context"></param>
        public void RegisterCompiler(Type type, T compiler, Type context)
        {
            if (compiler == null) throw new ArgumentNullException("compiler");

            AssertAssetType(type);

            var typeData = new CompilerTypeData
            {
                Context = context,
                Type = type
            };

            typeToCompiler[typeData] = compiler;
        }

        /// <summary>
        /// Gets the compiler associated to an <see cref="Asset"/> type.
        /// </summary>
        /// <param name="type">The type of the <see cref="Asset"/></param>
        /// <param name="context"></param>
        /// <returns>The compiler associated the provided asset type or null if no compiler exists for that type.</returns>
        public T GetCompiler(Type type, Type context)
        {
            AssertAssetType(type);

            EnsureTypes();

            var typeData = new CompilerTypeData
            {
                Context = context,
                Type = type
            };

            T compiler;
            if(!typeToCompiler.TryGetValue(typeData, out compiler))
            {
                //support nested context types!
                if (context.BaseType != typeof(object))
                {
                    return GetCompiler(type, context.BaseType);
                }

                compiler = DefaultCompiler;
            }

            return compiler;
        }

        protected virtual void EnsureTypes()
        {
        }


        protected void UnregisterCompilersFromAssembly(Assembly assembly)
        {
            foreach (var typeToRemove in typeToCompiler.Where(typeAndCompile => typeAndCompile.Key.Type.Assembly == assembly || typeAndCompile.Value.GetType().Assembly == assembly).Select(e => e.Key).ToList())
            {
                typeToCompiler.Remove(typeToRemove);
            }
        }

        private static void AssertAssetType(Type assetType)
        {
            if (assetType == null)
                throw new ArgumentNullException(nameof(assetType));

            if (!typeof(Asset).IsAssignableFrom(assetType))
                throw new ArgumentException("Type [{0}] must be assignable to Asset".ToFormat(assetType), nameof(assetType));
        }
    }
}
