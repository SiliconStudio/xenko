// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Quantum.Legacy.Contents
{
    class NullWithParentNodeValueViewModelContent : ContentBase
    {
        public NullWithParentNodeValueViewModelContent()
            : base(typeof(object), null, false)
        {
        }

        public NullWithParentNodeValueViewModelContent(Type type)
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
