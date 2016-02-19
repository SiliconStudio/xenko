// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Defines keys associated with mesh used for compiling assets.
    /// </summary>
    public class MeshKeys
    {
        /// <summary>
        /// When compiling effect with an EffectLibraryAsset (xkfxlib), set it to true to allow permutation based on the 
        /// parameters of all meshes.
        /// </summary>
        /// TODO: allow permutation for a specific mesh
        /// <userdoc>
        /// If checked, the mesh parameters will be used to generate effects.
        /// </userdoc>
        public static readonly ValueParameterKey<bool> UseParameters = ParameterKeys.NewValue<bool>();
    }
}