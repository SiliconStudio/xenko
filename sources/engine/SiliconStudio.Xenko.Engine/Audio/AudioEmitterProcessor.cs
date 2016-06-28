// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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
                TransformComponent = entity.Transform
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

            // Destroy all the SoundInstance created by the processor before closing.
            foreach (var soundInstance in ComponentDatas.Values.SelectMany(x => x.AudioEmitterComponent.SoundToController.Values))
                soundInstance.DestroyAllSoundInstances();

            audioSystem.Listeners.CollectionChanged -= OnListenerCollectionChanged;
        }

        protected override void OnEntityComponentAdding(Entity entity, AudioEmitterComponent component, AssociatedData data)
        {
            base.OnEntityComponentAdding(entity, component, data);

            // initialize the AudioEmitter first position
            data.TransformComponent.UpdateWorldMatrix(); // ensure the worldMatrix is correct
            data.AudioEmitter = new AudioEmitter { Position = data.TransformComponent.WorldMatrix.TranslationVector }; // valid position is needed at first Update loop to compute velocity.

            // create a SoundInstance for each listener activated and for each sound controller of the EmitterComponent.
            foreach (var listener in audioSystem.Listeners.Keys)
            {
                foreach (var soundController in data.AudioEmitterComponent.SoundToController.Values)
                {
                    soundController.CreateSoundInstance(listener);
                }
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
                var pos = worldMatrix.TranslationVector;

                if (!associatedData.AudioEmitterComponent.ShouldBeProcessed)
                {   
                    // to be sure to have a valid velocity at any time we are forced to affect position even if Component need not to be processed.
                    emitter.Position = pos;
                    continue;
                }

                // First update the emitter data if required.
                emitter.Velocity = pos - emitter.Position;
                emitter.Position = pos;
                emitter.Forward = Vector3.Normalize((Vector3)worldMatrix.Row3);
                emitter.Up = Vector3.Normalize((Vector3)worldMatrix.Row2);

                // Then apply 3D localization
                var performedAtLeastOneApply = false;
                foreach (var controller in associatedData.AudioEmitterComponent.SoundToController.Values)
                {
                    foreach (var listenerComponent in audioSystem.Listeners.Keys)
                    {
                        //todo this will be improved when we make Sound behave more like Animations
                        SoundInstance instance = null;
                        foreach (var v in controller.InstanceToListener)
                        {
                            if (v.Value != listenerComponent) continue;
                            instance = v.Key;
                            break;
                        }

                        if(instance == null) continue;

                        if (!listenerComponent.Enabled)
                        {
                            instance.Stop();
                            continue;
                        }
                        
                        // Apply3D localization
                        if (instance.PlayState == SoundPlayState.Playing || controller.ShouldBePlayed)
                        {
                            instance.Apply3D(emitter);
                            performedAtLeastOneApply = true;
                        }

                        //Apply parameters
                        if (instance.Volume != controller.Volume) instance.Volume = controller.Volume; // ensure that instance volume is valid
                        if (instance.IsLooped != controller.IsLooped) instance.IsLooped = controller.IsLooped;

                        //Play if stopped
                        if (instance.PlayState != SoundPlayState.Playing && controller.ShouldBePlayed)
                        {
                            instance.Play(false);
                        }
                    }
                }

                associatedData.AudioEmitterComponent.ShouldBeProcessed = performedAtLeastOneApply;
            }
        }

        protected override void OnEntityComponentRemoved(Entity entity, AudioEmitterComponent component, AssociatedData data)
        {
            base.OnEntityComponentRemoved(entity, component, data);

            // dispose and delete all SoundInstances associated to the EmitterComponent.
            foreach (var soundController in data.AudioEmitterComponent.SoundToController.Values)
                soundController.DestroyAllSoundInstances();

            data.AudioEmitterComponent.ControllerCollectionChanged -= OnSoundControllerListChanged;
        }

        private void OnListenerCollectionChanged(object o, TrackingCollectionChangedEventArgs args)
        {
            if (!args.CollectionChanged)// no keys have been added or removed, only one of the values changed
                return;
            
            // A listener have been Added or Removed. 
            // We need to create/destroy all SoundInstances associated to that listener for each AudioEmitterComponent.

            foreach (var associatedData in ComponentDatas.Values)
            {
                var soundControllers = associatedData.AudioEmitterComponent.SoundToController.Values;

                foreach (var soundController in soundControllers)
                {
                    if (args.Action == NotifyCollectionChangedAction.Add)   // A new listener have been added
                    {
                        soundController.CreateSoundInstance((AudioListenerComponent)args.Key);
                    }
                    else if (args.Action == NotifyCollectionChangedAction.Remove) // A listener have been removed
                    {
                        soundController.DestroySoundInstances((AudioListenerComponent)args.Key);
                    }
                }
            }
        }

        private void OnSoundControllerListChanged(object o, AudioEmitterComponent.ControllerCollectionChangedEventArgs args)
        {
            AssociatedData associatedData;
            if (!ComponentDatas.TryGetValue(args.EmitterComponent, out associatedData))
                return;

            // A new Sound have been associated to the AudioEmitterComponenent or an old Sound have been deleted.
            // We need to create/destroy the corresponding SoundInstances.

            var listeners = audioSystem.Listeners.Keys;
            foreach (var listener in listeners)
            {
                if (args.Action == NotifyCollectionChangedAction.Add)
                {
                    args.Controller.CreateSoundInstance(listener);
                }
                else if(args.Action == NotifyCollectionChangedAction.Remove )
                {
                    args.Controller.DestroySoundInstances(listener);
                }
            }
        }
    }
}
