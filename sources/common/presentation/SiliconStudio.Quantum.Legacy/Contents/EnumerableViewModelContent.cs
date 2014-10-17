// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;

namespace SiliconStudio.Quantum.Legacy.Contents
{
    public class EnumerableViewModelContent
    {
        public static EnumerableViewModelContent<T> FromUnaryLambda<T, U>(IContent operand, Func<U, IEnumerable<T>> enumerateItems)
        {
            return new EnumerableViewModelContent<T>(operand, (x) => enumerateItems((U)x.Value));
        }
    }

    public class EnumerableViewModelContent<T> : UnaryViewModelContentBase
    {
        private readonly Func<IContent, IEnumerable<T>> enumerateItems;

        public EnumerableViewModelContent(IContent operand, Func<IContent, IEnumerable<T>> enumerateItems)
            : base(typeof(IList<T>), operand)
        {
            this.enumerateItems = enumerateItems;
        }

        public EnumerableViewModelContent(Func<IEnumerable<T>> enumerateItems)
            : base(typeof(IList<T>), new NullViewModelContent())
        {
            this.enumerateItems = x => enumerateItems();
        }
        
        public override object Value
        {
            get
            {
                return enumerateItems(Operand).ToArray();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                return true;
            }
        }
    }
}