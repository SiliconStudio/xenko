// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

namespace NShader
{
    /// <summary>
    /// Result of shader navigation.
    /// </summary>
    [Serializable]
    public class RawShaderNavigationResult
    {
        public RawShaderNavigationResult()
        {
            Messages = new List<RawShaderAnalysisMessage>();
        }

        /// <summary>
        /// Gets or sets the definition Span.
        /// </summary>
        /// <value>The definition Span.</value>
        public RawSourceSpan DefinitionSpan { get; set; }

        /// <summary>
        /// Gets the parsing messages.
        /// </summary>
        /// <value>The messages.</value>
        public List<RawShaderAnalysisMessage> Messages { get; private set; }
    }
}