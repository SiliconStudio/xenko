// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Shaders.Ast.Hlsl
{
    /// <summary>
    /// Specialized ParameterQualifier for Hlsl.
    /// </summary>
    public class ParameterQualifier : Ast.ParameterQualifier
    {

        /// <summary>
        ///   Point modifier, only for method parameters in Geometry Shader.
        /// </summary>
        public static readonly Ast.ParameterQualifier Point = new Ast.ParameterQualifier("point");

        /// <summary>
        ///   Line modifier, only for method parameters in Geometry Shader.
        /// </summary>
        public static readonly Ast.ParameterQualifier Line = new Ast.ParameterQualifier("line");

        /// <summary>
        ///   LineAdjacent modifier, only for method parameters in Geometry Shader.
        /// </summary>
        public static readonly Ast.ParameterQualifier LineAdj = new Ast.ParameterQualifier("lineadj");

        /// <summary>
        ///   Triangle modifier, only for method parameters in Geometry Shader.
        /// </summary>
        public static readonly Ast.ParameterQualifier Triangle = new Ast.ParameterQualifier("triangle");

        /// <summary>
        ///   TriangleAdjacent modifier, only for method parameters in Geometry Shader.
        /// </summary>
        public static readonly Ast.ParameterQualifier TriangleAdj = new Ast.ParameterQualifier("triangleadj");

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
