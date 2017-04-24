// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
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
