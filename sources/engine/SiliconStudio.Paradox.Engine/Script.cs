// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Threading.Tasks;

using SiliconStudio.Core;
using SiliconStudio.Core.MicroThreading;
using SiliconStudio.Paradox.Engine;

namespace SiliconStudio.Paradox
{
    [DataContract("Script")]
    public abstract class Script : ScriptContext, IScript
    {
        [DataMemberIgnore]
        internal ScriptComponent ScriptComponent;
        [DataMemberIgnore]
        internal MicroThread MicroThread;
        [DataMemberIgnore]
        internal bool Unloaded;

        protected Script()
        {
        }

        protected Script(IServiceRegistry registry) : base(registry)
        {
        }

        public abstract Task Execute();
    }
}