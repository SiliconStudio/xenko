// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Quantum.Legacy.Contents
{
    public class LambdaViewModelContent<T> : UnaryViewModelContentBase
    {
        private readonly Func<IContent, T> getter;
        private readonly Action<IContent, T> setter;

        public LambdaViewModelContent(IContent operand, Func<IContent, T> getter, Action<IContent, T> setter = null)
            : base(typeof(T), operand)
        {
            this.getter = getter;
            this.setter = setter;
        }

        public LambdaViewModelContent(Func<T> getter, Action<T> setter = null)
            : this(new NullViewModelContent(), (operand1) => getter(), (operand1, value) => setter(value))
        {
        }

        public static LambdaViewModelContent<T> FromOperand<U>(IContent operand, Func<U, T> getter, Action<U, T> setter = null)
        {
            return new LambdaViewModelContent<T>(operand, (operand1) => getter((U)operand1.Value), (operand1, value) => setter((U)operand1.Value, value));
        }

        public static LambdaViewModelContent<T> FromParent<U>(Func<U, T> getter, Action<U, T> setter = null)
        {
            return FromOperand(new ParentValueViewModelContent(), getter, setter);
        }

        public override object Value
        {
            get
            {
                return getter(Operand);
            }
            set
            {
                if (setter != null)
                    setter(Operand, (T)value);
            }
        }
    }
}