// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Assets.Effect;
using SiliconStudio.Paradox.Assets.Materials.Processor.Visitors;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Shaders;
using SiliconStudio.Paradox.Shaders.Parser.Ast;
using SiliconStudio.Shaders.Ast;

namespace SiliconStudio.Paradox.Assets.Materials.ComputeColors
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
        public AssetReference<EffectShaderAsset> MixinReference
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
        public ComputeColorParameters Generics { get; set; }
        
        /// <summary>
        /// The compositions of this class.
        /// </summary>
        /// <userdoc>
        /// The compositions of the shader where material nodes can be attached. There is no need to edit the list, it is automatically filled when the shader is loaded.
        /// </userdoc>
        [DataMember(40)]
        public Dictionary<string, T> CompositionNodes { get; set; }

        /// <summary>
        /// The members of this class.
        /// </summary>
        /// <userdoc>
        /// The editables values of this shader. There is no need to edit the list, it is automatically filled when the shader is loaded.
        /// </userdoc>
        [DataMember(50)]
        public Dictionary<ParameterKey, object> Members { get; set; }

        #endregion

        #region Private members

        /// <summary>
        /// The reference to the shader.
        /// </summary>
        [DataMemberIgnore]
        private AssetReference<EffectShaderAsset> mixinReference;

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

        public override ShaderSource GenerateShaderSource(MaterialGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            if (!MixinReference.HasLocation())
                return new ShaderClassSource("ComputeColor");
            var mixinName = Path.GetFileNameWithoutExtension(MixinReference.Location);

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

        /// <summary>
        /// Load the shader and extract the information.
        /// </summary>
        public void PrepareNode(ConcurrentDictionary<string, string> projectShaders)
        {
            if (!MixinReference.HasLocation())
            {
                return;
            }

            var newGenerics = new ComputeColorParameters();
            var newCompositionNodes = new Dictionary<string, T>();
            var newMembers = new Dictionary<ParameterKey, object>();

            var localMixinName = Path.GetFileNameWithoutExtension(MixinReference.Location);
            ShaderClassType shader;

            string source;
            if (projectShaders.TryGetValue(localMixinName, out source))
            {
                shader = MaterialNodeClassLoader.GetLoader().ParseShader(source);
            }
            else
            {
                shader = MaterialNodeClassLoader.GetLoader().GetShader(localMixinName);
            }

            if (shader == null)
                return;

            var acceptLinkedVariable = true;

            foreach (var generic in shader.ShaderGenerics)
            {
                if (generic.Type.Name.Text == "float4")
                    AddKey<Vector4>(generic.Name.Text, newGenerics);
                else if (generic.Type.Name.Text == "float3")
                    AddKey<Vector3>(generic.Name.Text, newGenerics);
                else if (generic.Type.Name.Text == "float2")
                    AddKey<Vector2>(generic.Name.Text, newGenerics);
                else if (generic.Type.Name.Text == "float")
                    AddKey<float>(generic.Name.Text, newGenerics);
                else if (generic.Type.Name.Text == "int")
                    AddKey<int>(generic.Name.Text, newGenerics);
                else if (generic.Type.Name.Text == "Texture2D")
                    AddKey<Graphics.Texture>(generic.Name.Text, newGenerics);
                else if (generic.Type.Name.Text == "SamplerState")
                    AddKey<SamplerState>(generic.Name.Text, newGenerics);
                else
                    AddKey<string>(generic.Name.Text, newGenerics);

                if (generic.Type is LinkType)
                    acceptLinkedVariable = false; // since the behavior is unpredictable, safely prevent addition of linked variable (= with Link annotation)
            }

            foreach (var member in shader.Members.OfType<Variable>())
            {
                // TODO: enough detect compositions?
                if (member.Type is TypeName && (member.Type.TypeInference == null || member.Type.TypeInference.TargetType == null))
                {
                    // ComputeColor only
                    if (member.Type.Name.Text == "ComputeColor")
                    {
                        if (CompositionNodes.ContainsKey(member.Name.Text))
                            newCompositionNodes.Add(member.Name.Text, CompositionNodes[member.Name.Text]);
                        else
                            newCompositionNodes.Add(member.Name.Text, null);
                    }
                }
                else
                {
                    var isColor = false;
                    string linkName = null;
                    var isStage = member.Qualifiers.Contains(ParadoxStorageQualifier.Stage);
                    var isStream = member.Qualifiers.Contains(ParadoxStorageQualifier.Stream);
                    foreach (var annotation in member.Attributes.OfType<SiliconStudio.Shaders.Ast.Hlsl.AttributeDeclaration>())
                    {
                        if (annotation.Name == "Color")
                            isColor = true;
                        if (acceptLinkedVariable && annotation.Name == "Link" && annotation.Parameters.Count > 0)
                            linkName = (string)annotation.Parameters[0].Value;
                    }

                    if (!isStream && (isStage || !string.IsNullOrEmpty(linkName)))
                    {
                        if (linkName == null)
                            linkName = localMixinName + "." + member.Name.Text;

                        var memberType = member.Type.ResolveType();
                        if (isColor)
                        {
                            AddMember<Color4>(linkName, newMembers);
                        }
                        else if (memberType == ScalarType.Float || memberType == ScalarType.Half)
                        {
                            AddMember<float>(linkName, newMembers);
                        }
                        else if (memberType == ScalarType.Double)
                        {
                            AddMember<double>(linkName, newMembers);
                        }
                        else if (memberType == ScalarType.Int)
                        {
                            AddMember<int>(linkName, newMembers);
                        }
                        else if (memberType == ScalarType.UInt)
                        {
                            AddMember<uint>(linkName, newMembers);
                        }
                        else if (memberType == ScalarType.Bool)
                        {
                            AddMember<bool>(linkName, newMembers);
                        }
                        else if (memberType is VectorType)
                        {
                            switch (((VectorType)memberType).Dimension)
                            {
                                case 2:
                                    AddMember<Vector2>(linkName, newMembers);
                                    break;
                                case 3:
                                    AddMember<Vector3>(linkName, newMembers);
                                    break;
                                case 4:
                                    AddMember<Vector4>(linkName, newMembers);
                                    break;
                            }
                        }
                        else if (member.Type.Name.Text == "Texture2D")
                        {
                            AddMember<Graphics.Texture>(linkName, newMembers);
                        }
                        else if (member.Type.Name.Text == "SamplerState")
                        {
                            AddMember<Graphics.SamplerState>(linkName, newMembers);
                        }
                    }
                }
            }

            Generics = newGenerics;
            CompositionNodes = newCompositionNodes;
            Members = newMembers;
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
        /// <typeparam name="T">The type of the member.</typeparam>
        /// <param name="linkName">The name of the parameter key.</param>
        /// <param name="members">The target parameter collection.</param>
        private void AddMember<T>(string linkName, Dictionary<ParameterKey, object> members)
        {
            var pk = GetTypedParameterKey<T>(linkName);
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
        /// <typeparam name="T">The type of the generic.</typeparam>
        /// <param name="keyName">The name of the generic.</param>
        /// <param name="generics">The target ComputeColorParameters.</param>
        private void AddKey<T>(string keyName, ComputeColorParameters generics)
        {
            IComputeColorParameter computeColorParameter;
            var typeT = typeof(T);
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
        /// <typeparam name="T">The type of the parameter.</typeparam>
        /// <param name="key">The key of the variable.</param>
        /// <param name="value"></param>
        /// <param name="collection"></param>
        private void AddToCollection<T>(ParameterKey key, T value, ParameterCollection collection)
        {
            var pk = key as ParameterKey<T>;
            if (pk != null)
                collection.Set(pk, value);
        }

        #endregion

        #region Private static methods

        /// <summary>
        /// Get the correct parameter key.
        /// </summary>
        /// <typeparam name="T">The type of the parameter.</typeparam>
        /// <param name="linkName">The name of the parameter key.</param>
        /// <returns>The parameter key.</returns>
        private static ParameterKey<T> GetTypedParameterKey<T>(string linkName)
        {
            var pk = ParameterKeys.FindByName(linkName);
            if (pk != null)
            {
                if (pk.PropertyType == typeof(T))
                    return (ParameterKey<T>)pk;
            }
            //TODO: log error
            return null;
        }

        #endregion
    }
}