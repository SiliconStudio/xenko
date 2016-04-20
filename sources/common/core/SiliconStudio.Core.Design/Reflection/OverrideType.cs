// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Core.Reflection
{
    /// <summary>
    /// A Type of override used on a member value.
    /// </summary>
    [Flags]
    public enum OverrideType
    {
        /// <summary>
        /// The value is taken from a base value or this instance if no base (default).
        /// </summary>
        Base = 0,  // This is strictly not a correct value for a flag, but it is used to make sure default value is always base. When testing for this value, better use IsBase() extension method.

        /// <summary>
        /// The value is new and overridden locally. Base value is ignored.
        /// </summary>
        New = 1,

        /// <summary>
        /// The value is sealed and cannot be changed by inherited instances.
        /// </summary>
        Sealed = 2,
    }

    /// <summary>
    /// This class is holding the PropertyKey using to store <see cref="OverrideType"/> per object into the <see cref="ShadowObject"/>.
    /// </summary>
    public static partial class Override
    {
        internal const char PostFixSealed = '!';

        internal const char PostFixNew = '*';

        internal const string PostFixNewSealed = "*!";

        internal const string PostFixNewSealedAlt = "!*";

        internal const string PostFixSealedText = "!";

        internal const string PostFixNewText = "*";

        /// <summary>
        /// Determines whether the specified type is sealed.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if the specified type is sealed; otherwise, <c>false</c>.</returns>
        public static bool IsSealed(this OverrideType type)
        {
            return (type & OverrideType.Sealed) != 0;
        }

        /// <summary>
        /// Determines whether the specified type is base.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if the specified type is base; otherwise, <c>false</c>.</returns>
        public static bool IsBase(this OverrideType type)
        {
            return type == OverrideType.Base || type == OverrideType.Sealed;
        }

        /// <summary>
        /// Determines whether the specified type is new.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if the specified type is new; otherwise, <c>false</c>.</returns>
        public static bool IsNew(this OverrideType type)
        {
            return (type & OverrideType.New) != 0;
        }

        public static string ToText(this OverrideType type)
        {
            if (type == OverrideType.New)
            {
                return Override.PostFixNewText;
            }
            if (type == OverrideType.Sealed)
            {
                return Override.PostFixSealedText;
            }
            if (type == (OverrideType.New | OverrideType.Sealed))
            {
                return Override.PostFixNewSealed;
            }
            return string.Empty;
        }
    }
}