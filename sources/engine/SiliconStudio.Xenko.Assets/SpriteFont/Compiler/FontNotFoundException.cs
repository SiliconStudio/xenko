// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Xenko.Assets.SpriteFont.Compiler
{
    internal class FontNotFoundException : Exception
    {
        public FontNotFoundException(string fontName) : base(string.Format("Font with name [{0}] not found on this machine", fontName))
        {
            FontName = fontName;
        }

        public string FontName { get; private set; }
    }
}