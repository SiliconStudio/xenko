// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;

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
    /// A class handling the allocation of shadow maps with a fixed budget of shadow map textures. This class is meant to be inherited with the desired budget since it has no texture at all.
    /// </summary>
    // TODO: rewrite this class
    public class LightShadowProcessorWithBudget : LightShadowProcessor
    {
        #region Private members

        private readonly List<ShadowMapTexture> shadowMapDefaultTextures;

        private readonly List<ShadowMapTexture> shadowMapVsmTextures;

        private readonly Dictionary<ShadowMapTexture, int> texturesDefault;

        private readonly Dictionary<ShadowMapTexture, int> texturesVsm;

        private readonly List<EntityLightShadow> activeLightShadowMaps;

        private bool shadowMapFullWarningDone = false;

        #endregion

        #region Constructor

        public LightShadowProcessorWithBudget(GraphicsDevice device, bool manageShadows)
            : base(device, manageShadows)
        {
            shadowMapDefaultTextures = new List<ShadowMapTexture>();
            shadowMapVsmTextures = new List<ShadowMapTexture>();
            texturesDefault = new Dictionary<ShadowMapTexture, int>();
            texturesVsm = new Dictionary<ShadowMapTexture, int>();

            activeLightShadowMaps = new List<EntityLightShadow>();
        }

        #endregion

        #region Public methods

        /// <inheritdoc/>
        public override void Update(GameTime time)
        {
            base.Update(time);
            if (ManageShadows)
            {
                // clear the virtual allocation
                foreach (var texture in shadowMapDefaultTextures)
                {
                    texture.GuillotinePacker.Clear(texture.ShadowMapDepthTexture.ViewWidth, texture.ShadowMapDepthTexture.ViewHeight);
                    texturesDefault[texture] = texture.ShadowMapDepthTexture.ViewWidth * texture.ShadowMapDepthTexture.ViewHeight;
                }
                foreach (var texture in shadowMapVsmTextures)
                {
                    texture.GuillotinePacker.Clear(texture.ShadowMapDepthTexture.ViewWidth, texture.ShadowMapDepthTexture.ViewHeight);
                    texturesVsm[texture] = texture.ShadowMapDepthTexture.ViewWidth * texture.ShadowMapDepthTexture.ViewHeight;
                }

                // sort the textures based on the available size.
                shadowMapDefaultTextures.Sort(ShadowMapTextureComparerDefault);
                shadowMapVsmTextures.Sort(ShadowMapTextureComparerVsm);

                // create shadow maps for new lights
                foreach (var light in Lights)
                {
                    // create new shadow maps
                    if (light.Value.Light.Shadow != null && light.Value.ShadowMap == null)
                        CreateShadowMap(light.Value);

                    // remove shadow maps
                    if ((light.Value.Light.Shadow == null || !light.Value.Light.Shadow.Enabled) && light.Value.ShadowMap != null)
                        RemoveShadowMap(light.Value);
                }

                FillActiveLightShadowMaps();

                InternalActiveShadowMaps.Clear();
                InternalActiveShadowMapTextures.Clear();

                foreach (var light in activeLightShadowMaps)
                {
                    var shadowMap = (LightShadowMap)light.Light.Shadow;

                    if (shadowMap.FilterType == LightShadowMapFilterType.Variance)
                    {
                        // if it was inserted, sort the shadow maps
                        if (ChooseShadowMapTexture(light, shadowMapVsmTextures, texturesVsm))
                        {
                            shadowMapVsmTextures.Sort(ShadowMapTextureComparerVsm);
                            InternalActiveShadowMaps.Add(light.ShadowMap);
                            InternalActiveShadowMapTextures.Add(light.ShadowMap.Texture);
                        }
                    }
                    else
                    {
                        // if it was inserted, sort the shadow maps
                        if (ChooseShadowMapTexture(light, shadowMapDefaultTextures, texturesDefault))
                        {
                            shadowMapDefaultTextures.Sort(ShadowMapTextureComparerDefault);
                            InternalActiveShadowMaps.Add(light.ShadowMap);
                            InternalActiveShadowMapTextures.Add(light.ShadowMap.Texture);
                        }
                    }
                }

                // update shadow map infos
                foreach (var light in activeLightShadowMaps)
                    UpdateEntityLightShadow(light);

                activeLightShadowMaps.Clear();

                // clear the virtual allocation again
                foreach (var texture in shadowMapDefaultTextures)
                    texture.GuillotinePacker.Clear(texture.ShadowMapDepthTexture.ViewWidth, texture.ShadowMapDepthTexture.ViewHeight);
                foreach (var texture in shadowMapVsmTextures)
                    texture.GuillotinePacker.Clear(texture.ShadowMapDepthTexture.ViewWidth, texture.ShadowMapDepthTexture.ViewHeight);
            }
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Adds the shadow map texture to the budget of textures.
        /// </summary>
        /// <param name="shadowMapTexture">The shadow map texture.</param>
        /// <param name="filterType">The filtering that will be applied to this shadow.</param>
        protected void AddShadowMapTexture(ShadowMapTexture shadowMapTexture, LightShadowMapFilterType filterType)
        {
            if (filterType == LightShadowMapFilterType.Variance)
            {
                texturesVsm.Add(shadowMapTexture, shadowMapTexture.ShadowMapDepthTexture.ViewWidth * shadowMapTexture.ShadowMapDepthTexture.ViewHeight);
                shadowMapVsmTextures.Add(shadowMapTexture);
            }
            else
            {
                texturesDefault.Add(shadowMapTexture, shadowMapTexture.ShadowMapDepthTexture.ViewWidth * shadowMapTexture.ShadowMapDepthTexture.ViewHeight);
                shadowMapDefaultTextures.Add(shadowMapTexture);
            }

            InternalShadowMapTextures.Add(shadowMapTexture);
        }

        /// <inheritdoc/>
        protected override void OnSystemRemove()
        {
            texturesDefault.Clear();
            texturesVsm.Clear();
            shadowMapDefaultTextures.Clear();
            shadowMapVsmTextures.Clear();
            activeLightShadowMaps.Clear();

            base.OnSystemRemove();
        }

        /// <inheritdoc/>
        protected override void OnEntityRemoved(Entity entity, EntityLightShadow data)
        {
            if (ManageShadows && data.ShadowMap != null)
                RemoveShadowMap(data);
            base.OnEntityRemoved(entity, data);
        }
        
        protected override void CreateShadowMap(EntityLightShadow light)
        {
            var shadowMapDesc = (LightShadowMap)light.Light.Shadow;

            // create the shadow map
            var shadowMap = new ShadowMap
            {
                LightDirection = light.Light.Direction,
                ShadowMapSize = shadowMapDesc.MinSize,
                ShadowNearDistance = shadowMapDesc.NearDistance,
                ShadowFarDistance = shadowMapDesc.FarDistance,
                CascadeCount = shadowMapDesc.CascadeCount,
                Filter = shadowMapDesc.FilterType,
                Layers = light.Light.Layers
            };

            InternalShadowMaps.Add(shadowMap);
            light.ShadowMap = shadowMap;
        }

        #endregion

        #region Private methods

        private void FillActiveLightShadowMaps()
        {
            foreach (var light in Lights)
            {
                // TODO: handle shadow maps that does no require to be updated like static shadow maps.
                if (light.Value.Light.Enabled && light.Value.Light.Shadow != null && light.Value.Light.Shadow.Enabled && light.Value.ShadowMap != null && light.Value.ShadowMap.Update)
                    activeLightShadowMaps.Add(light.Value);
            }

            // sort the maps
            activeLightShadowMaps.Sort(CompareShadows);
        }

        private void RemoveShadowMap(EntityLightShadow data)
        {
            InternalShadowMaps.Remove(data.ShadowMap);
            InternalActiveShadowMaps.Remove(data.ShadowMap);
            data.ShadowMap = null;
        }

        private bool ChooseShadowMapTexture(EntityLightShadow light, List<ShadowMapTexture> shadowMapTextures, Dictionary<ShadowMapTexture, int> shadowMapRemainingSize)
        {
            var shadowMap = light.ShadowMap;
            var shadowMapDesc = (LightShadowMap)light.Light.Shadow;
            var shadowMapSize = shadowMapDesc.MaxSize;
            // find best texture
            while (shadowMapSize > 0)
            {
                if (shadowMapSize < shadowMapDesc.MinSize)
                    shadowMapSize = shadowMapDesc.MinSize;

                foreach (var shadowMapTexture in shadowMapTextures)
                {
                    if (shadowMapTexture.GuillotinePacker.TryInsert(shadowMapSize, shadowMapSize, shadowMap.CascadeCount))
                    {
                        shadowMap.Texture = shadowMapTexture;
                        shadowMapRemainingSize[shadowMapTexture] = shadowMapRemainingSize[shadowMapTexture] - (shadowMap.CascadeCount * shadowMapSize * shadowMapSize);
                        shadowMap.ShadowMapSize = shadowMapSize;
                        return true;
                    }
                }

                if (shadowMapSize == shadowMapDesc.MinSize)
                    break;
                shadowMapSize /= 2;
            }

            // Issue a warning only once
            if (!shadowMapFullWarningDone)
            {
                shadowMapFullWarningDone = true;
                Logger.Warning("Unable to find a texture to create the shadow map.");
            }

            shadowMap.Texture = null;
            return false;
        }

        private int ShadowMapTextureComparerDefault(ShadowMapTexture texture0, ShadowMapTexture texture1)
        {
            return texturesDefault[texture0] - texturesDefault[texture1];
        }

        private int ShadowMapTextureComparerVsm(ShadowMapTexture texture0, ShadowMapTexture texture1)
        {
            return texturesVsm[texture0] - texturesVsm[texture1];
        }

        #endregion

        #region Private static methods

        // returns shadow0 - shadow1
        private static int CompareShadows(EntityLightShadow shadow0, EntityLightShadow shadow1)
        {
            var lightTypeComparaison = GetLightTypeValue(shadow0.Light.Type) - GetLightTypeValue(shadow1.Light.Type);
            if (lightTypeComparaison != 0)
                return lightTypeComparaison;

            // TODO: Shadow light comparison

            //var shadowMapSizeDiff = shadow0.Light.ShadowMapMaxSize - shadow1.Light.ShadowMapMaxSize;
            //if (shadowMapSizeDiff > 0)
            //    return -1;
            //if (shadowMapSizeDiff < 0)
            //    return 1;

            // TODO: more comparisons

            return 0;
        }

        private static int GetLightTypeValue(ILight lightType)
        {
            // TODO: Use comparison operator on ILight

            //switch (lightType)
            //{
            //    case LightType.Point:
            //        return 3;
                //case LightType.Spherical:
                //    return 4;
            if (lightType is LightDirectional)
                return 0;
            if (lightType is LightSpot)
                return 1;
            //    default:
            //        throw new ArgumentOutOfRangeException("lightType");
            //}
            return 0;
        }

        #endregion
    }
}
