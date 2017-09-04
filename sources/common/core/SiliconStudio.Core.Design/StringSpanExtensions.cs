// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Core
{
    public static class StringSpanExtensions
    {
        /// <summary>
        /// Gets the substring with the specified span. If the span is invalid, return null.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="span">The span.</param>
        /// <returns>A substring with the specified span or null if span is empty.</returns>
        [CanBeNull]
        public static string Substring(this string str, StringSpan span)
        {
            return span.IsValid ? str.Substring(span.Start, span.Length) : null;
        }
    }
}
