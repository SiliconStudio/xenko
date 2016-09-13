// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

// ReSharper disable once CheckNamespace
namespace SiliconStudio
{
    public static class Utilities
    {
        private const string RegexReservedCharacters = @"[ \-;',+*|!`~@#$%^&\?()=[\]{}<>\""]";

        /// <summary>
        /// Build a valid C# class name from the provided string. 
        /// It replaces all the forbidden characters by the provided replacement character.
        /// </summary>
        /// <param name="originalName">The original name</param>
        /// <param name="replacementCharacter">The replacement character</param>
        /// <returns></returns>
        public static string BuildValidClassName(string originalName, char replacementCharacter = '_')
        {
            return BuildValidClassName(originalName, null, replacementCharacter);
        }

        /// <summary>
        /// Build a valid C# class name from the provided string. 
        /// It replaces all the forbidden characters by the provided replacement character.
        /// </summary>
        /// <param name="originalName">The original name</param>
        /// <param name="additionalReservedWords">Reserved words that must be escaped if used directly</param>
        /// <param name="replacementCharacter">The replacement character</param>
        /// <returns></returns>
        public static string BuildValidClassName(string originalName, IEnumerable<string> additionalReservedWords, char replacementCharacter = '_')
        {
            if (ReservedNames.Contains(originalName))
                return originalName + replacementCharacter;

            if (additionalReservedWords != null && additionalReservedWords.Contains(originalName))
                return originalName + replacementCharacter;

            return Regex.Replace(originalName, $"{RegexReservedCharacters}|[.]", replacementCharacter.ToString());
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
            return BuildValidNamespaceName(originalName, null, replacementCharacter);
        }

        /// <summary>
        /// Build a valid C# namespace name from the provided string. 
        /// It replaces all the forbidden characters by the provided replacement character.
        /// </summary>
        /// <param name="originalName">The original name</param>
        /// <param name="additionalReservedWords">Reserved words that must be escaped if used directly</param>
        /// <param name="replacementCharacter">The replacement character</param>
        /// <returns></returns>
        public static string BuildValidNamespaceName(string originalName, IEnumerable<string> additionalReservedWords, char replacementCharacter = '_')
        {
            if (ReservedNames.Contains(originalName))
                return originalName + replacementCharacter;

            if (additionalReservedWords != null && additionalReservedWords.Contains(originalName))
                return originalName + replacementCharacter;

            return Regex.Replace(originalName, $"{RegexReservedCharacters}|[.](?=[0-9])", replacementCharacter.ToString());
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

        private static readonly string[] ReservedNames =
        {
            "abstract",
            "as",
            "base",
            "bool",
            "break",
            "byte",
            "case",
            "catch",
            "char",
            "checked",
            "class",
            "const",
            "continue",
            "decimal",
            "default",
            "delegate",
            "do",
            "double",
            "else",
            "enum",
            "event",
            "explicit",
            "extern",
            "false",
            "finally",
            "fixed",
            "float",
            "for",
            "foreach",
            "goto",
            "if",
            "implicit",
            "in",
            "int",
            "interface",
            "internal",
            "is",
            "lock",
            "long",
            "namespace",
            "new",
            "null",
            "object",
            "operator",
            "out",
            "override",
            "params",
            "private",
            "protected",
            "public",
            "readonly",
            "ref",
            "return",
            "sbyte",
            "sealed",
            "short",
            "sizeof",
            "stackalloc",
            "static",
            "string",
            "struct",
            "switch",
            "this",
            "throw",
            "true",
            "try",
            "typeof",
            "uint",
            "ulong",
            "unchecked",
            "unsafe",
            "ushort",
            "using",
            "virtual",
            "void",
            "volatile",
            "while",
        };
    }
}
