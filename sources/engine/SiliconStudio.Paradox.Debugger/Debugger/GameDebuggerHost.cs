// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Paradox.Debugger.Target
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class GameDebuggerHost : IGameDebuggerHost
    {
        private TaskCompletionSource<IGameDebuggerTarget> target = new TaskCompletionSource<IGameDebuggerTarget>();

        public event Action GameExited;

        public Task<IGameDebuggerTarget> Target
        {
            get { return target.Task; }
        }

        public void RegisterTarget()
        {
            target.TrySetResult(OperationContext.Current.GetCallbackChannel<IGameDebuggerTarget>());
        }

        public void OnGameExited()
        {
            var gameExited = GameExited;
            if (gameExited != null)
                gameExited();
        }
    }
}