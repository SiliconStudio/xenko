// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using Irony.Parsing;

namespace SiliconStudio.Shaders.Grammar
{
    internal abstract class DynamicKeyTerm : KeyTerm
    {
        protected DynamicKeyTerm(string text, string name)
            : base(text, name)
        {
        }

        public abstract void Match(Tokenizer toknizer, out Token token);
    }
}
