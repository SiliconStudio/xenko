// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;

using SiliconStudio.Xenko;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;

namespace ScriptShader.Effects
{
    public class ParticleUpdater
    {
        public Entity Entity;

        public ParticleEmitterComponent Emitter;

        public TransformationComponent Transform;

        public MeshComponent Mesh;
    }

    public class ParticleProcessor : EntityProcessor<ParticleUpdater>
    {
        public static readonly PropertyKey<ParticlePlugin> ParticlePluginKey = new PropertyKey<ParticlePlugin>("ParticlePlugin", typeof(ParticleProcessor));

        private ParticlePlugin particlePlugin;

        private List<ParticleUpdater> updaters;

        // TODO: Add support for optinal keys again, for MeshComponent.Key
        public ParticleProcessor(ParticlePlugin particlePlugin)
            : base(new PropertyKey[] { ParticleEmitterComponent.Key, TransformationComponent.Key })
        {
            this.particlePlugin = particlePlugin;
            updaters = new List<ParticleUpdater>();
        }

        protected override ParticleUpdater GenerateAssociatedData(Entity entity)
        {
            return new ParticleUpdater()
                {
                    Entity = entity,
                    Emitter = entity.Get(ParticleEmitterComponent.Key),
                    Transform = entity.Transformation,
                    Mesh = entity.Get(MeshComponent.Key),
                };
        }

        protected override void OnEntityAdded(Entity entity, ParticleUpdater data)
        {
            data.Emitter.OnAddToSystem(Services);

            if (data.Mesh != null)
            {
                throw new NotImplementedException();
                //var meshListener = data.Mesh.SubMeshes as ITrackingCollectionChanged;
                //if (meshListener != null)
                //    meshListener.CollectionChanged += MeshListenerOnCollectionChanged;

                data.Emitter.OnMeshUpdate(data.Entity, new TrackingCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, null, null));
            }

            updaters.Add(data);
            particlePlugin.Updaters.Add(data.Emitter);
        }

        protected override void OnEntityRemoved(Entity entity, ParticleUpdater data)
        {
            if (data.Mesh != null)
            {
                throw new NotImplementedException();
                //var meshListener = data.Mesh.SubMeshes as ITrackingCollectionChanged;
                //if (meshListener != null)
                //    meshListener.CollectionChanged -= MeshListenerOnCollectionChanged;
            }

            particlePlugin.Updaters.Remove(data.Emitter);
            updaters.Remove(data);
        }

        private void MeshListenerOnCollectionChanged(object sender, TrackingCollectionChangedEventArgs trackingCollectionChangedEventArgs)
        {
            throw new NotImplementedException();
            //var updater = updaters.Where(particleUpdater => particleUpdater.Mesh != null).FirstOrDefault(particleUpdater => ReferenceEquals(sender, particleUpdater.Mesh.SubMeshes));
            //if (updater != null)
            //    updater.Emitter.OnMeshUpdate(updater.Entity, trackingCollectionChangedEventArgs);
        }

        public override void Update()
        {
            foreach (var matchingEntity in enabledEntities)
            {
                var entity = matchingEntity.Key;
                var name = entity.Name;
                var particleEmitterComponent = matchingEntity.Value.Emitter;
                var transformationComponent = matchingEntity.Value.Transform;

                particleEmitterComponent.Parameters.Set(TransformationKeys.World, transformationComponent.WorldMatrix);

                // Call the OnUpdateSystem on the particle emitter
                particleEmitterComponent.OnUpdateSystem();
            }
        }
    }
}
