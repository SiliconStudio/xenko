// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Shaders.Ast
{
    /// <summary>
    /// A Layout qualifier.
    /// </summary>
    public class LayoutQualifier : Qualifier
    {
        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "LayoutQualifier" /> class.
        /// </summary>
        public LayoutQualifier()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LayoutQualifier"/> class.
        /// </summary>
        /// <param name="key">
        /// Name of the enum.
        /// </param>
        public LayoutQualifier(object key) : base(key)
        {
        }

        #endregion

        #region Operators

        /// <summary>
        ///   Implements the operator ==.
        /// </summary>
        /// <param name = "left">The left.</param>
        /// <param name = "right">The right.</param>
        /// <returns>
        ///   The result of the operator.
        /// </returns>
        public static LayoutQualifier operator &(LayoutQualifier left, LayoutQualifier right)
        {
            return OperatorAnd(left, right);
        }

        /// <summary>
        ///   Implements the operator |.
        /// </summary>
        /// <param name = "left">The left.</param>
        /// <param name = "right">The right.</param>
        /// <returns>
        ///   The result of the operator.
        /// </returns>
        public static LayoutQualifier operator |(LayoutQualifier left, LayoutQualifier right)
        {
            return OperatorOr(left, right);
        }

        /// <summary>
        ///   Implements the operator ^.
        /// </summary>
        /// <param name = "left">The left.</param>
        /// <param name = "right">The right.</param>
        /// <returns>
        ///   The result of the operator.
        /// </returns>
        public static LayoutQualifier operator ^(LayoutQualifier left, LayoutQualifier right)
        {
            return OperatorXor(left, right);
        }

        #endregion
    }
}