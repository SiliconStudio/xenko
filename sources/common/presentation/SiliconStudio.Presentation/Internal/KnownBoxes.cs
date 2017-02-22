// Copyright (c) 2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Presentation.Internal
{
    internal static class KnownBoxes
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
        /// Returns an object representing the provided boolean <paramref name="value"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <returns><see cref="TrueBox"/> if the value is <c>true</c>; otherwise, <see cref="FalseBox"/>.</returns>
        internal static object Box(this bool value)
        {
            return value ? TrueBox : FalseBox;
        }
    }
}
