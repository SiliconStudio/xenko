// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.MicroThreading;

namespace SiliconStudio.Xenko.Engine
{
    [DataContract("Script", Inherited = true)]
    public abstract class Script : ScriptContext
    {
        public const uint LiveScriptingMask = 128;

        [DataMemberIgnore]
        internal ScriptComponent ScriptComponent;

        private int priority;

        /// <summary>
        /// The priority this script will be scheduled with (compared to other scripts).
        /// </summary>
        /// <userdoc>The execution priority for this script. It applies to async, sync and startup scripts.</userdoc>
        [DefaultValue(0)]
        [DataMember(10000)]
        public int Priority
        {
            get { return priority; }
            set { priority = value; PriorityUpdated(); }
        }

        /// <summary>
        /// Determines whether the script is currently undergoing live reloading.
        /// </summary>
        public bool IsLiveReloading { get; internal set; }

        protected Script()
        {
        }

        protected Script(IServiceRegistry registry) : base(registry)
        {
        }

        /// <summary>
        /// Internal helper function called when <see cref="Priority"/> is changed.
        /// </summary>
        protected internal virtual void PriorityUpdated()
        {
        }

        /// <summary>
        /// Gets the entity this script is attached to.
        /// </summary>
        /// <value>
        /// The entity this script is attached to.
        /// </value>
        [DataMemberIgnore]
        public Entity Entity => ScriptComponent?.Entity;

        /// <summary>
        /// Called when the script's update loop is canceled.
        /// </summary>
        public virtual void Cancel()
        {
        }
    }
}