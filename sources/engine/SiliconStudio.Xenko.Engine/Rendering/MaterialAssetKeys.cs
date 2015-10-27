// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Paradox.Rendering
{
    /// <summary>
    /// Defines keys associated with mesh used for compiling assets.
    /// </summary>
    public sealed class MaterialAssetKeys
    {
        /// <summary>
        /// When compiling effect with an EffectLibraryAsset (pdxfxlib), set it to true to allow permutation based on the 
        /// parameters of all materials.
        /// </summary>
        /// <userdoc>
        /// If checked, the material parameters will be used to generate effects.
        /// </userdoc>
        public static readonly ParameterKey<bool> UseParameters = ParameterKeys.New<bool>();

        /// <summary>
        /// Allow material compilation without mesh.
        /// </summary>
        /// <userdoc>
        /// If checked, the materials will generate a shader even if they are not attached to a mesh.
        /// </userdoc>
        public static readonly ParameterKey<bool> GenerateShader = ParameterKeys.New<bool>();
    }
}