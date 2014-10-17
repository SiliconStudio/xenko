// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Shaders.Ast
{
    /// <summary>
    /// A Storage qualifier.
    /// </summary>
    public class StorageQualifier : Qualifier
    {
        #region Constants and Fields

        /// <summary>
        ///   Const qualifier.
        /// </summary>
        public static readonly StorageQualifier Const = new StorageQualifier("const");

        /// <summary>
        ///   Uniform qualifier.
        /// </summary>
        public static readonly StorageQualifier Uniform = new StorageQualifier("uniform");

        /// <summary>
        ///   Internal map used for parsing.
        /// </summary>
        private static readonly StringEnumMap Map = PrepareParsing<StorageQualifier>();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "StorageQualifier" /> class.
        /// </summary>
        public StorageQualifier()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageQualifier"/> class.
        /// </summary>
        /// <param name="key">
        /// Name of the enum.
        /// </param>
        public StorageQualifier(object key) : base(key)
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
        /// A storage qualifier
        /// </returns>
        public static StorageQualifier Parse(string enumName)
        {
            return Map.ParseEnumFromName<StorageQualifier>(enumName);
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
        public static StorageQualifier operator &(StorageQualifier left, StorageQualifier right)
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
        public static StorageQualifier operator |(StorageQualifier left, StorageQualifier right)
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
        public static StorageQualifier operator ^(StorageQualifier left, StorageQualifier right)
        {
            return OperatorXor(left, right);
        }

        #endregion
    }
}