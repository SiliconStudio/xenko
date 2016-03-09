// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Xenko.Rendering.Images
{
    /// <summary>
    /// Parameter keys used by <see cref="ColorTransformBase"/>
    /// </summary>
    internal static class ColorTransformKeys
    {
        /// <summary>
        /// A boolean indicating wheter a <see cref="ColorTransformBase"/> is active or not.
        /// </summary>
        public static readonly PermutationParameterKey<bool> Enabled = ParameterKeys.NewPermutation(true);

        /// <summary>
        /// The shader used by <see cref="ColorTransformBase"/>.
        /// </summary>
        public static readonly PermutationParameterKey<string> Shader = ParameterKeys.NewPermutation("ColorTransformShader");

        /// <summary>
        /// The shader used by <see cref="ColorTransformBase"/>.
        /// </summary>
        public static readonly PermutationParameterKey<object[]> GenericArguments = ParameterKeys.NewPermutation((object[])null);
    }
}