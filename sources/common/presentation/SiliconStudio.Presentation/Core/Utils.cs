// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Text.RegularExpressions;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Presentation.Core
{
    /// <summary>
    /// A static class containing various useful methods and constants.
    /// </summary>
    public static class Utils
    {   
        /// <summary>
        /// An array of values that can be used for zooming.
        /// </summary>
        public static readonly double[] ZoomFactors = { 0.02, 0.05, 0.083, 0.125, 0.167, 0.20, 0.25, 0.333, 0.5, 0.667, 1.0, 1.5, 2.0, 3.0, 4.0, 5.0, 6.0, 8.0, 12.0, 16.0, 24.0 };

        /// <summary>
        /// The index of the factor <c>1.0</c> in the <see cref="ZoomFactors"/> array.
        /// </summary>
        public static readonly int ZoomFactorIdentityIndex = 10;

        [NotNull]
        public static string SplitCamelCase([NotNull] string input)
        {
            return Regex.Replace(input, "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))", "$1 ");
        }
    }
}
