// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Threading.Tasks;

namespace SiliconStudio.Quantum.Legacy.Contents
{
    public class AsyncViewModelContent<T> : UnaryViewModelContentBase, IAsyncViewModelContent
    {
        private readonly Task<T> task;

        public AsyncViewModelContent(IContent operand, Func<IContent, T> function)
            : base(typeof(T), operand)
        {
            task = new Task<T>(() => function(Operand));
            task.ContinueWith(_ => this.LoadState = ViewModelContentState.Loaded);
            LoadState = ViewModelContentState.NotLoaded;
            SerializeFlags = ViewModelContentSerializeFlags.Static | ViewModelContentSerializeFlags.Async | ViewModelContentSerializeFlags.SerializeValue;
        }

        public override object Value
        {
            get
            {
                return task.IsCompleted ? task.Result : default(T);
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public void RequestLoadContent()
        {
            lock (task)
            {
                if (LoadState == ViewModelContentState.NotLoaded)
                {
                    LoadState = ViewModelContentState.Loading;
                    task.Start();
                }
            }
        }
    }
}