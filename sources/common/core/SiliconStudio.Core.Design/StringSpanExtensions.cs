// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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
        public static string Substring(this string str, StringSpan span)
        {
            return span.IsValid ? str.Substring(span.Start, span.Length) : null;
        } 
    }
}