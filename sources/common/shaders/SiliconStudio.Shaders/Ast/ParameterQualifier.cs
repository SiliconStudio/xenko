// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Shaders.Ast
{
    /// <summary>
    /// A Storage qualifier.
    /// </summary>
    public class ParameterQualifier : Qualifier
    {
        #region Constants and Fields

        /// <summary>
        ///   In modifier, only for method parameters.
        /// </summary>
        public static readonly ParameterQualifier In = new ParameterQualifier("in");

        /// <summary>
        ///   InOut Modifier, only for method parameters.
        /// </summary>
        public static readonly ParameterQualifier InOut = new ParameterQualifier("inout");

        /// <summary>
        ///   Out modifier, only for method parameters.
        /// </summary>
        public static readonly ParameterQualifier Out = new ParameterQualifier("out");

        /// <summary>
        ///   Internal map used for parsing.
        /// </summary>
        private static readonly StringEnumMap Map = PrepareParsing<ParameterQualifier>();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "ParameterQualifier" /> class.
        /// </summary>
        public ParameterQualifier()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterQualifier"/> class.
        /// </summary>
        /// <param name="key">
        /// Name of the enum.
        /// </param>
        public ParameterQualifier(string key)
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
        /// A parameter qualifier
        /// </returns>
        public static ParameterQualifier Parse(string enumName)
        {
            return Map.ParseEnumFromName<ParameterQualifier>(enumName);
        }

        #endregion
    }
}