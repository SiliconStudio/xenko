// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Shaders.Ast.Hlsl
{
    /// <summary>
    /// A Storage qualifier.
    /// </summary>
    public class StorageQualifier : Ast.StorageQualifier
    {
        #region Constants and Fields

        /// <summary>
        ///   Centroid modifier, only valid for structure field.
        /// </summary>
        public static readonly Ast.StorageQualifier Centroid = new Ast.StorageQualifier("centroid");

        /// <summary>
        ///   ColumnMajor modifier.
        /// </summary>
        public static readonly Ast.StorageQualifier ColumnMajor = new Ast.StorageQualifier("column_major");

        /// <summary>
        ///   Extern modifier.
        /// </summary>
        public static readonly Ast.StorageQualifier Extern = new Ast.StorageQualifier("extern");

        /// <summary>
        ///   Groupshared modifier.
        /// </summary>
        public static readonly Ast.StorageQualifier Groupshared = new Ast.StorageQualifier("groupshared");

        /// <summary>
        ///   Linear modifier, only valid for structure field.
        /// </summary>
        public static readonly Ast.StorageQualifier Linear = new Ast.StorageQualifier("linear");

        /// <summary>
        ///   NoPerspective modifier, only valid for structure field.
        /// </summary>
        public static readonly Ast.StorageQualifier NoPerspective = new Ast.StorageQualifier("noperspective");

        /// <summary>
        ///   Nointerpolation modifier.
        /// </summary>
        public static readonly Ast.StorageQualifier Nointerpolation = new Ast.StorageQualifier("nointerpolation");

        /// <summary>
        ///   Precise modifier.
        /// </summary>
        public static readonly Ast.StorageQualifier Precise = new Ast.StorageQualifier("precise");

        /// <summary>
        ///   RowMajor modifier.
        /// </summary>
        public static readonly Ast.StorageQualifier RowMajor = new Ast.StorageQualifier("row_major");

        /// <summary>
        ///   Sample modifier, only valid for structure field.
        /// </summary>
        public static readonly Ast.StorageQualifier Sample = new Ast.StorageQualifier("sample");

        /// <summary>
        ///   Shared modifier.
        /// </summary>
        public static readonly Ast.StorageQualifier Shared = new Ast.StorageQualifier("shared");

        /// <summary>
        ///   Static modifier.
        /// </summary>
        public static readonly Ast.StorageQualifier Static = new Ast.StorageQualifier("static");

        /// <summary>
        ///   Inline modifier.
        /// </summary>
        public static readonly Ast.StorageQualifier Inline = new Ast.StorageQualifier("inline");

        /// <summary>
        ///   Unsigned modifier.
        /// </summary>
        public static readonly Ast.StorageQualifier Unsigned = new Ast.StorageQualifier("unsigned");

        /// <summary>
        ///   Volatile modifier.
        /// </summary>
        public static readonly Ast.StorageQualifier Volatile = new Ast.StorageQualifier("volatile");

        /// <summary>
        ///   Internal map used for parsing.
        /// </summary>
        private static readonly StringEnumMap Map = PrepareParsing<StorageQualifier>();

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageQualifier"/> class.
        /// </summary>
        public StorageQualifier()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageQualifier"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        public StorageQualifier(object key)
            : base(key)
        {
        }

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
        public static new Qualifier Parse(string enumName)
        {
            return Map.ParseEnumFromName<Qualifier>(enumName);
        }

        #endregion
    }
}