// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;

namespace SiliconStudio.Quantum.Legacy.Contents
{
    public class CombinedListReferenceViewModelContent : CombinedViewModelContent
    {
        private List<ViewModelReference> references = new List<ViewModelReference>();
        private IContent[] contents;

        public CombinedListReferenceViewModelContent(IContent[] contents) : base(contents)
        {
            this.contents = contents;
        }

        public override object Value
        {
            get
            {
                references.Clear();

                var referenceListFirstChild = ((IList<ViewModelReference>)contents[0].Value);
                for (int i = 0; i < referenceListFirstChild.Count; ++i)
                {
                    int index = i;
                    references.Add(new CombinedViewModelReference(contents.Select(x => ((IList<ViewModelReference>)x.Value)[index].Model)));
                }

                return references;
            }
            set
            {
                throw new InvalidOperationException();
            }
        }
    }
}