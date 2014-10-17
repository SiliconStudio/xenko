using System;
using System.Linq;

using SiliconStudio.Paradox;
using SiliconStudio.Paradox.DataModel;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Extensions;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Games.Collections;
using SiliconStudio.Paradox.Games.Serialization.Contents;
using Paradox.Framework.Shaders;
using Utilities = SiliconStudio.Paradox.Games.Utilities;
using Vector3 = SiliconStudio.Paradox.Games.Mathematics.Vector3;
using Buffer = SiliconStudio.Paradox.Graphics.Buffer;
using Half = SiliconStudio.Paradox.Games.Mathematics.Half;
using Half4 = SiliconStudio.Paradox.Games.Mathematics.Half4;

namespace ScriptTest
{
    [ContentSerializer(typeof(EntityComponentContentSerializer<BulletParticleEmitterComponent>))]
    [Display]
    public class BulletParticleEmitterComponent : ParticleEmitterComponent
    {
        internal EntitySystem EntitySystem;
        internal RenderContextBase renderContext;

        private TimeSpan lastTime;

        private Entity dragonHead = null;

        public BulletParticleEmitterComponent()
        {
            Type = ParticleEmitterType.GpuStatic;
            Shader = new ShaderClassSource("ParticleUpdaterBullet");
            MeshUpdate += OnMeshUpdate;
            UpdateData += UpdateParticlesData;
            UpdateSystem += OnUpdateSystem;
            ParticleElementSize = Utilities.SizeOf<ScriptParticleSmoke.ParticleData>();
            Description = new BulletEmitterDescription();
        }

        public Entity RootAnimation { get; set; }

        public override void OnAddToSystem(EntitySystem EntitySystem, RenderContextBase renderContext)
        {
            base.OnAddToSystem(EntitySystem, renderContext);
            this.EntitySystem = EntitySystem;
            this.renderContext = renderContext;
        }

        private void OnUpdateSystem(ParticleEmitterComponent particleEmitterComponent)
        {
            if (dragonHead == null)
                dragonHead = EntitySystem.Entities.FirstOrDefault(x => x.Name == "English DragonPelvis");

            var desc = Description;

            if (dragonHead != null)
            {
                var dragonTransform = dragonHead.Transformation;
                if (dragonTransform != null)
                {
                    desc.TargetOld = desc.Target;
                    desc.Target = (Vector3)dragonTransform.WorldMatrix.Row4;
                }
            }

            var animationComponent = RootAnimation.GetOrCreate(AnimationComponent.Key);
            if (animationComponent != null)
            {
                desc.AnimationTime = (float)animationComponent.CurrentTime.TotalSeconds;
                // If we reset, reupload the particle buffer
                if ((animationComponent.CurrentTime.Ticks - lastTime.Ticks) < 0) 
                    particleEmitterComponent.UpdateNextBuffer = true;

                lastTime = animationComponent.CurrentTime;
            }

            Description = desc;
        }

        private void OnMeshUpdate(ParticleEmitterComponent particleEmitterComponent, Entity entity, TrackingCollectionChangedEventArgs arg3)
        {
            if (entity.Name.StartsWith("maguma_"))
            {
                var meshComponent = entity.Get(ModelComponent.Key);
                if (meshComponent == null)
                    return;

                foreach (var effectMesh in meshComponent.SubMeshes)
                {
                    var subMeshData = effectMesh.MeshData.Value.SubMeshDatas[MeshData.StandardSubMeshData];
                    var buffer = subMeshData.GetVertexBufferData<Vector3>("POSITION");
                    var gpuBuffer = Buffer.Structured.New(renderContext.GraphicsDevice, buffer);
                    Parameters.Set(ScriptParticleSmoke.VerticesEmitterKey, gpuBuffer);
                    break;
                }
            }
        }

        private void UpdateParticlesData(ParticleEmitterComponent smokeParticleEmitterComponent)
        {
            bool isUptoDate = true;
            if (ParticleData == null || ParticleData.Length != Count)
            {
                ParticleData = new ScriptParticleSmoke.ParticleData[Count];
                isUptoDate = false;
            }

            if (isUptoDate)
                return;

            var description = Description;
            var random = new Random(0);
            var particlesBuffer = (ScriptParticleSmoke.ParticleData[])ParticleData;

            for (int i = 0; i < particlesBuffer.Length; i++)
            {
                var timeProb = (float)random.NextDouble();
                var timeStepFactor = 50 * Math.Pow(1 - timeProb, 3.0) + 1.0;
                var time = (((int)(random.NextDouble() * description.MaxTimeTarget / timeStepFactor) + 1) * timeStepFactor) % description.MaxTimeTarget;
                particlesBuffer[i] = new ScriptParticleSmoke.ParticleData
                    {
                        Time = (float)time,
                        Opacity = (Half)0.0f,
                        Factors = new Half4(Half.Zero, Half.Zero, Half.One, Half.Zero),
                        TimeStep = (Half)timeStepFactor,
                    };
            }
        }

        [DataMemberConvert]
        [Display]
        public BulletEmitterDescription Description
        {
            get
            {
                return Parameters.TryGet(ScriptParticleSmoke.BulletEmitterKey);
            }
            set
            {
                Parameters.Set(ScriptParticleSmoke.BulletEmitterKey, value);
            }
        }
    }
}