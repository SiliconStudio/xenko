// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiliconStudio.TextureConverter
{
    public class TextureToolsException : ApplicationException
    {
        public TextureToolsException() : base() {}
        public TextureToolsException(string message) : base(message) {}
        public TextureToolsException(string message, System.Exception inner) : base(message, inner) {}
        protected TextureToolsException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
