using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Paradox.DataModel;
using Paradox.Effects;
using Paradox.EntityModel;
using Paradox.Framework;
using Paradox.Framework.Extensions;
using Paradox.Framework.Serialization.Assets;
using Paradox.Framework.Serialization.Contents;

namespace Paradox.Engine
{
    /*
    public class CameraProcessor : EntityProcessor<CameraComponent>
    {
        private RenderSystem renderSystem;
        private EffectSystem effectSystem;

        public CameraProcessor()
            : base(new PropertyKey[] { MeshComponent.Key })
        {
        }

        protected internal override void OnSystemAdd()
        {
            renderSystem = (RenderSystem) Services.GetSafeServiceAs<IRenderSystem>();
            effectSystem = (EffectSystem)Services.GetSafeServiceAs<IEffectSystem>();
        }

        protected override CameraComponent GenerateAssociatedData(Entity entity)
        {
            return entity.Get(CameraComponent.Key);
        }

        protected override void OnEntityAdded(Entity entity, CameraComponent cameraComponent)
        {
            var instantiatedSubMeshes = cameraComponent.InstantiatedSubMeshes;
            cameraComponent.MeshComponent.InstantiatedSubMeshes = instantiatedSubMeshes;

            if (instantiatedSubMeshes != null)
            {
                foreach (var effectMesh in instantiatedSubMeshes)
                {
                    renderSystem.GlobalMeshes.AddMesh(effectMesh.Value);
                }
            }
        }

        protected override void OnEntityRemoved(Entity entity, CameraComponent associatedData)
        {
            var instantiatedSubMeshes = associatedData.InstantiatedSubMeshes;

            if (instantiatedSubMeshes != null)
            {
                foreach (var effectMesh in instantiatedSubMeshes)
                {
                    renderSystem.GlobalMeshes.RemoveMesh(effectMesh.Value);
                }
            }
        }


        public override void Update()
        {
            //renderSystem.DataContext.RenderPassPlugins.




            foreach (var entity in matchingEntities)
            {
                UpdateCamera(entity.Key, entity.Value);
            }
        }

        private EffectMesh GenerateEffectMesh(MeshComponent meshComponent, EffectMeshData effectMeshData)
        {
            var effect = effectSystem.CreateEffect(effectMeshData);
            var effectMesh = new EffectMesh(effect, effectMeshData);

            // TODO: Copy of parameters? (depending on prefab/clone processor)
            //effectMesh.Parameters.AddSources(effectMeshData.Parameters);
            effectMesh.Parameters.AddSources(meshComponent.MeshParameters);

            if (effectMesh.Permutations != null)
                effectMesh.Permutations.AddSources(meshComponent.Permutations);

            return effectMesh;
        }
        
        void instantiatedSubMeshes_CollectionChanged(object sender, Framework.Collections.TrackingCollectionChangedEventArgs e)
        {
            var effectMesh = (EffectMesh)e.Item;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    renderSystem.GlobalMeshes.AddMesh(effectMesh);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    renderSystem.GlobalMeshes.RemoveMesh(effectMesh);
                    break;
            }
        }
    }
     */
}