// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Materials.Processor.Visitors;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Materials.ComputeColors
{
    /// <summary>
    /// Base class for shader class node.
    /// </summary>
    /// <typeparam name="T">Type of the node (scalar or color)</typeparam>
    [DataContract(Inherited = true)]
    [Display("Shader")]
    public abstract class ComputeShaderClassBase<T> : ComputeNode where T : class, IComputeNode
    {
        #region Public properties

        //TODO: use typed AssetReferences
        /// <summary>
        /// The shader.
        /// </summary>
        /// <userdoc>
        /// The shader used in this node. It should be a ComputeColor.
        /// </userdoc>
        [DataMember(10)]
        [InlineProperty]
        public string MixinReference
        {
            get
            {
                return mixinReference;
            }
            set
            {
                mixinReference = value;
            }
        }

        /// <summary>
        /// The generics of this class.
        /// </summary>
        /// <userdoc>
        /// The generics of the shader. There is no need to edit the list, it is automatically filled when the shader is loaded.
        /// </userdoc>
        [DataMember(30)]
        [MemberCollection(ReadOnly = true)]
        public ComputeColorParameters Generics { get; set; }
        
        /// <summary>
        /// The compositions of this class.
        /// </summary>
        /// <userdoc>
        /// The compositions of the shader where material nodes can be attached. There is no need to edit the list, it is automatically filled when the shader is loaded.
        /// </userdoc>
        [DataMember(40)]
        [MemberCollection(ReadOnly = true)]
        public Dictionary<string, T> CompositionNodes { get; set; }

        /// <summary>
        /// The members of this class.
        /// </summary>
        /// <userdoc>
        /// The editables values of this shader. There is no need to edit the list, it is automatically filled when the shader is loaded.
        /// </userdoc>
        [DataMemberIgnore]
        [MemberCollection(ReadOnly = true)]
        public Dictionary<ParameterKey, object> Members { get; set; }

        #endregion

        #region Private members

        /// <summary>
        /// The reference to the shader.
        /// </summary>
        [DataMemberIgnore]
        private string mixinReference;

        #endregion

        #region Constructor & public methods

        protected ComputeShaderClassBase()
        {
            Generics = new ComputeColorParameters();
            CompositionNodes = new Dictionary<string, T>();
            Members = new Dictionary<ParameterKey, object>();
        }

        /// <inheritdoc/>
        public override IEnumerable<IComputeNode> GetChildren(object context = null)
        {
            foreach (var composition in CompositionNodes)
            {
                if (composition.Value != null)
                {
                    yield return composition.Value;
                }
            }

            var materialContext = context as MaterialGeneratorContext;
            if (materialContext != null)
            {
                foreach (var gen in Generics)
                {
                    if (gen.Value is ComputeColorParameterTexture)
                    {
                        var foundNode = ((ComputeColorParameterTexture)gen.Value).Texture;
                        if (foundNode != null) 
                            yield return foundNode;
                    }
                }
            }
        }

        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            if (string.IsNullOrEmpty(MixinReference))
                return new ShaderClassSource("ComputeColor");
            var mixinName = MixinReference;

            object[] generics = null;
            if (Generics.Count > 0)
            {
                // TODO: correct generic order
                var mixinGenerics = new List<object>();
                foreach (var genericKey in Generics.Keys)
                {
                    var generic = Generics[genericKey];
                    if (generic is ComputeColorParameterTexture)
                    {
                        var textureParameter = ((ComputeColorParameterTexture)generic);
                        var textureKey = context.GetTextureKey(textureParameter.Texture, baseKeys);
                        mixinGenerics.Add(textureKey.ToString());
                    }
                    else if (generic is ComputeColorParameterSampler)
                    {
                        var pk = context.GetSamplerKey((ComputeColorParameterSampler)generic);
                        mixinGenerics.Add(pk.ToString());
                    }
                    else if (generic is ComputeColorParameterFloat)
                        mixinGenerics.Add(((ComputeColorParameterFloat)generic).Value.ToString(CultureInfo.InvariantCulture));
                    else if (generic is ComputeColorParameterInt)
                        mixinGenerics.Add(((ComputeColorParameterInt)generic).Value.ToString(CultureInfo.InvariantCulture));
                    else if (generic is ComputeColorParameterFloat2)
                        mixinGenerics.Add(MaterialUtility.GetAsShaderString(((ComputeColorParameterFloat2)generic).Value));
                    else if (generic is ComputeColorParameterFloat3)
                        mixinGenerics.Add(MaterialUtility.GetAsShaderString(((ComputeColorParameterFloat3)generic).Value));
                    else if (generic is ComputeColorParameterFloat4)
                        mixinGenerics.Add(MaterialUtility.GetAsShaderString(((ComputeColorParameterFloat4)generic).Value));
                    else if (generic is ComputeColorStringParameter)
                        mixinGenerics.Add(((ComputeColorStringParameter)generic).Value);
                    else
                        throw new Exception("[Material] Unknown node type: " + generic.GetType());
                }
                generics = mixinGenerics.ToArray();
            }

            var shaderClassSource = new ShaderClassSource(mixinName, generics);

            if (CompositionNodes.Count == 0)
                return shaderClassSource;

            var mixin = new ShaderMixinSource();
            mixin.Mixins.Add(shaderClassSource);

            foreach (var comp in CompositionNodes)
            {
                if (comp.Value != null)
                {
                    var compShader = comp.Value.GenerateShaderSource(context, baseKeys);
                    if (compShader != null)
                        mixin.Compositions.Add(comp.Key, compShader);
                }
            }

            return mixin;
        }



        public ParameterCollection GetParameters(object context)
        {
            var collection = new ParameterCollection();

            if (MixinReference != null)
            {
                foreach (var keyValue in Members)
                {
                    var expectedType = keyValue.Value.GetType();
                    if (expectedType == typeof(Color4))
                    {
                        AddToCollection<Color4>(keyValue.Key, (Color4)keyValue.Value, collection);
                    }
                    else if (expectedType == typeof(float))
                    {
                        AddToCollection<float>(keyValue.Key, (float)keyValue.Value, collection);
                    }
                    else if (expectedType == typeof(double))
                    {
                        AddToCollection<double>(keyValue.Key, (double)keyValue.Value, collection);
                    }
                    else if (expectedType == typeof(int))
                    {
                        AddToCollection<int>(keyValue.Key, (int)keyValue.Value, collection);
                    }
                    else if (expectedType == typeof(uint))
                    {
                        AddToCollection<uint>(keyValue.Key, (uint)keyValue.Value, collection);
                    }
                    else if (expectedType == typeof(bool))
                    {
                        AddToCollection<bool>(keyValue.Key, (bool)keyValue.Value, collection);
                    }
                    else if (expectedType == typeof(Vector2))
                    {
                        AddToCollection<Vector2>(keyValue.Key, (Vector2)keyValue.Value, collection);
                    }
                    else if (expectedType == typeof(Vector3))
                    {
                        AddToCollection<Vector3>(keyValue.Key, (Vector3)keyValue.Value, collection);
                    }
                    else if (expectedType == typeof(Vector4))
                    {
                        AddToCollection<Vector4>(keyValue.Key, (Vector4)keyValue.Value, collection);
                    }
                    else if (expectedType == typeof(ComputeColorParameterTexture))
                    {
                        var matContext = context as MaterialGeneratorContext;
                        if (matContext != null)
                        {
                            var textureNode = ((ComputeColorParameterTexture)keyValue.Value).Texture;
                            if (textureNode != null)
                                throw new NotImplementedException();
                            //AddToCollection<Graphics.Texture>(keyValue.Key, textureNode, collection);
                        }
                    }
                    else if (expectedType == typeof(ComputeColorParameterSampler))
                    {
                        throw new NotImplementedException();
                        //AddToCollection<Graphics.SamplerState>(keyValue.Key, keyValue.Value, collection);
                    }
                }
            }
            return collection;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "Shader";
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Add a new member.
        /// </summary>
        /// <typeparam name="TMember">The type of the member.</typeparam>
        /// <param name="linkName">The name of the parameter key.</param>
        /// <param name="members">The target parameter collection.</param>
        public void AddMember<TMember>(string linkName, Dictionary<ParameterKey, object> members)
        {
            var pk = GetTypedParameterKey<TMember>(linkName);
            if (pk != null)
            {
                Type expectedType = null;
                object defaultValue;
                if (pk.PropertyType == typeof(Graphics.Texture))
                {
                    expectedType = typeof(ComputeColorParameterTexture);
                    defaultValue = new ComputeColorParameterTexture();
                }
                else if (pk.PropertyType == typeof(Graphics.SamplerState))
                {
                    expectedType = typeof(ComputeColorParameterSampler);
                    defaultValue = new ComputeColorParameterSampler();
                }
                else
                {
                    expectedType = pk.PropertyType;
                    defaultValue = pk.DefaultValueMetadataT.DefaultValue;
                }

                if (Members.ContainsKey(pk))
                {
                    var value = Members[pk];
                    if (value.GetType() == expectedType)
                        members.Add(pk, value);
                }
                else
                    members.Add(pk, defaultValue);
            }
        }

        /// <summary>
        /// Add a new generic parameter.
        /// </summary>
        /// <typeparam name="TValue">The type of the generic.</typeparam>
        /// <param name="keyName">The name of the generic.</param>
        /// <param name="generics">The target ComputeColorParameters.</param>
        public void AddKey<TValue>(string keyName, ComputeColorParameters generics)
        {
            IComputeColorParameter computeColorParameter;
            var typeT = typeof(TValue);
            if (typeT == typeof(Texture))
                computeColorParameter = new ComputeColorParameterTexture();
            else if (typeT == typeof(float))
                computeColorParameter = new ComputeColorParameterFloat();
            else if (typeT == typeof(int))
                computeColorParameter = new ComputeColorParameterInt();
            else if (typeT == typeof(Vector2))
                computeColorParameter = new ComputeColorParameterFloat2();
            else if (typeT == typeof(Vector3))
                computeColorParameter = new ComputeColorParameterFloat3();
            else if (typeT == typeof(Vector4))
                computeColorParameter = new ComputeColorParameterFloat4();
            else if (typeT == typeof(SamplerState))
                computeColorParameter = new ComputeColorParameterSampler();
            else
                throw new Exception("Unsupported generic format");

            if (Generics.ContainsKey(keyName))
            {
                var gen = Generics[keyName];
                if (gen == null || gen.GetType() != computeColorParameter.GetType())
                    generics[keyName] = computeColorParameter;
                else
                    generics[keyName] = gen;
            }
            else
                generics.Add(keyName, computeColorParameter);
        }

        /// <summary>
        /// Add the parameter to the collection.
        /// </summary>
        /// <typeparam name="TValue">The type of the parameter.</typeparam>
        /// <param name="key">The key of the variable.</param>
        /// <param name="value"></param>
        /// <param name="collection"></param>
        private void AddToCollection<TValue>(ParameterKey key, TValue value, ParameterCollection collection) where TValue : struct
        {
            var pk = key as ValueParameterKey<TValue>;
            if (pk != null)
                collection.Set(pk, value);
        }

        #endregion

        #region Private static methods

        /// <summary>
        /// Get the correct parameter key.
        /// </summary>
        /// <typeparam name="TValue">The type of the parameter.</typeparam>
        /// <param name="linkName">The name of the parameter key.</param>
        /// <returns>The parameter key.</returns>
        private static ParameterKey<TValue> GetTypedParameterKey<TValue>(string linkName)
        {
            var pk = ParameterKeys.FindByName(linkName);
            if (pk != null)
            {
                if (pk.PropertyType == typeof(TValue))
                    return (ParameterKey<TValue>)pk;
            }
            //TODO: log error
            return null;
        }

        #endregion
    }
}