// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace NShader
{
    [Serializable]
    public class SourceLocation
    {
        public string File;

        public int Line;

        public int Column;

        public int EndLine;

        public int EndColumn;
    }
}