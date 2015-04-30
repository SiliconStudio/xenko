// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.MicroThreading;

namespace SiliconStudio.Paradox.Engine
{
    [DataContract("Script", Inherited = true)]
    public abstract class Script : ScriptContext
    {
        [DataMemberIgnore]
        internal ScriptComponent ScriptComponent;
        [DataMemberIgnore]
        internal MicroThread MicroThread;
        [DataMemberIgnore]
        internal bool Unloaded;

        /// <summary>
        /// The script identifier.
        /// </summary>
        [Browsable(false)]
        public new Guid Id;

        protected Script()
        {
            Id = Guid.NewGuid();
        }

        protected Script(IServiceRegistry registry) : base(registry)
        {
            Id = Guid.NewGuid();
        }

        /// <summary>
        /// Gets the entity this script is attached to.
        /// </summary>
        /// <value>
        /// The entity this script is attached to.
        /// </value>
        [DataMemberIgnore]
        public Entity Entity
        {
            // Note: we might want to make this property public?
            get { return ScriptComponent != null ? ScriptComponent.Entity : null; }
        }

        /// <summary>
        /// Called before any other method in the script.
        /// </summary>
        public virtual void Start()
        {
        }
    }
}