// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Paradox
{
    /// <summary>
    /// Delegate for a RenderPass action used by <see cref="Effects.RenderPass.StartPass"/> and <see cref="Effects.RenderPass.EndPass"/>.
    /// </summary>
    /// <param name="param">The param.</param>
    public struct DelegateHolder<T1>
    {
        public DelegateHolder(DelegateType delegateValue)
        {
            currentDelegate = delegateValue;
        }

        public static DelegateHolder<T1> operator +(DelegateHolder<T1> delegateHolder, DelegateType delegateValue)
        {
            if (delegateHolder.currentDelegate == null)
                return new DelegateHolder<T1>(delegateValue);
            return new DelegateHolder<T1>(delegateHolder.currentDelegate + delegateValue);
        }

        public static DelegateHolder<T1> operator -(DelegateHolder<T1> delegateHolder, DelegateType delegateValue)
        {
            if (delegateHolder.currentDelegate == null)
                return delegateHolder;

            return new DelegateHolder<T1>((DelegateType)Delegate.Remove(delegateHolder.currentDelegate, delegateValue));
        }
        
        /// <summary>
        /// Delegate for a RenderPass action used by <see cref="Effects.RenderPass.StartPass"/> and <see cref="Effects.RenderPass.EndPass"/>.
        /// </summary>
        /// <param name="param">The param.</param>
        public delegate void DelegateType(T1 param);

        private DelegateType currentDelegate;

        public void Invoke(T1 param)
        {
            if (currentDelegate != null)
                currentDelegate(param);
        }

        /// <summary>
        /// Set delegate action.
        /// </summary>
        /// <value>
        /// The action to set.
        /// </value>
        public DelegateType Set
        {
            set
            {
                currentDelegate = value;
            }
        }
        
        /// <summary>
        /// Adds an action at the end of the invocation list.
        /// </summary>
        /// <value>
        /// The action to add.
        /// </value>
        public DelegateType AddLast
        {
            set
            {
                if (currentDelegate == null)
                    currentDelegate = value;
                else
                    currentDelegate += value;
            }
        }

        /// <summary>
        /// Adds an action at the beginning of the invocation list.
        /// </summary>
        /// <value>
        /// The action to add.
        /// </value>
        public DelegateType AddFirst
        {
            set
            {
                if (currentDelegate == null)
                    currentDelegate = value;
                else
                    currentDelegate = (DelegateType)Delegate.Combine(value, currentDelegate);
            }
        }
    }
}