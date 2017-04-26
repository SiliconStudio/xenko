// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Shaders.Ast;
using SiliconStudio.Shaders.Grammar.Hlsl;

// Use XenkoGrammar (compatiable with HLSL), in order to avoid initializing both Xenko and HLSL grammar
using HlslGrammar = SiliconStudio.Shaders.Grammar.Xenko.XenkoGrammar;

namespace SiliconStudio.Shaders.Parser.Hlsl
{
    /// <summary>
    /// HlslParser.
    /// </summary>
    public class HlslParser
    {
        /// <summary>
        /// Initializes this instance.
        /// </summary>
        static HlslParser()
        {
            // Call get parser to force an initialization
            ShaderParser.GetParser<HlslGrammar>();
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public static void Initialize()
        {
            // Call get parser to force an initialization
            ShaderParser.GetParser<HlslGrammar>();
        }

        /// <summary>
        /// Parses the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="sourceFileName">Name of the source file.</param>
        /// <param name="macros">The macros defined for the preprocessor.</param>
        /// <param name="includeDirectories">The include directories used by the preprocessor..</param>
        /// <returns>Result of parsing</returns>
        public static ParsingResult TryPreProcessAndParse(string source, string sourceFileName, ShaderMacro[] macros = null, params string[] includeDirectories)
        {
            return ShaderParser.GetParser<HlslGrammar>().TryPreProcessAndParse(source, sourceFileName, macros, includeDirectories);
        }

        /// <summary>
        /// Parses the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="sourceFileName">Name of the source file.</param>
        /// <param name="macros">The macros defined for the preprocessor.</param>
        /// <param name="includeDirectories">The include directories used by the preprocessor..</param>
        /// <returns>Result of parsing</returns>
        public static Shader Parse(string source, string sourceFileName, ShaderMacro[] macros = null, params string[] includeDirectories)
        {
            return ShaderParser.GetParser<HlslGrammar>().PreProcessAndParse(source, sourceFileName, macros, includeDirectories);
        }
    }
}
