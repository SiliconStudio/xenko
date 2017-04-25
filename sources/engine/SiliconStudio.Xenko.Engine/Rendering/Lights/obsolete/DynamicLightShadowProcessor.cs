// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using System.Text;

using SiliconStudio.Core;
using SiliconStudio.Xenko.DataModel;
using SiliconStudio.Xenko.Effects.Lights;
using SiliconStudio.Xenko.Effects.Shadows;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.EntityModel;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Effects.Processors
{
    /// <summary>
    /// A class allocating as many shadow map as possible without any predefined memory budget.
    /// </summary>
    // TODO: rewrite this class
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
                    if (light.Value.Light.Shadow != null)
                        CreateShadowMap(light.Value);

                    // TODO: handle shadow maps that does no require to be updated like static shadow maps.
                    // update shadow maps info
                    if (light.Value.Light.Enabled && light.Value.Light.Shadow != null && light.Value.Light.Shadow.Enabled && light.Value.ShadowMap.Update)
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
            var shadowMapDesc = light.Light.Shadow as LightShadowMap;


            // create the shadow map
            var shadowMap = new ShadowMap
            {
                LightDirection = light.Light.Direction,
                LightPosition = light.Entity.Transformation.Translation,
                ShadowMapSize = shadowMapDesc.MaxSize,
                ShadowNearDistance = shadowMapDesc.NearDistance,
                ShadowFarDistance = shadowMapDesc.FarDistance,
                CascadeCount = light.Light.Type is LightDirectional ? shadowMapDesc.CascadeCount : 1, // cascades are only supported for directional shadow maps
                //Fov = light.Light.SpotFieldAngle,
                Filter = shadowMapDesc.FilterType,
                Layers = light.Light.Layers
            };

            // find or create the shadow map texture
            ShadowMapTexture chosenTexture = null;
            chosenTexture = AllocateOrChooseTexture(shadowMap, shadowMapDesc.FilterType == LightShadowMapFilterType.Variance ? texturesVsm : texturesDefault);

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
