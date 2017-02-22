// Copyright (c) 2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Windows;

namespace SiliconStudio.Presentation.Internal
{
    internal static class BooleanBoxes
    {
        /// <summary>
        /// An object representing the value <c>false</c>.
        /// </summary>
        internal static readonly object FalseBox = false;
        /// <summary>
        /// An object representing the value <c>true</c>.
        /// </summary>
        internal static readonly object TrueBox = true;

        /// <summary>
        /// Returns an object representing the provided <see cref="bool"/> <paramref name="value"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>A boxed <see cref="bool"/> equivalent to the provided <paramref name="value"/>.</returns>
        internal static object Box(this bool value)
        {
            return value ? TrueBox : FalseBox;
        }
    }

    internal static class VisibilityBoxes
    {
        /// <summary>
        /// An object representing the value <see cref="Visibility.Visible"/>.
        /// </summary>
        internal static object VisibleBox = Visibility.Visible;
        /// <summary>
        /// An object representing the value <see cref="Visibility.Hidden"/>.
        /// </summary>
        internal static object HiddenBox = Visibility.Hidden;
        /// <summary>
        /// An object representing the value <see cref="Visibility.Collapsed"/>.
        /// </summary>
        internal static object CollapsedBox = Visibility.Collapsed;

        /// <summary>
        /// Returns an object representing the provided Visibility <paramref name="value"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>A boxed <see cref="Visibility"/> equivalent to the provided <paramref name="value"/>.</returns>
        internal static object Box(this Visibility value)
        {
            switch (value)
            {
                case Visibility.Visible:
                    return VisibleBox;
                case Visibility.Hidden:
                    return HiddenBox;
                case Visibility.Collapsed:
                    return CollapsedBox;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }
}
