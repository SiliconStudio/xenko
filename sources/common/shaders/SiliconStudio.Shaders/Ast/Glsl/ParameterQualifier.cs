// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Shaders.Ast.Glsl
{
    /// <summary>
    /// Specialized ParameterQualifier for Hlsl.
    /// </summary>
    public class ParameterQualifier : Ast.ParameterQualifier
    {

        /// <summary>
        ///   Varying modifier, only for OpenGL ES 2.0.
        /// </summary>
        public static readonly Ast.ParameterQualifier Varying = new Ast.ParameterQualifier("varying");

        /// <summary>
        ///   Attribute modifier, only for OpenGL ES 2.0.
        /// </summary>
        public static readonly Ast.ParameterQualifier Attribute = new Ast.ParameterQualifier("attribute");

        /// <summary>
        ///   Internal map used for parsing.
        /// </summary>
        private static readonly StringEnumMap Map = PrepareParsing<ParameterQualifier>();

        /// <summary>
        /// Parses the specified enum name.
        /// </summary>
        /// <param name="enumName">
        /// Name of the enum.
        /// </param>
        /// <returns>
        /// A parameter qualifier
        /// </returns>
        public static new Ast.ParameterQualifier Parse(string enumName)
        {
            return Map.ParseEnumFromName<Ast.ParameterQualifier>(enumName);
        }

    }
}
