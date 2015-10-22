// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace NShader
{
    /// <summary>
    /// Defines a Span span in a file.
    /// </summary>
    [Serializable]
    public class RawSourceSpan
    {
        public RawSourceSpan()
        {
        }

        public RawSourceSpan(string file, int line, int column)
        {
            File = file;
            Line = line;
            Column = column;
            EndLine = line;
            EndColumn = column;
        }

        public string File { get; set; }

        public int Line { get; set; }

        public int Column { get; set; }

        public int EndLine { get; set; }

        public int EndColumn { get; set; }

        public override string ToString()
        {
            // TODO: include span
            return string.Format("{0}({1},{2})", File ?? string.Empty, Line, Column);
        }
    }
}