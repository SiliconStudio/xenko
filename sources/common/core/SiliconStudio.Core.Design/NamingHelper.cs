// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Generate a name for a new object that is guaranteed to be unique among a collection of existing objects. To generate such name, a base name and a pattern for variations must be provided.
        /// </summary>
        /// <typeparam name="T">The type of object in the collection of existing object.</typeparam>
        /// <param name="baseName">The base name used to generate the new name. If the name is available in the collection, it will be returned as-is. Otherwise, a name following the given pattern will be returned.</param>
        /// <param name="existingItems">The collection of existing items, used to verify that the name being generated is not already used.</param>
        /// <param name="existingNameFunc">A function used to extract the name of an object of the given collection. If null, the <see cref="object.ToString"/> method will be used.</param>
        /// <param name="namePattern">The pattern used to generate the new name, when the base name is unavailable. This pattern must contains the token '{0}' that will be replaced by the base name, and the token '{1}' that will be replaced by the smallest numerical value that can generate an available name, starting from 2. If null, <see cref="DefaultNamePattern"/> will be used instead.</param>
        /// <returns><see cref="baseName"/> if no item of <see cref="existingItems"/> returns this value through <see cref="existingNameFunc"/>. Otherwise, a string formatted with <see cref="namePattern"/>, using <see cref="baseName"/> as token '{0}' and the smallest numerical value that can generate an available name, starting from 2</returns>
        public static string ComputeNewName<T>(string baseName, ICollection<T> existingItems, Func<T, string> existingNameFunc = null, string namePattern = null)
        {
            const string defaultNamePattern = "{0} ({1})";
            if (baseName == null) throw new ArgumentNullException("baseName");
            if (existingItems == null) throw new ArgumentNullException("existingItems");
            if (namePattern == null) namePattern = defaultNamePattern;
            if (!namePattern.Contains("{0}") || !namePattern.Contains("{1}")) throw new ArgumentException(@"This parameter must be a formattable string containing '{0}' and '{1}' tokens", "namePattern");
            if (existingNameFunc == null)
                existingNameFunc = x => x.ToString();

            string result = baseName;
            int counter = 1;
            while (existingItems.Select(existingNameFunc).Any(x => x == result))
            {
                result = string.Format(namePattern, baseName, ++counter);
            }
            return result;
        }

    }
}