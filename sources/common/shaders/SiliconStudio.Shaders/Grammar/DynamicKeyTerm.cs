// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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
