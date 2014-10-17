// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace SiliconStudio.Core
{
    /// <summary>
    /// Helper to verify naming conventions.
    /// </summary>
    public static class NamingHelper
    {
        private static readonly Regex MatchIdentifier = new Regex("^[a-zA-Z_][a-zA-Z0-9_]*$");

        /// <summary>
        /// Determines whether the specified string is valid namespace identifier.
        /// </summary>
        /// <param name="text">The namespace text.</param>
        /// <returns><c>true</c> if is a valid namespace identifier; otherwise, <c>false</c>.</returns>
        public static bool IsValidNamespace(string text)
        {
            string error;
            return IsValidNamespace(text, out error);
        }

        /// <summary>
        /// Determines whether the specified string is valid namespace identifier.
        /// </summary>
        /// <param name="text">The namespace text.</param>
        /// <param name="error">The error if return is false.</param>
        /// <returns><c>true</c> if is a valid namespace identifier; otherwise, <c>false</c>.</returns>
        public static bool IsValidNamespace(string text, out string error)
        {
            if (text == null) throw new ArgumentNullException("text");

            if (string.IsNullOrWhiteSpace(text))
            {
                error = "Namespace cannot be empty";
            }
            else
            {
                var items = text.Split(new[] { '.' }, StringSplitOptions.None);
                error = items.Where(s => !IsIdentifier(s)).Select(item => string.Format("[{0}]", item, text)).FirstOrDefault();
            }
            return error == null;
        }

        /// <summary>
        /// Determines whether the specified text is a C# identifier.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns><c>true</c> if the specified text is an identifier; otherwise, <c>false</c>.</returns>
        /// <exception cref="System.ArgumentNullException">text</exception>
        public static bool IsIdentifier(string text)
        {
            if (text == null) throw new ArgumentNullException("text");
            return MatchIdentifier.Match(text).Success;
        }
    }
}