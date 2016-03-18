// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Shaders
{
    /// <summary>
    /// Keys used for sourcecode generation.
    /// </summary>
    public static class EffectSourceCodeKeys
    {
        /// <summary>
        /// When compiling a xksl, this will generate a source code file
        /// </summary>
        public static readonly ObjectParameterKey<bool> Enable = ParameterKeys.NewObject<bool>();

        /// <summary>
        /// The class modifier declaration (Default: "public partial")
        /// </summary>
        public static readonly ObjectParameterKey<string> ClassDeclaration = ParameterKeys.NewObject("public partial");

        /// <summary>
        /// The namespace used for the declaration.
        /// </summary>
        public static readonly ObjectParameterKey<string> Namespace = ParameterKeys.NewObject<string>();

        /// <summary>
        /// The classname used for the (Default: name of the effect).
        /// </summary>
        public static readonly ObjectParameterKey<string> ClassName = ParameterKeys.NewObject<string>();

        /// <summary>
        /// The field declaration (default: "private")
        /// </summary>
        public static readonly ObjectParameterKey<string> FieldDeclaration = ParameterKeys.NewObject("private");

        /// <summary>
        /// The field name (default: "binaryBytecode")
        /// </summary>
        public static readonly ObjectParameterKey<string> FieldName = ParameterKeys.NewObject("binaryBytecode");
    }
}