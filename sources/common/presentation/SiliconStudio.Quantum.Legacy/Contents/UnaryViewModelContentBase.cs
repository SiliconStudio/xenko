// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Quantum.Legacy.Contents
{
    public abstract class UnaryViewModelContentBase : ContentBase
    {
        protected UnaryViewModelContentBase(Type type, IContent operand)
            : base(type, null, false)
        {
            Operand = operand;
        }

        public IContent Operand { get; private set; }

        public override IViewModelNode OwnerNode
        {
            get
            {
                return base.OwnerNode;
            }
            set
            {
                Operand.OwnerNode = value;
                base.OwnerNode = value;
            }
        }
    }
}