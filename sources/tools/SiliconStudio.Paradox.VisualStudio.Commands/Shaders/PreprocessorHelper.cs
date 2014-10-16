// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SiliconStudio.Shaders;
using SiliconStudio.Shaders.Parser;

namespace SiliconStudio.Paradox.VisualStudio.Commands.Shaders
{
    public class PreprocessorHelper
    {
        private static Regex concatenateTokensRegex = new Regex(@"(\w+)?\s*#(#)?\s*(\w+)");

        private static string[] preprocessorKeywords = new[] { "if", "else", "elif", "endif", "define", "undef", "ifdef", "ifndef", "line", "error", "pragma", "include" };

        private static string TransformToken(string token, ShaderMacro[] macros = null, bool emptyIfNotFound = false)
        {
            if (macros == null) return token;

            foreach (var macro in macros)
            {
                if (macro.Name == token) return macro.Definition;
            }

            return emptyIfNotFound ? string.Empty : token;
        }

        private static string EscapeString(string s)
        {
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private static string ConcatenateTokens(string source, ShaderMacro[] macros = null)
        {
            var stringBuilder = new StringBuilder(source.Length);
            int position = 0;

            // Process every A ## B ## C ## ... patterns
            // Find first match
            var match = concatenateTokensRegex.Match(source, position);

            // Early exit
            if (!match.Success) return source;

            while (match.Success)
            {
                // Add what was before regex
                stringBuilder.Append(source, position, match.Index - position);

                // Check if # (stringify) or ## (concat)
                bool stringify = !match.Groups[2].Success;

                var token = match.Groups[3].Value;
                if (stringify && preprocessorKeywords.Contains(token))
                {
                    // Ignore some special preprocessor tokens
                    stringBuilder.Append(match.Groups[0].Value);
                }
                else
                {
                    // Expand and add first macro
                    stringBuilder.Append(TransformToken(match.Groups[1].Value, macros));


                    if (stringify) // stringification
                    {
                        stringBuilder.Append('"');
                        // TODO: Escape string
                        stringBuilder.Append(EscapeString(TransformToken(token, macros, true)));
                        stringBuilder.Append('"');
                    }
                    else // concatenation
                    {
                        stringBuilder.Append(TransformToken(match.Groups[3].Value, macros));
                    }
                }

                // Find next match
                position = match.Groups[3].Index + match.Groups[3].Length;
                match = concatenateTokensRegex.Match(source, position);
            }

            // Add what is after regex
            stringBuilder.Append(source, position, source.Length - position);

            return stringBuilder.ToString();
        }

        public static string Preprocess(string shaderSource, string filename, ShaderMacro[] macros)
        {
            // Preprocess
            // First, perform token concatenation (not supported by D3DX)
            // Check for either TOKEN ## or ## TOKEN
            var preprocessedSource = ConcatenateTokens(shaderSource, macros);
            return PreProcessor.Run(preprocessedSource, filename, macros);
        }
    }
}