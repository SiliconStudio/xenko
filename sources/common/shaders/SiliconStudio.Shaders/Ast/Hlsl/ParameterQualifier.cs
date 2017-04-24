// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

namespace SiliconStudio.Shaders.Ast.Hlsl
{
    /// <summary>
    /// Specialized ParameterQualifier for Hlsl.
    /// </summary>
    public static class ParameterQualifier
    {

        /// <summary>
        ///   Point modifier, only for method parameters in Geometry Shader.
        /// </summary>
        public static readonly Qualifier Point = new Qualifier("point");

        /// <summary>
        ///   Line modifier, only for method parameters in Geometry Shader.
        /// </summary>
        public static readonly Qualifier Line = new Qualifier("line");

        /// <summary>
        ///   LineAdjacent modifier, only for method parameters in Geometry Shader.
        /// </summary>
        public static readonly Qualifier LineAdj = new Qualifier("lineadj");

        /// <summary>
        ///   Triangle modifier, only for method parameters in Geometry Shader.
        /// </summary>
        public static readonly Qualifier Triangle = new Qualifier("triangle");

        /// <summary>
        ///   TriangleAdjacent modifier, only for method parameters in Geometry Shader.
        /// </summary>
        public static readonly Qualifier TriangleAdj = new Qualifier("triangleadj");

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
            if (enumName == (string)Point.Key)
                return Point;
            if (enumName == (string)Line.Key)
                return Line;
            if (enumName == (string)LineAdj.Key)
                return LineAdj;
            if (enumName == (string)Triangle.Key)
                return Triangle;
            if (enumName == (string)TriangleAdj.Key)
                return TriangleAdj;

            // Fallback to shared parameter qualifiers
            return Ast.ParameterQualifier.Parse(enumName);
        }
    }
}
