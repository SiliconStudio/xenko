// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Threading.Tasks;

using SiliconStudio.Xenko;
using SiliconStudio.Xenko.DataModel;
using SiliconStudio.Xenko.Effects;
using SiliconStudio.Xenko.Effects;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.EntityModel;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Data;
using SiliconStudio.Xenko.Games.Mathematics;
using SiliconStudio.Xenko.Games.MicroThreading;
using SiliconStudio.Xenko.Games.Serialization;
using SiliconStudio.Xenko.Games.Serialization.Contents;
using SiliconStudio.Xenko.Games.IO;
using SiliconStudio.Xenko.Prefabs;
using SiliconStudio.Xenko.Games.Serialization.Serializers;

namespace ScriptTest2
{
    [ContentSerializer(typeof(EntityComponentContentSerializer<ModelConverterComponent>))]
    public class ModelConverterComponent : EntityComponent
    {
        public static PropertyKey<ModelConverterComponent> Key = new PropertyKey<ModelConverterComponent>("Key", typeof(ModelConverterComponent));

        [DataMemberConvert]
        public string Model { get; set; }
        public EffectOld Effect { get; set; }
    }

    public class ModelConverterProcessor : EntityProcessor<ModelConverterComponent>
    {
        private EngineContext engineContext;

        public ModelConverterProcessor(EngineContext engineContext)
            : base(new PropertyKey[] { ModelConverterComponent.Key })
        {
            this.engineContext = engineContext;
        }

        protected override ModelConverterComponent GenerateAssociatedData(Entity entity)
        {
            return entity.Get(ModelConverterComponent.Key);
        }
    }

    [XenkoScript]
    public class AnimScript
    {
        private static EffectOld effect;

        public static async Task<Entity> LoadFBXModel(EngineContext engineContext, string url)
        {
            var characterEntityPrefab = await engineContext.AssetManager.LoadAsync<Entity>(url);
            // Prefabs
            var characterEntity = Prefab.Clone(characterEntityPrefab);

            //characterEntity.Set(TransformationComponent.Key, new TransformationTRSComponent());
            await engineContext.EntityManager.AddEntityAsync(characterEntity);

            return characterEntity;
        }

        public static async Task AnimateFBXModel(EngineContext engineContext, Entity characterEntity, float endAnimTime = 0.0f, float loopTime = 0.0f)
        {
            var animationComponent = characterEntity.GetOrCreate(AnimationComponent.Key);
            while (true)
            {
                await Scheduler.Current.NextFrame();
                if (engineContext.EntityManager.State != GameState.Saving)
                {
                    if (loopTime == 0.0f)
                    {
                        loopTime = endAnimTime;
                    }

                    var endTick = endAnimTime == 0.0f ? animationComponent.AnimationData.Duration.Ticks : (long)((double)endAnimTime * TimeSpan.TicksPerSecond);
                    var resetTick = (long)((double)loopTime * TimeSpan.TicksPerSecond);
                    animationComponent.CurrentTime = new TimeSpan(Math.Min(endTick, (engineContext.CurrentTime.Ticks % resetTick)));
                }
                else
                {
                    animationComponent.CurrentTime = new TimeSpan(0);
                }
            }
        }
    }
}
