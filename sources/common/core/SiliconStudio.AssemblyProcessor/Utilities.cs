// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Text.RegularExpressions;

// ReSharper disable once CheckNamespace
namespace SiliconStudio
{
    public static class Utilities
    {
        /// <summary>
        /// Build a valid C# class name from the provided string. 
        /// It replaces all the forbidden characters by the provided replacement character.
        /// </summary>
        /// <param name="originalName">The original name</param>
        /// <param name="replacementCharacter">The replacement character</param>
        /// <returns></returns>
        public static string BuildValidClassName(string originalName, char replacementCharacter = '_')
        {
            return Regex.Replace(originalName, @"[ \-.;',+*|!`~@#$%^&()=[\]{}]", replacementCharacter.ToString());
        }

        /// <summary>
        /// Build a valid C# namespace name from the provided string. 
        /// It replaces all the forbidden characters by the provided replacement character.
        /// </summary>
        /// <param name="originalName">The original name</param>
        /// <param name="replacementCharacter">The replacement character</param>
        /// <returns></returns>
        public static string BuildValidNamespaceName(string originalName, char replacementCharacter = '_')
        {
            return Regex.Replace(originalName, @"[ \-;',+*|!`~@#$%^&()=[\]{}]|[.](?=[0-9])", replacementCharacter.ToString());
        }

        /// <summary>
        /// Build a valid C# project name from the provided string. 
        /// It replaces all the forbidden characters by the provided replacement character.
        /// </summary>
        /// <param name="originalName">The original name</param>
        /// <param name="replacementCharacter">The replacement character</param>
        /// <returns></returns>
        public static string BuildValidProjectName(string originalName, char replacementCharacter = '_')
        {
            return Regex.Replace(originalName, "[=;,/\\?:&*<>|#%\"]", replacementCharacter.ToString());
        }
    }
}
