// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using SiliconStudio.Xenko;
using SiliconStudio.Xenko.Effects;
//using Xenko.Mathematics;
using SiliconStudio.Xenko.Effects;
using System.Diagnostics;

using SiliconStudio.Xenko.Games;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Xenko.Games.MicroThreading;
using SiliconStudio.Xenko.Games.Mathematics;
using SiliconStudio.Xenko.Configuration;

namespace ScriptTest
{
    [XenkoScript]
    public class LightScript
    {
        struct LightInfo
        {
            public float Radius;
            public float Phase;
            public float Z;
        }

        public class Config
        {
            public Config()
            {
                AnimatedLights = true;
            }


            [XmlAttribute("animatedLights")]
            public bool AnimatedLights { get; set; }
        }

        [XenkoScript]
        public static async Task MoveLights(EngineContext engineContext)
        {
            var r = new Random(0);

            var config = AppConfig.GetConfiguration<Config>("LightScript2");

            LightingPrepassPlugin lightingPrepassPlugin;
            if (!engineContext.DataContext.RenderPassPlugins.TryGetValueCast("LightingPrepassPlugin", out lightingPrepassPlugin))
                return;

            var effectMeshGroup = new RenderPassListEnumerator();
            engineContext.RenderContext.RenderPassEnumerators.Add(effectMeshGroup);

            // Lights
            for (int i = 0; i < 1024; ++i)
            {
                var effectMesh = new EffectMesh(lightingPrepassPlugin.Lights);

                Color3 color = (Color3)Color.White;
                switch (i % 7)
                {
                    case 0: color = new Color3(0.7f, 0.0f, 0.0f); break;
                    case 1: color = new Color3(0.0f, 0.7f, 0.0f); break;
                    case 2: color = new Color3(0.0f, 0.0f, 0.7f); break;
                    case 3: color = new Color3(0.7f, 0.7f, 0.0f); break;
                    case 4: color = new Color3(0.7f, 0.0f, 0.7f); break;
                    case 5: color = new Color3(0.0f, 0.7f, 0.7f); break;
                    case 6: color = new Color3(0.7f, 0.7f, 0.7f); break;
                }
                effectMesh.Parameters.Set(LightKeys.LightRadius, 60.0f);
                effectMesh.Parameters.Set(LightKeys.LightColor, color);
                effectMesh.Parameters.Set(LightKeys.LightIntensity, 1.0f);
                effectMesh.KeepAliveBy(engineContext.SimpleComponentRegistry);

                effectMeshGroup.AddMesh(effectMesh);
            } 
            
            bool animatedLights = config.AnimatedLights;

            EffectOld effectLight = null;
            try
            {
                effectLight = engineContext.RenderContext.RenderPassPlugins.OfType<LightingPrepassPlugin>().FirstOrDefault().Lights;
            }
            catch
            {
                return;
            }

            var lightInfo = new LightInfo[effectLight != null ? effectLight.Meshes.Count : 0];
            for (int i = 0; i < lightInfo.Length; ++i)
            {
                lightInfo[i].Radius = (float)r.NextDouble() * 1000.0f + 500.0f;
                lightInfo[i].Phase = (float)r.NextDouble() * 10.0f;
                lightInfo[i].Z = (float)r.NextDouble() * 150.0f + 150.0f;
            }
            float time = 0.0f;
            var st = new Stopwatch();
            var lastTickCount = 0;

            var st2 = new Stopwatch();
            st2.Start();

            bool firstTime = true;
            while (true)
            {
                await Scheduler.NextFrame();

                time += 0.003f;

                if (lightInfo.Length > 0)
                {
                    if (animatedLights || firstTime)
                    {
                        int index = 0;
                        foreach (var mesh in effectLight.Meshes)
                        {
                            mesh.Parameters.Set(LightKeys.LightPosition, new Vector3(lightInfo[index].Radius * (float)Math.Cos(time * 3.0f + lightInfo[index].Phase), lightInfo[index].Radius * (float)Math.Sin(time * 3.0f + lightInfo[index].Phase), lightInfo[index].Z));
                            index++;
                        }

                        firstTime = false;
                    }
                }
            }
        }
    }
}
