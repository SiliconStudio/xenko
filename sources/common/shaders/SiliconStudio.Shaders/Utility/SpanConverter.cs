// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Shaders.Parser;
using SiliconStudio.Shaders.Ast;

namespace SiliconStudio.Shaders.Utility
{
    public class SpanConverter
    {
        public static SourceLocation Convert(Irony.Parsing.SourceLocation sourceLocation)
        {
            return new SourceLocation(sourceLocation.SourceFilename, sourceLocation.Position, sourceLocation.Line, sourceLocation.Column);
        }

        public static Irony.Parsing.SourceLocation Convert(SourceLocation sourceLocation)
        {
            return new Irony.Parsing.SourceLocation(sourceLocation.Position, sourceLocation.Line, sourceLocation.Column, sourceLocation.FileSource);
        }

        public static SourceSpan Convert(Irony.Parsing.SourceSpan sourceSpan)
        {
            return new SourceSpan(Convert(sourceSpan.Location), sourceSpan.Length);
        }
    }
}
