// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Text;

using SiliconStudio.Core;
using SiliconStudio.Paradox.DataModel;
using SiliconStudio.Paradox.Effects.ShadowMaps;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Processors
{
    /// <summary>
    /// A class allocating as many shadow map as possible without any predefined memory budget.
    /// </summary>
    public class DynamicLightShadowProcessor : LightShadowProcessor
    {
        #region Private members

        private readonly Dictionary<ShadowMapTexture, List<ShadowMap>> texturesDefault;

        private readonly Dictionary<ShadowMapTexture, List<ShadowMap>> texturesVsm;

        #endregion

        #region Constructor

        public DynamicLightShadowProcessor(GraphicsDevice device, bool manageShadows)
            : base(device, manageShadows)
        {
            texturesDefault = new Dictionary<ShadowMapTexture, List<ShadowMap>>();
            texturesVsm = new Dictionary<ShadowMapTexture, List<ShadowMap>>();
        }

        #endregion

        #region Public methods

        /// <inheritdoc/>
        public override void Update(GameTime time)
        {
            base.Update(time);
            if (ManageShadows)
            {
                InternalActiveShadowMaps.Clear();
                InternalActiveShadowMapTextures.Clear();

                foreach (var light in Lights)
                {
                    // create new shadow maps
                    if (light.Value.Light.ShadowMap && light.Value.ShadowMap == null)
                        CreateShadowMap(light.Value);

                    // TODO: handle shadow maps that does no require to be updated like static shadow maps.
                    // update shadow maps info
                    if (light.Value.Light.Enabled && light.Value.Light.ShadowMap && light.Value.ShadowMap != null && light.Value.ShadowMap.Update)
                    {
                        UpdateEntityLightShadow(light.Value);
                        InternalActiveShadowMaps.Add(light.Value.ShadowMap);
                        InternalActiveShadowMapTextures.Add(light.Value.ShadowMap.Texture);
                    }
                }
            }
        }

        #endregion

        #region Protected methods

        /// <inheritdoc/>
        protected override void OnEntityRemoved(Entity entity, EntityLightShadow data)
        {
            if (ManageShadows && data.ShadowMap != null)
            {
                InternalShadowMaps.Remove(data.ShadowMap);

                List<ShadowMap> shadowMaps = null;
                if (!texturesDefault.TryGetValue(data.ShadowMap.Texture, out shadowMaps))
                    texturesVsm.TryGetValue(data.ShadowMap.Texture, out shadowMaps);

                if (shadowMaps == null)
                    throw new Exception("Untracked shadow map texture");

                shadowMaps.Remove(data.ShadowMap);

                // if no more shadow maps on this texture, delete it.
                if (shadowMaps.Count == 0)
                {
                    InternalShadowMapTextures.Remove(data.ShadowMap.Texture);
                    Utilities.Dispose(ref data.ShadowMap.Texture.ShadowMapDepthTexture);
                    Utilities.Dispose(ref data.ShadowMap.Texture.ShadowMapTargetTexture);
                    Utilities.Dispose(ref data.ShadowMap.Texture.IntermediateBlurTexture);

                    if (!texturesDefault.Remove(data.ShadowMap.Texture))
                        texturesVsm.Remove(data.ShadowMap.Texture);
                }
            }
            base.OnEntityRemoved(entity, data);
        }
        
        protected override void CreateShadowMap(EntityLightShadow light)
        {
            // create the shadow map
            var shadowMap = new ShadowMap
            {
                LightDirection = light.Light.LightDirection,
                LightPosition = light.Entity.Transformation.Translation,
                ShadowMapSize = light.Light.ShadowMapMaxSize,
                ShadowNearDistance = light.Light.ShadowNearDistance,
                ShadowFarDistance = light.Light.ShadowFarDistance,
                CascadeCount = light.Light.Type == LightType.Directional ? light.Light.ShadowMapCascadeCount : 1, // cascades are only supported for directional shadow maps
                LightType = light.Light.Type,
                Fov = light.Light.SpotFieldAngle,
                Filter = light.Light.ShadowMapFilterType,
                Layers = light.Light.Layers
            };

            // find or create the shadow map texture
            ShadowMapTexture chosenTexture = null;
            chosenTexture = AllocateOrChooseTexture(shadowMap, light.Light.ShadowMapFilterType == ShadowMapFilterType.Variance ? texturesVsm : texturesDefault);

            shadowMap.Texture = chosenTexture;
            InternalShadowMaps.Add(shadowMap);
            light.ShadowMap = shadowMap;
        }

        #endregion

        #region Private methods

        private ShadowMapTexture AllocateOrChooseTexture(ShadowMap newShadowMap, Dictionary<ShadowMapTexture, List<ShadowMap>> shadowMapTextures)
        {
            ShadowMapTexture chosenTexture = null;

            // find best texture
            foreach (var shadowMapTexture in shadowMapTextures)
            {
                var shadowTexture = shadowMapTexture.Key;
                var shadowMaps = shadowMapTexture.Value;

                shadowTexture.GuillotinePacker.Clear(shadowTexture.ShadowMapDepthTexture.ViewWidth, shadowTexture.ShadowMapDepthTexture.ViewHeight);

                var useShadowTexture = true;
                for (var i = 0; i < shadowMaps.Count && useShadowTexture; ++i)
                    useShadowTexture = shadowTexture.GuillotinePacker.TryInsert(shadowMaps[i].ShadowMapSize, shadowMaps[i].ShadowMapSize, shadowMaps[i].CascadeCount);

                useShadowTexture = useShadowTexture && shadowTexture.GuillotinePacker.TryInsert(newShadowMap.ShadowMapSize, newShadowMap.ShadowMapSize, newShadowMap.CascadeCount);

                shadowTexture.GuillotinePacker.Clear();

                if (useShadowTexture)
                {
                    chosenTexture = shadowMapTexture.Key;
                    break;
                }
            }

            if (chosenTexture == null)
            {
                // allocate a new texture
                chosenTexture = new ShadowMapTexture(GraphicsDevice, newShadowMap.Filter, 2048);
                chosenTexture.GuillotinePacker.Clear(chosenTexture.ShadowMapDepthTexture.ViewWidth, chosenTexture.ShadowMapDepthTexture.ViewHeight);

                // TODO: choose texture size based on the shadow map. For now throw exception
                if (!chosenTexture.GuillotinePacker.TryInsert(newShadowMap.ShadowMapSize, newShadowMap.ShadowMapSize, newShadowMap.CascadeCount))
                {
                    var message = new StringBuilder();
                    message.AppendFormat("Unable to allocate shadow map texture. The default size (2048 x 2048) is too small for the shadow map ({0} cascade(s) with size {1}).", newShadowMap.CascadeCount, newShadowMap.ShadowMapSize);
                    throw new Exception(message.ToString());
                }

                chosenTexture.GuillotinePacker.Clear();

                InternalShadowMapTextures.Add(chosenTexture);
                var shadowMaps = new List<ShadowMap> { newShadowMap };
                shadowMapTextures.Add(chosenTexture, shadowMaps);
            }
            else
            {
                shadowMapTextures[chosenTexture].Add(newShadowMap);
            }

            return chosenTexture;
        }

        #endregion
    }
}
