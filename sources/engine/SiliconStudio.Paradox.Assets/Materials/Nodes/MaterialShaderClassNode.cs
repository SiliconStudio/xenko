// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Paradox.Assets.Effect;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.Data;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Shaders.Parser.Ast;
using SiliconStudio.Shaders.Ast;

namespace SiliconStudio.Paradox.Assets.Materials.Nodes
{
    [ContentSerializer(typeof(DataContentSerializer<MaterialShaderClassNode>))]
    [DataContract("MaterialShaderClassNode")]
    public class MaterialShaderClassNode : MaterialNodeBase
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
        public GenericDictionary Generics { get; set; }
        
        /// <summary>
        /// The compositions of this class.
        /// </summary>
        /// <userdoc>
        /// The compositions of the shader where material nodes can be attached. There is no need to edit the list, it is automatically filled when the shader is loaded.
        /// </userdoc>
        [DataMember(40)]
        public Dictionary<string, IMaterialNode> CompositionNodes { get; set; }

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
        
        public MaterialShaderClassNode()
            : base()
        {
            Generics = new GenericDictionary();
            CompositionNodes = new Dictionary<string, IMaterialNode>();
            Members = new Dictionary<ParameterKey, object>();
        }

        /// <inheritdoc/>
        public override IEnumerable<MaterialNodeEntry> GetChildren(object context = null)
        {
            foreach (var composition in CompositionNodes)
            {
                if (composition.Value != null)
                {
                    KeyValuePair<string, IMaterialNode> composition1 = composition;
                    yield return new MaterialNodeEntry(composition.Value, node => CompositionNodes[composition1.Key] = node);
                }
            }

            var materialContext = context as MaterialContext;
            if (materialContext != null && materialContext.ExploreGenerics)
            {
                foreach (var gen in Generics)
                {
                    if (gen.Value is NodeParameterTexture)
                    {
                        var foundNode = materialContext.Material.FindNode(((NodeParameterTexture)gen.Value).Reference);
                        if (foundNode != null) 
                            yield return new MaterialNodeEntry(foundNode, node => { }); // TODO: change the callback
                    }
                }
            }
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

            var newGenerics = new GenericDictionary();
            var newCompositionNodes = new Dictionary<string, IMaterialNode>();
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

        public ParameterCollectionData GetParameters(object context)
        {
            var collection = new ParameterCollectionData();

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
                    else if (expectedType == typeof(NodeParameterTexture))
                    {
                        var matContext = context as MaterialContext;
                        if (matContext != null)
                        {
                            var textureNode = matContext.Material.FindNode(((NodeParameterTexture)keyValue.Value).Reference);
                            if (textureNode != null)
                                AddToCollection<Graphics.Texture>(keyValue.Key, textureNode, collection);
                        }
                    }
                    else if (expectedType == typeof(NodeParameterSampler))
                    {
                        AddToCollection<Graphics.SamplerState>(keyValue.Key, keyValue.Value, collection);
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
                    expectedType = typeof(NodeParameterTexture);
                    defaultValue = new NodeParameterTexture();
                }
                else if (pk.PropertyType == typeof(Graphics.SamplerState))
                {
                    expectedType = typeof(NodeParameterSampler);
                    defaultValue = new NodeParameterSampler();
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
        /// <param name="generics">The target GenericDictionary.</param>
        private void AddKey<T>(string keyName, GenericDictionary generics)
        {
            INodeParameter nodeParameter;
            var typeT = typeof(T);
            if (typeT == typeof(string))
                nodeParameter = new NodeParameter();
            else if (typeT == typeof(Graphics.Texture))
                nodeParameter = new NodeParameterTexture();
            else if (typeT == typeof(float))
                nodeParameter = new NodeParameterFloat();
            else if (typeT == typeof(int))
                nodeParameter = new NodeParameterInt();
            else if (typeT == typeof(Vector2))
                nodeParameter = new NodeParameterFloat2();
            else if (typeT == typeof(Vector3))
                nodeParameter = new NodeParameterFloat3();
            else if (typeT == typeof(Vector4))
                nodeParameter = new NodeParameterFloat4();
            else if (typeT == typeof(SamplerState))
                nodeParameter = new NodeParameterSampler();
            else
                throw new Exception("Unsupported generic format");

            if (Generics.ContainsKey(keyName))
            {
                var gen = Generics[keyName];
                if (gen == null || gen.GetType() != nodeParameter.GetType())
                    generics[keyName] = nodeParameter;
                else
                    generics[keyName] = gen;
            }
            else
                generics.Add(keyName, nodeParameter);
        }

        /// <summary>
        /// Add the parameter to the collection.
        /// </summary>
        /// <typeparam name="T">The type of the parameter.</typeparam>
        /// <param name="key">The key of the variable.</param>
        /// <param name="value"></param>
        /// <param name="collection"></param>
        private void AddToCollection<T>(ParameterKey key, object value, ParameterCollectionData collection)
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
