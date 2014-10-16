// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Core.Extensions;

namespace SiliconStudio.Quantum.Legacy.Contents
{
    public class CombinedViewModelContent : ContentBase
    {
        private IContent[] contents;

        public CombinedViewModelContent(IContent[] contents)
            : base(contents[0].Type, null, false)
        {
            this.contents = contents;
        }

        public override object Value
        {
            get
            {
                object value;
                var combineResult = contents.Select(x => x.Value).AllEqual(out value);

                if (combineResult)
                {
                    Flags &= ~ViewModelContentFlags.CombineError;
                }
                else
                {
                    Flags |= ViewModelContentFlags.CombineError;
                    value = "<Different values>";
                }
                return value;
            }
            set
            {
                foreach (var content in contents)
                    content.Value = value;
            }
        }

        public IEnumerable<object> Values
        {
            get
            {
                return contents.Select(x => x.Value);
            }
        }
    }
}