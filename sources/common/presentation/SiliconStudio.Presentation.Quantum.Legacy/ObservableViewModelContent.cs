// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.Legacy;
using SiliconStudio.Quantum.Legacy.Contents;

namespace SiliconStudio.Presentation.Quantum.Legacy
{
    public class ObservableViewModelContent<T> : IContent
    {
        public ObservableViewModelContent(bool isReadOnly)
        {
            IsReadOnly = isReadOnly;
        }

        public bool IsReadOnly { get; protected set; }

        public Type Type
        {
            get { return typeof(T); }
        }

        public IViewModelNode OwnerNode { get; set; }

        public object Value
        {
            get { return TValue; }
            set { TValue = (T)value; }
        }

        public T TValue { get; set; }

        public ViewModelContentFlags Flags { get; set; }

        public ViewModelContentSerializeFlags SerializeFlags { get; set; }

        public ITypeDescriptor Descriptor { get; private set; }

        public object UpdatedValue { get { throw new InvalidOperationException(); } private set { throw new InvalidOperationException(); } }

        public ViewModelContentState LoadState { get; set; }

        public object AssociatedData { get; set; }
    }
}