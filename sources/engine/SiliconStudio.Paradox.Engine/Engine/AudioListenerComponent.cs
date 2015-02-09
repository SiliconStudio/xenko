// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Paradox.EntityModel;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// Component representing an audio listener.
    /// </summary>
    /// <remarks>
    /// <para>Associate this component to an <see cref="Entity"/> to simulate a physical listener listening to the <see cref="AudioEmitterComponent"/>s of the scene,
    /// placed at the entity's center and oriented along the entity's Oz (forward) and Oy (up) vectors.</para>
    /// <para>Use the AudioSytem's <see cref="AudioSystem.AddListener"/> and <see cref="AudioSystem.RemoveListener"/> functions 
    /// to activate/deactivate the listeners that are actually listening at a given time.</para>
    /// <para>The entity needs to be added to the Entity System so that the associated AudioListenerComponent can be processed.</para></remarks>
    [Display(60, "Audio Listener")]
    public sealed class AudioListenerComponent : EntityComponent
    {
        public static PropertyKey<AudioListenerComponent> Key = new PropertyKey<AudioListenerComponent>("Key", typeof(AudioListenerComponent));

        protected internal override PropertyKey DefaultKey
        {
            get { return Key; }
        }

        private static readonly Type[] DefaultProcessors = new Type[] { typeof(AudioListenerProcessor) };
        protected internal override IEnumerable<Type> GetDefaultProcessors()
        {
            return DefaultProcessors;
        }
    }
}
