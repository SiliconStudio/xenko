// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Shaders.Ast.Hlsl;

namespace SiliconStudio.Paradox.Shaders.Parser.Ast
{
    public class ParadoxStorageQualifier : StorageQualifier
    {
        /// <summary>
        ///   Stream keyword (stream).
        /// </summary>
        public static readonly SiliconStudio.Shaders.Ast.StorageQualifier Stream = new ParadoxStorageQualifier("stream");

        /// <summary>
        ///   Patch stream keyword (patchstream).
        /// </summary>
        public static readonly SiliconStudio.Shaders.Ast.StorageQualifier PatchStream = new ParadoxStorageQualifier("patchstream");

        /// <summary>
        ///   Stage keyword (stage).
        /// </summary>
        public static readonly SiliconStudio.Shaders.Ast.StorageQualifier Stage = new ParadoxStorageQualifier("stage");

        /// <summary>
        ///   Clone keyword (clone).
        /// </summary>
        public static readonly SiliconStudio.Shaders.Ast.StorageQualifier Clone = new ParadoxStorageQualifier("clone");

        /// <summary>
        ///   Override keyword (override).
        /// </summary>
        public static readonly SiliconStudio.Shaders.Ast.StorageQualifier Override = new ParadoxStorageQualifier("override");

        /// <summary>
        ///   Override keyword (override).
        /// </summary>
        public static readonly SiliconStudio.Shaders.Ast.StorageQualifier Abstract = new ParadoxStorageQualifier("abstract");

        /// <summary>
        ///   Compose keyword (compose).
        /// </summary>
        public static readonly SiliconStudio.Shaders.Ast.StorageQualifier Compose = new ParadoxStorageQualifier("compose");

        /// <summary>
        ///   Internal keyword (internal).
        /// </summary>
        public static readonly SiliconStudio.Shaders.Ast.StorageQualifier Internal = new ParadoxStorageQualifier("internal");

        /// <summary>
        ///   Internal map used for parsing.
        /// </summary>
        private static readonly StringEnumMap Map = PrepareParsing<ParadoxStorageQualifier>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ParadoxStorageQualifier"/> class.
        /// </summary>
        public ParadoxStorageQualifier()
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ParadoxStorageQualifier"/> class.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        public ParadoxStorageQualifier(string key)
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
        public static ParadoxStorageQualifier operator &(ParadoxStorageQualifier left, ParadoxStorageQualifier right)
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
        public static ParadoxStorageQualifier operator |(ParadoxStorageQualifier left, ParadoxStorageQualifier right)
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
        public static ParadoxStorageQualifier operator ^(ParadoxStorageQualifier left, ParadoxStorageQualifier right)
        {
            return OperatorXor(left, right);
        }

        #endregion
    }
}