// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
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
