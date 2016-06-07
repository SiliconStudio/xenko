// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Xenko.Assets.Textures;
using SiliconStudio.Xenko.Rendering.Skyboxes;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Assets.Skyboxes
{
    /// <summary>
    /// A cubemap based skybox.
    /// </summary>
    [DataContract("SkyboxCubeMapModel")]
    [Display("Cubemap")]
    public class SkyboxCubeMapModel : ISkyboxModel
    {
        /// <summary>
        /// Gets or sets the cubemap texture.
        /// </summary>
        /// <value>The cubemap texture.</value>
        /// <userdoc>The cube map texture to use has skybox.</userdoc>
        [DataMember(10)]
        public Texture CubeMap { get; set; }

        public ShaderSource Generate(SkyboxGeneratorContext context)
        {
            // If the skybox is only used for lighting, don't generate a shader for the background
            if (context.Skybox.Usage == SkyboxUsage.Lighting)
            {
                return null;
            }

            var key = context.GetTextureKey(CubeMap, SkyboxKeys.CubeMap);
            return new ShaderClassSource("ComputeSkyboxCubeMapColor", key);
        }

        public IEnumerable<IReference> GetDependencies()
        {
            if (CubeMap != null)
            {
                var reference = AttachedReferenceManager.GetAttachedReference(CubeMap);
                yield return new AssetReference<TextureAsset>(reference.Id, reference.Url);
            }
        }
    }
}
