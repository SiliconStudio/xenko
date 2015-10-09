// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Shaders.Ast.Hlsl;

namespace SiliconStudio.Xenko.Shaders.Parser.Ast
{
    public class XenkoStorageQualifier : StorageQualifier
    {
        /// <summary>
        ///   Stream keyword (stream).
        /// </summary>
        public static readonly SiliconStudio.Shaders.Ast.StorageQualifier Stream = new XenkoStorageQualifier("stream");

        /// <summary>
        ///   Patch stream keyword (patchstream).
        /// </summary>
        public static readonly SiliconStudio.Shaders.Ast.StorageQualifier PatchStream = new XenkoStorageQualifier("patchstream");

        /// <summary>
        ///   Stage keyword (stage).
        /// </summary>
        public static readonly SiliconStudio.Shaders.Ast.StorageQualifier Stage = new XenkoStorageQualifier("stage");

        /// <summary>
        ///   Clone keyword (clone).
        /// </summary>
        public static readonly SiliconStudio.Shaders.Ast.StorageQualifier Clone = new XenkoStorageQualifier("clone");

        /// <summary>
        ///   Override keyword (override).
        /// </summary>
        public static readonly SiliconStudio.Shaders.Ast.StorageQualifier Override = new XenkoStorageQualifier("override");

        /// <summary>
        ///   Override keyword (override).
        /// </summary>
        public static readonly SiliconStudio.Shaders.Ast.StorageQualifier Abstract = new XenkoStorageQualifier("abstract");

        /// <summary>
        ///   Compose keyword (compose).
        /// </summary>
        public static readonly SiliconStudio.Shaders.Ast.StorageQualifier Compose = new XenkoStorageQualifier("compose");

        /// <summary>
        ///   Internal keyword (internal).
        /// </summary>
        public static readonly SiliconStudio.Shaders.Ast.StorageQualifier Internal = new XenkoStorageQualifier("internal");

        /// <summary>
        ///   Internal map used for parsing.
        /// </summary>
        private static readonly StringEnumMap Map = PrepareParsing<XenkoStorageQualifier>();

        /// <summary>
        /// Initializes a new instance of the <see cref="XenkoStorageQualifier"/> class.
        /// </summary>
        public XenkoStorageQualifier()
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="XenkoStorageQualifier"/> class.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        public XenkoStorageQualifier(string key)
            : base(key)
        {
        }

        /// <summary>
        /// Parses the specified enum name.
        /// </summary>
        /// <param name="enumName">
        /// Name of the enum.
        /// </param>
        /// <returns>
        /// A qualifier
        /// </returns>
        public static new SiliconStudio.Shaders.Ast.StorageQualifier Parse(string enumName)
        {
            return Map.ParseEnumFromName<SiliconStudio.Shaders.Ast.StorageQualifier>(enumName);
        }
        
        #region Operators

        /// <summary>
        ///   Implements the operator ==.
        /// </summary>
        /// <param name = "left">The left.</param>
        /// <param name = "right">The right.</param>
        /// <returns>
        ///   The result of the operator.
        /// </returns>
        public static XenkoStorageQualifier operator &(XenkoStorageQualifier left, XenkoStorageQualifier right)
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
        public static XenkoStorageQualifier operator |(XenkoStorageQualifier left, XenkoStorageQualifier right)
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
        public static XenkoStorageQualifier operator ^(XenkoStorageQualifier left, XenkoStorageQualifier right)
        {
            return OperatorXor(left, right);
        }

        #endregion
    }
}