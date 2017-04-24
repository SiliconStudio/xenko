// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

namespace SiliconStudio.Shaders.Ast.Hlsl
{
    /// <summary>
    /// A Storage qualifier.
    /// </summary>
    public static class StorageQualifier
    {
        #region Constants and Fields

        /// <summary>
        ///   Centroid modifier, only valid for structure field.
        /// </summary>
        public static readonly Qualifier Centroid = new Qualifier("centroid");

        /// <summary>
        ///   ColumnMajor modifier.
        /// </summary>
        public static readonly Qualifier ColumnMajor = new Qualifier("column_major");

        /// <summary>
        ///   Extern modifier.
        /// </summary>
        public static readonly Qualifier Extern = new Qualifier("extern");

        /// <summary>
        ///   Groupshared modifier.
        /// </summary>
        public static readonly Qualifier Groupshared = new Qualifier("groupshared");

        /// <summary>
        ///   Linear modifier, only valid for structure field.
        /// </summary>
        public static readonly Qualifier Linear = new Qualifier("linear");

        /// <summary>
        ///   NoPerspective modifier, only valid for structure field.
        /// </summary>
        public static readonly Qualifier NoPerspective = new Qualifier("noperspective");

        /// <summary>
        ///   Nointerpolation modifier.
        /// </summary>
        public static readonly Qualifier Nointerpolation = new Qualifier("nointerpolation");

        /// <summary>
        ///   Precise modifier.
        /// </summary>
        public static readonly Qualifier Precise = new Qualifier("precise");

        /// <summary>
        ///   RowMajor modifier.
        /// </summary>
        public static readonly Qualifier RowMajor = new Qualifier("row_major");

        /// <summary>
        ///   Sample modifier, only valid for structure field.
        /// </summary>
        public static readonly Qualifier Sample = new Qualifier("sample");

        /// <summary>
        ///   Shared modifier.
        /// </summary>
        public static readonly Qualifier Shared = new Qualifier("shared");

        /// <summary>
        ///   Static modifier.
        /// </summary>
        public static readonly Qualifier Static = new Qualifier("static");

        /// <summary>
        ///   Inline modifier.
        /// </summary>
        public static readonly Qualifier Inline = new Qualifier("inline");

        /// <summary>
        ///   Unsigned modifier.
        /// </summary>
        public static readonly Qualifier Unsigned = new Qualifier("unsigned");

        /// <summary>
        ///   Volatile modifier.
        /// </summary>
        public static readonly Qualifier Volatile = new Qualifier("volatile");

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
        public static Qualifier Parse(string enumName)
        {
            if (enumName == (string)Centroid.Key)
                return Centroid;
            if (enumName == (string)ColumnMajor.Key)
                return ColumnMajor;
            if (enumName == (string)Extern.Key)
                return Extern;
            if (enumName == (string)Groupshared.Key)
                return Groupshared;
            if (enumName == (string)Linear.Key)
                return Linear;
            if (enumName == (string)NoPerspective.Key)
                return NoPerspective;
            if (enumName == (string)Nointerpolation.Key)
                return Nointerpolation;
            if (enumName == (string)Precise.Key)
                return Precise;
            if (enumName == (string)Precise.Key)
                return Precise;
            if (enumName == (string)RowMajor.Key)
                return RowMajor;
            if (enumName == (string)Sample.Key)
                return Sample;
            if (enumName == (string)Shared.Key)
                return Shared;
            if (enumName == (string)Static.Key)
                return Static;
            if (enumName == (string)Inline.Key)
                return Inline;
            if (enumName == (string)Unsigned.Key)
                return Unsigned;
            if (enumName == (string)Volatile.Key)
                return Volatile;

            // Fallback to shared parameter qualifiers
            return Ast.StorageQualifier.Parse(enumName);
        }

        #endregion
    }
}
