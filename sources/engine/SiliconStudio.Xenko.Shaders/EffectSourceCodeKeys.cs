// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Paradox.Rendering;

namespace SiliconStudio.Paradox.Shaders
{
    /// <summary>
    /// Keys used for sourcecode generation.
    /// </summary>
    public static class EffectSourceCodeKeys
    {
        /// <summary>
        /// When compiling a pdxsl, this will generate a source code file
        /// </summary>
        public static readonly ParameterKey<bool> Enable = ParameterKeys.New<bool>();

        /// <summary>
        /// The class modifier declaration (Default: "public partial")
        /// </summary>
        public static readonly ParameterKey<string> ClassDeclaration = ParameterKeys.New("public partial");

        /// <summary>
        /// The namespace used for the declaration.
        /// </summary>
        public static readonly ParameterKey<string> Namespace = ParameterKeys.New<string>();

        /// <summary>
        /// The classname used for the (Default: name of the effect).
        /// </summary>
        public static readonly ParameterKey<string> ClassName = ParameterKeys.New<string>();

        /// <summary>
        /// The field declaration (default: "private")
        /// </summary>
        public static readonly ParameterKey<string> FieldDeclaration = ParameterKeys.New("private");

        /// <summary>
        /// The field name (default: "binaryBytecode")
        /// </summary>
        public static readonly ParameterKey<string> FieldName = ParameterKeys.New("binaryBytecode");
    }
}