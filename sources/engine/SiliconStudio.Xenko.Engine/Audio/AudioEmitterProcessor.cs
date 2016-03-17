// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Audio
{
    /// <summary>
    /// Processor in charge of updating the <see cref="AudioEmitterComponent"/>s.
    /// </summary>
    /// <remarks>
    /// <para>More precisely it updates the <see cref="AudioEmitter"/>s and 
    /// then applies 3D localization to each couple <see cref="AudioEmitterComponent"/>-<see cref="AudioListenerComponent"/>.
    /// When a new emitter or a new listener is added to the system, its creates the required SoundInstances and associate them with the new emitter/listener tuples.
    /// </para> 
    /// </remarks>
    public class AudioEmitterProcessor: EntityProcessor<AudioEmitterComponent, AudioEmitterProcessor.AssociatedData>
    {
        /// <summary>
        /// Reference to the audioSystem.
        /// </summary>
        private AudioSystem audioSystem;

        /// <summary>
        /// Data associated to each <see cref="Entity"/> instances of the system having an <see cref="AudioEmitterComponent"/> and an <see cref="TransformComponent"/>.
        /// </summary>
        public class AssociatedData
        {
            /// <summary>
            /// The <see cref="Xenko.Audio.AudioEmitter"/> associated to the <see cref="AudioEmitterComponent"/>.
            /// </summary>
            public AudioEmitter AudioEmitter;

            /// <summary>
            /// The <see cref="Engine.AudioEmitterComponent"/> associated to the entity
            /// </summary>
            public AudioEmitterComponent AudioEmitterComponent;

            /// <summary>
            /// The <see cref="TransformComponent"/> associated to the entity
            /// </summary>
            public TransformComponent TransformComponent;

            /// <summary>
            /// A dictionary associating each activated listener of the AudioSystem and each sound controller of the <see cref="AudioEmitterComponent"/> to a valid sound effect instance.
            /// </summary>
            public Dictionary<Tuple<AudioListenerComponent, AudioEmitterSoundController>, SoundEffectInstance> ListenerControllerToSoundInstance;
        }

        /// <summary>
        /// Create a new instance of the processor.
        /// </summary>
        public AudioEmitterProcessor()
            : base(typeof(TransformComponent))
        {
        }

        protected internal override void OnSystemAdd()
        {
            base.OnSystemAdd();

            audioSystem = Services.GetServiceAs<AudioSystem>();

            audioSystem.Listeners.CollectionChanged += OnListenerCollectionChanged;
        }

        protected override AssociatedData GenerateComponentData(Entity entity, AudioEmitterComponent component)
        {
            return new AssociatedData
            {
                AudioEmitterComponent = component,
                TransformComponent = entity.Transform,
                ListenerControllerToSoundInstance = new Dictionary<Tuple<AudioListenerComponent, AudioEmitterSoundController>, SoundEffectInstance>()
            };
        }

        protected override bool IsAssociatedDataValid(Entity entity, AudioEmitterComponent component, AssociatedData associatedData)
        {
            return
                component == associatedData.AudioEmitterComponent &&
                entity.Transform == associatedData.TransformComponent;
        }

        protected internal override void OnSystemRemove()
        {
            base.OnSystemRemove();

            // Destroy all the SoundEffectInstance created by the processor before closing.
            foreach (var soundInstance in ComponentDatas.Values.SelectMany(x => x.AudioEmitterComponent.SoundEffectToController.Values))
                soundInstance.DestroyAllSoundInstances();

            audioSystem.Listeners.CollectionChanged -= OnListenerCollectionChanged;
        }

        protected override void OnEntityComponentAdding(Entity entity, AudioEmitterComponent component, AssociatedData data)
        {
            base.OnEntityComponentAdding(entity, component, data);

            // initialize the AudioEmitter first position
            data.TransformComponent.UpdateWorldMatrix(); // ensure the worldMatrix is correct
            data.AudioEmitter = new AudioEmitter { Position = data.TransformComponent.WorldMatrix.TranslationVector }; // valid position is needed at first Update loop to compute velocity.

            // create a SoundEffectInstance for each listener activated and for each sound controller of the EmitterComponent.
            foreach (var listener in audioSystem.Listeners.Keys)
            {
                foreach (var soundController in data.AudioEmitterComponent.SoundEffectToController.Values)
                    data.ListenerControllerToSoundInstance[Tuple.Create(listener, soundController)] = soundController.CreateSoundInstance();
            }

            data.AudioEmitterComponent.ControllerCollectionChanged += OnSoundControllerListChanged;
        }

        public override void Draw(RenderContext context)
        {
            base.Draw(context);

            foreach (var associatedData in ComponentDatas.Values)
            {
                var emitter = associatedData.AudioEmitter;
                var worldMatrix = associatedData.TransformComponent.WorldMatrix;
                var newPosition = worldMatrix.TranslationVector;

                if (!associatedData.AudioEmitterComponent.ShouldBeProcessed)
                {   // to be sure to have a valid velocity at any time we are forced to affect position even if Component need not to be processed.
                    emitter.Position = newPosition;
                    continue;
                }

                // First update the emitter data if required.
                emitter.DistanceScale = associatedData.AudioEmitterComponent.DistanceScale;
                emitter.DopplerScale = associatedData.AudioEmitterComponent.DopplerScale;
                emitter.Velocity = newPosition - emitter.Position;
                emitter.Position = newPosition;

                // Then apply 3D localization
                var performedAtLeastOneApply = false;
                foreach (var controller in associatedData.AudioEmitterComponent.SoundEffectToController.Values)
                {
                    foreach (var listenerComponent in audioSystem.Listeners.Keys)
                    {
                        var currentTupple = Tuple.Create(listenerComponent, controller);
                        var instance = associatedData.ListenerControllerToSoundInstance[currentTupple];
                        var listener = audioSystem.Listeners[listenerComponent];

                        if (listener == null)   // ListenerComponent activated but not present into the entity system anymore/yet. 
                        {                       // Thus it can not be processed by the AudioListenerProcessor and does not contain valid AudioListener data.
                            instance.Stop();    // Thus stops any instances that was possibly playing.
                            continue;           // and ignore any possible play request
                        }

                        // Apply3D localization
                        if (instance.PlayState == SoundPlayState.Playing || controller.ShouldBePlayed)
                        {
                            instance.Apply3D(listener, emitter);
                            performedAtLeastOneApply = true;
                        }

                        // Finally start playing the sounds if needed
                        if (controller.ShouldBePlayed)
                        {
                            instance.Volume = controller.Volume; // ensure that instance volume is valid
                            if(instance.PlayState == SoundPlayState.Stopped)
                                instance.IsLooped = controller.IsLooped && !controller.ShouldExitLoop;    // update instances' IsLooped value, if was set by the user when when not listeners where activated.
                            instance.Play(false);
                        }
                    }
                    controller.ShouldBePlayed = false;
                    controller.ShouldExitLoop = false;
                }

                associatedData.AudioEmitterComponent.ShouldBeProcessed = performedAtLeastOneApply;
            }
        }

        protected override void OnEntityComponentRemoved(Entity entity, AudioEmitterComponent component, AssociatedData data)
        {
            base.OnEntityComponentRemoved(entity, component, data);

            // dispose and delete all SoundEffectInstances associated to the EmitterComponent.
            foreach (var soundController in data.AudioEmitterComponent.SoundEffectToController.Values)
                soundController.DestroyAllSoundInstances();

            data.AudioEmitterComponent.ControllerCollectionChanged -= OnSoundControllerListChanged;
        }

        private void OnListenerCollectionChanged(object o, TrackingCollectionChangedEventArgs args)
        {
            if (!args.CollectionChanged)// no keys have been added or removed, only one of the values changed
                return;
            
            // A listener have been Added or Removed. 
            // We need to create/destroy all SoundEffectInstances associated to that listener for each AudioEmitterComponent.

            foreach (var associatedData in ComponentDatas.Values)
            {
                var listenerControllerToSoundInstance = associatedData.ListenerControllerToSoundInstance;
                var soundControllers = associatedData.AudioEmitterComponent.SoundEffectToController.Values;

                foreach (var soundController in soundControllers)
                {
                    var currentTupple = Tuple.Create((AudioListenerComponent)args.Key, soundController);

                    if (args.Action == NotifyCollectionChangedAction.Add)   // A new listener have been added
                    {
                        listenerControllerToSoundInstance[currentTupple] = soundController.CreateSoundInstance();
                    }
                    else if (args.Action == NotifyCollectionChangedAction.Remove) // A listener have been removed
                    {
                        soundController.DestroySoundInstance(listenerControllerToSoundInstance[currentTupple]);
                        listenerControllerToSoundInstance.Remove(currentTupple);
                    }
                }
            }
        }

        private void OnSoundControllerListChanged(object o, AudioEmitterComponent.ControllerCollectionChangedEventArgs args)
        {
            AssociatedData associatedData = null;
            Internal.Refactor.ThrowNotImplementedException(null);
            //if (!ComponentDatas.TryGetValue(args.Entity, out associatedData))
            //    return;

            // A new SoundEffect have been associated to the AudioEmitterComponenent or an old SoundEffect have been deleted.
            // We need to create/destroy the corresponding SoundEffectInstances.

            var listeners = audioSystem.Listeners.Keys;
            foreach (var listener in listeners)
            {
                var currentTuple = Tuple.Create(listener, args.Controller);

                if (args.Action == NotifyCollectionChangedAction.Add)
                {
                    associatedData.ListenerControllerToSoundInstance[currentTuple] = args.Controller.CreateSoundInstance();
                }
                else if(args.Action == NotifyCollectionChangedAction.Remove )
                {
                    args.Controller.DestroySoundInstance(associatedData.ListenerControllerToSoundInstance[currentTuple]);
                    associatedData.ListenerControllerToSoundInstance.Remove(currentTuple);
                }
            }
        }
    }
}
