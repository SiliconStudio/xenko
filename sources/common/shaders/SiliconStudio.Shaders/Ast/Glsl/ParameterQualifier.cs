// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Shaders.Ast.Glsl
{
    /// <summary>
    /// Specialized ParameterQualifier for Hlsl.
    /// </summary>
    public static class ParameterQualifier
    {
        /// <summary>
        ///   Varying modifier, only for OpenGL ES 2.0.
        /// </summary>
        public static readonly Qualifier Varying = new Qualifier("varying");

        /// <summary>
        ///   Attribute modifier, only for OpenGL ES 2.0.
        /// </summary>
        public static readonly Qualifier Attribute = new Qualifier("attribute");

        /// <summary>
        /// Parses the specified enum name.
        /// </summary>
        /// <param name="enumName">
        /// Name of the enum.
        /// </param>
        /// <returns>
        /// A parameter qualifier
        /// </returns>
        public static Qualifier Parse(string enumName)
        {
            if (enumName == (string)Varying.Key)
                return Varying;
            if (enumName == (string)Attribute.Key)
                return Attribute;

            // Fallback to shared parameter qualifiers
            return Ast.ParameterQualifier.Parse(enumName);
        }
    }
}
