// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Shaders.Ast.Hlsl
{
    /// <summary>
    /// Value range for a float
    /// </summary>
    public class FloatQualifier : Qualifier
    {
        #region Constants and Fields

        /// <summary>
        ///   IEEE 32-bit signed-normalized float in range -1 to 1 inclusive.
        /// </summary>
        public static readonly Qualifier SNorm = new FloatQualifier("snorm");

        /// <summary>
        ///   IEEE 32-bit unsigned-normalized float in range 0 to 1 inclusive.
        /// </summary>
        public static readonly Qualifier UNorm = new FloatQualifier("unorm");

        /// <summary>
        ///   Internal map used for parsing.
        /// </summary>
        private static readonly StringEnumMap Map = PrepareParsing<FloatQualifier>();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "FloatQualifier" /> class.
        /// </summary>
        public FloatQualifier()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FloatQualifier"/> class.
        /// </summary>
        /// <param name="key">
        /// Name of the enum.
        /// </param>
        public FloatQualifier(string key)
            : base(key)
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Parses the specified enum name.
        /// </summary>
        /// <param name="enumName">
        /// Name of the enum.
        /// </param>
        /// <returns>
        /// A qualifier
        /// </returns>
        public static FloatQualifier Parse(string enumName)
        {
            return Map.ParseEnumFromName<FloatQualifier>(enumName);
        }

        #endregion
    }
}