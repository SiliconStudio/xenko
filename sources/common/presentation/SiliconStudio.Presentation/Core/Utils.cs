// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SiliconStudio.Presentation.Core
{
    /// <summary>
    /// A static class containing various useful methods and constants.
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// The default name pattern used by the function <see cref="ComputeNewName{T}"/> when no pattern is provided in the function call.
        /// </summary>
        public const string DefaultNamePattern = "{0} ({1})";
        
        /// <summary>
        /// An array of values that can be used for zooming.
        /// </summary>
        public static readonly double[] ZoomFactors = { 0.02, 0.05, 0.083, 0.125, 0.167, 0.20, 0.25, 0.333, 0.5, 0.667, 1.0, 1.5, 2.0, 3.0, 4.0, 5.0, 6.0, 8.0, 12.0, 16.0, 24.0 };

        /// <summary>
        /// The index of the factor <c>1.0</c> in the <see cref="ZoomFactors"/> array.
        /// </summary>
        public static readonly int ZoomFactorIdentityIndex = 10;
         
        /// <summary>
        /// Updates the given field to the given value. If the field changes, invoke the given action.
        /// </summary>
        /// <typeparam name="T">The type of the field and the value.</typeparam>
        /// <param name="field">The field to update.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="action">The action to invoke if the field has changed.</param>
        public static void SetAndInvokeIfChanged<T>(ref T field, T value, Action action)
        {
            if (action == null) throw new ArgumentNullException("action");
            bool changed = !Equals(field, value);
            if (changed)
            {
                field = value;
                action();
            }
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
            if (baseName == null) throw new ArgumentNullException("baseName");
            if (existingItems == null) throw new ArgumentNullException("existingItems");
            if (namePattern == null) namePattern = DefaultNamePattern;
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

        public static string SplitCamelCase(string input)
        {
            return Regex.Replace(input, "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))", "$1 ");
        }
    }
}
