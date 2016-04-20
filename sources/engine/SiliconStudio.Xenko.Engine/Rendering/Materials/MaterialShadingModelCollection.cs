// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Materials
{
    /// <summary>
    /// Stores a collection of shading model used by a layer and allow to compare if a layer can be blend by attributes (if
    /// shading model doesn't change) or by shading (if shading model is different).
    /// </summary>
    internal sealed class MaterialShadingModelCollection : Dictionary<Type, KeyValuePair<IMaterialShadingModelFeature, ShaderSource>>
    {
        /// <summary>
        /// Adds the specified shading model associated with source to this collection.
        /// </summary>
        /// <typeparam name="T">Type of the shading model</typeparam>
        /// <param name="shadingModel">The shading model</param>
        /// <param name="shaderSource">The shading source</param>
        public void Add<T>(T shadingModel, ShaderSource shaderSource) where T : class, IMaterialShadingModelFeature
        {
            if (shadingModel == null) throw new ArgumentNullException(nameof(shadingModel));
            if (shaderSource == null) throw new ArgumentNullException(nameof(shaderSource));

            // Check that we cannot have the same type of shading model multiple times
            if (ContainsKey(shadingModel.GetType()))
            {
                throw new InvalidOperationException($"The shading model with type [{shadingModel.GetType()}] is already added and cannot be added anymore");
            }

            this[shadingModel.GetType()] = new KeyValuePair<IMaterialShadingModelFeature, ShaderSource>(shadingModel, shaderSource);
        }

        /// <summary>
        /// Copies the shading models of this instance to the destination instance.
        /// </summary>
        /// <param name="node">The destination collection</param>
        public void CopyTo(MaterialShadingModelCollection node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            foreach (var keyValue in this)
            {
                node[keyValue.Key] = keyValue.Value;
            }
        }

        /// <summary>
        /// Compares the shading models of this instance against the specified instance.
        /// </summary>
        /// <param name="node">The shading model</param>
        /// <returns><c>true</c> if shading models are equvalent; <c>false</c> otherwise</returns>
        public bool Equals(MaterialShadingModelCollection node)
        {
            // Methods that allows to deeply compare shading models
            if (node == null || ReferenceEquals(node, this))
            {
                return true;
            }

            if (Count != node.Count)
            {
                return false;
            }

            if (Count == 0 || node.Count == 0)
            {
                return true;
            }

            // Because we expect the same number of shading models, we can perform the whole check in a single pass
            foreach (var shadingModelKeyPair in this)
            {
                KeyValuePair<IMaterialShadingModelFeature, ShaderSource> shadingModelAgainst;
                if (!node.TryGetValue(shadingModelKeyPair.Key, out shadingModelAgainst))
                {
                    return false;
                }

                // Note: this method is going to compare deeply the shading models (all implem of IMaterialShadingModelFeature)
                // and calling their respective Equals method implemented.
                if (!shadingModelKeyPair.Value.Key.Equals(shadingModelAgainst.Key))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Generates the shader for the shading models of this instance.
        /// </summary>
        /// <param name="context">The material generator context</param>
        /// <returns>An enumeration of shader sources</returns>
        public IEnumerable<ShaderSource> Generate(MaterialGeneratorContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            ShaderMixinSource mixinSourceForLightDependentShadingModel = null;

            // Process first shading models that are light dependent
            foreach (var shadingModelKeyPair in Values)
            {
                var shadingModel = shadingModelKeyPair.Key;
                if (shadingModel.IsLightDependent)
                {
                    context.Material.IsLightDependent = true;

                    if (mixinSourceForLightDependentShadingModel == null)
                    {
                        mixinSourceForLightDependentShadingModel = new ShaderMixinSource();
                        mixinSourceForLightDependentShadingModel.Mixins.Add(new ShaderClassSource("MaterialSurfaceLightingAndShading"));
                    }
                    mixinSourceForLightDependentShadingModel.AddCompositionToArray("surfaces", shadingModelKeyPair.Value);
                }
            }
            if (mixinSourceForLightDependentShadingModel != null)
            {
                yield return mixinSourceForLightDependentShadingModel;
            }

            // Then process shading models that are light independent
            foreach (var shadingSource in Values.Where(keyPair => !keyPair.Key.IsLightDependent).Select(keyPair => keyPair.Value))
            {
                yield return shadingSource;
            }
        }
    }
}