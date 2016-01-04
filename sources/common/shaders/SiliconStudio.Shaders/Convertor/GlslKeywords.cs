// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using SiliconStudio.Shaders.Properties;

namespace SiliconStudio.Shaders.Convertor
{
    /// <summary>
    /// GlslKeywords
    /// </summary>
    public class GlslKeywords
    {
        /// <summary>
        /// Name of the default keywords.glsl file
        /// </summary>
        private const string KeywordsFileName = "keywords.glsl";

        /// <summary>
        /// Regex to remove pseudo C++ comments
        /// </summary>
        private static readonly Regex StripComments = new Regex("//.*");

        /// <summary>
        /// Regsitered tokens
        /// </summary>
        private static readonly HashSet<string> Tokens = new HashSet<string>();

        /// <summary>
        /// Initializes the <see cref="GlslKeywords"/> class by loading the keywords file.
        /// </summary>
        /// <remarks>
        /// It tries to load the "keywords.glsl" file from the same directory than this assembly (allowing overrides)
        /// else it loads it from an internal resource of this assembly.
        /// </remarks>
        static GlslKeywords()
        {
            Stream stream = null;
            try
            {
                // Try to load from the 
                var keywordFilePath = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(typeof (GlslKeywords).GetTypeInfo().Assembly.Location)), KeywordsFileName);
                if (File.Exists(keywordFilePath))
                    stream = new FileStream(keywordFilePath, FileMode.Open, FileAccess.Read);

                if (stream == null) stream = new MemoryStream(Resources.Keywords);

                InitializeFromStream(stream);
            } catch(Exception ex)
            {
                Console.WriteLine("Unable to load keywords.glsl file. Reason: " + ex);
            } finally
            {
                if (stream != null)
                    try { stream.Dispose(); } catch {}
            }
        }

        /// <summary>
        /// Initializes the tokens from a stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        private static void InitializeFromStream(Stream stream)
        {
            if (stream == null)
                return;

            var reader = new StreamReader(stream);
            string line;
            while ( (line = reader.ReadLine()) != null)
            {
                var newTokens = StripComments.Replace(line, "").Trim().Split((string[])null, StringSplitOptions.RemoveEmptyEntries);
                foreach (var newToken in newTokens)
                    Tokens.Add(newToken);
            }
        }

        /// <summary>
        /// Determines whether the specified identifier is a glsl reserved keyword.
        /// </summary>
        /// <param name="identifier">A glsl identifier.</param>
        /// <returns>
        ///   <c>true</c> if the specified identifier is a glsl reserved keyword; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsReserved(string identifier)
        {
            return Tokens.Contains(identifier);

        }
    }
}
