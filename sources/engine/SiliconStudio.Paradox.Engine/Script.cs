// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Threading.Tasks;

using SiliconStudio.Core;

namespace SiliconStudio.Paradox
{
    public abstract class Script : ScriptContext, IScript
    {
        protected Script(IServiceRegistry registry) : base(registry)
        {
        }

        public abstract Task Execute();
    }
}