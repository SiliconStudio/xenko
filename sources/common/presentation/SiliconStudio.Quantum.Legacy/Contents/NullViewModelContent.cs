// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Quantum.Legacy.Contents
{
    public class NullViewModelContent : ContentBase
    {
        public NullViewModelContent()
            : base(typeof(object), null, false)
        {
        }

        public NullViewModelContent(Type type)
            : base(type, null, false)
        {
        }

        public override object Value
        {
            get { return null; }
            set { throw new NotImplementedException(); }
        }
    }
}