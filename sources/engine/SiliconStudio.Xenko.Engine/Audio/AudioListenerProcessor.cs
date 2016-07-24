// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Audio
{
    /// <summary>
    /// Processor in charge of creating and updating the <see cref="AudioListener"/> data associated to the scene <see cref="AudioListenerComponent"/>s.
    /// </summary>
    /// <remarks>
    /// The processor updates only <see cref="AudioListener"/> associated to <see cref="AudioListenerComponent"/>s 
    /// The processor is subscribing to the <see cref="audioSystem"/> <see cref="AudioListenerComponent"/> collection events to be informed of required <see cref="AudioEmitter"/> updates.
    /// When a <see cref="AudioListenerComponent"/> is added to the <see cref="audioSystem"/>, the processor set the associated <see cref="AudioEmitter"/>.
    /// When a <see cref="AudioListenerComponent"/> is removed from the entity system, 
    /// the processor set the <see cref="AudioEmitter"/> reference of the <see cref="AudioSystem"/> to null 
    /// but do not remove the <see cref="AudioListenerComponent"/> from its collection.
    /// </remarks>
    public class AudioListenerProcessor : EntityProcessor<AudioListenerComponent, AudioListenerComponent>
    {
        /// <summary>
        /// Reference to the <see cref="AudioSystem"/> of the game instance.
        /// </summary>
        private AudioSystem audioSystem;

        /// <summary>
        /// Create a new instance of AudioListenerProcessor.
        /// </summary>
        public AudioListenerProcessor()
            : base(typeof(AudioListenerComponent))
        {
        }

        protected override AudioListenerComponent GenerateComponentData(Entity entity, AudioListenerComponent component)
        {
            return component;
        }

        protected override bool IsAssociatedDataValid(Entity entity, AudioListenerComponent component, AudioListenerComponent associatedData)
        {
            return component == associatedData;
        }

        protected internal override void OnSystemAdd()
        {
            base.OnSystemAdd();

            audioSystem = Services.GetServiceAs<AudioSystem>();
        }

        protected internal override void OnSystemRemove()
        {
            base.OnSystemRemove();

            audioSystem.Listeners.Clear();
        }

        protected override void OnEntityComponentAdding(Entity entity, AudioListenerComponent component, AudioListenerComponent data)
        {
            base.OnEntityComponentAdding(entity, component, data);

            component.Listener = new AudioListener(audioSystem.AudioEngine);

            audioSystem.Listeners.Add(component, component.Listener);
        }

        protected override void OnEntityComponentRemoved(Entity entity, AudioListenerComponent component, AudioListenerComponent data)
        {
            base.OnEntityComponentRemoved(entity, component, data);

            audioSystem.Listeners.Remove(component);

            component.Listener.Dispose();
        }

        public override void Draw(RenderContext context)
        {
            base.Draw(context);

            foreach (var listenerData in ComponentDatas.Values)
            {
                if(!listenerData.Enabled)  // skip all updates if the listener is not used.
                    continue;

                var listener = listenerData.Listener;
                var worldMatrix = listenerData.Entity.Transform.WorldMatrix;
                var newPosition = worldMatrix.TranslationVector;
                listener.Velocity = newPosition - listener.Position; // estimate velocity from last and new position
                listener.Position = newPosition;
                listener.Forward = Vector3.Normalize((Vector3)worldMatrix.Row3);
                listener.Up = Vector3.Normalize((Vector3)worldMatrix.Row2);

                listener.Update();
            }
        }
    }
}
