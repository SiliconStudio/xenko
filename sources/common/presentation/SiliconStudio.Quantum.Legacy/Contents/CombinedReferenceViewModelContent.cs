// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Linq;

namespace SiliconStudio.Quantum.Legacy.Contents
{
    public class CombinedReferenceViewModelContent : CombinedViewModelContent
    {
        private IContent[] contents;

        public CombinedReferenceViewModelContent(IContent[] contents) : base(contents)
        {
            this.contents = contents;
        }

        public override object Value
        {
            get
            {
                return new CombinedViewModelReference(contents.Select(x => ((ViewModelReference)x.Value).Model));
            }
            set
            {
                throw new InvalidOperationException();
            }
        }
    }
}