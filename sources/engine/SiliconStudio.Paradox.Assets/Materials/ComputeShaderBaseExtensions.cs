// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Rendering.Materials;
using SiliconStudio.Paradox.Rendering.Materials.ComputeColors;
using SiliconStudio.Paradox.Shaders.Parser.Ast;
using SiliconStudio.Paradox.Shaders.Parser.Mixins;
using SiliconStudio.Shaders.Ast;
using SiliconStudio.Shaders.Utility;

namespace SiliconStudio.Paradox.Assets.Materials
{
    public static class ComputeShaderBaseExtensions
    {
        /// <summary>
        /// Load the shader and extract the information.
        /// </summary>
        public static void PrepareNode<T>(this ComputeShaderClassBase<T> _this, ConcurrentDictionary<string, string> projectShaders) where T : class, IComputeNode
        {
            // TODO: merge common keys between the previous dictionaries and the new one
            _this.Generics.Clear();
            _this.CompositionNodes.Clear();
            
            if (string.IsNullOrEmpty(_this.MixinReference))
            {
                return;
            }

            var newGenerics = new ComputeColorParameters();
            var newCompositionNodes = new Dictionary<string, T>();
            var newMembers = new Dictionary<ParameterKey, object>();

            var localMixinName = _this.MixinReference;
            ShaderClassType shader = null;

            string source;
            if (projectShaders.TryGetValue(localMixinName, out source))
            {
                var logger = new LoggerResult();
                try
                {
                    shader = ShaderLoader.ParseSource(source, logger);
                    if (logger.HasErrors)
                    {
                        logger.Messages.Clear();
                        return;
                    }
                }
                catch
                {
                    // TODO: output messages
                    return;
                }
            }

            if (shader == null)
                return;

            var acceptLinkedVariable = true;

            foreach (var generic in shader.ShaderGenerics)
            {
                if (generic.Type.Name.Text == "float4")
                    _this.AddKey<Vector4>(generic.Name.Text, newGenerics);
                else if (generic.Type.Name.Text == "float3")
                    _this.AddKey<Vector3>(generic.Name.Text, newGenerics);
                else if (generic.Type.Name.Text == "float2")
                    _this.AddKey<Vector2>(generic.Name.Text, newGenerics);
                else if (generic.Type.Name.Text == "float")
                    _this.AddKey<float>(generic.Name.Text, newGenerics);
                else if (generic.Type.Name.Text == "int")
                    _this.AddKey<int>(generic.Name.Text, newGenerics);
                else if (generic.Type.Name.Text == "Texture2D")
                    _this.AddKey<Graphics.Texture>(generic.Name.Text, newGenerics);
                else if (generic.Type.Name.Text == "SamplerState")
                    _this.AddKey<SamplerState>(generic.Name.Text, newGenerics);
                else
                    _this.AddKey<string>(generic.Name.Text, newGenerics);

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
                        if (_this.CompositionNodes.ContainsKey(member.Name.Text))
                            newCompositionNodes.Add(member.Name.Text, _this.CompositionNodes[member.Name.Text]);
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
                            _this.AddMember<Color4>(linkName, newMembers);
                        }
                        else if (memberType == ScalarType.Float || memberType == ScalarType.Half)
                        {
                            _this.AddMember<float>(linkName, newMembers);
                        }
                        else if (memberType == ScalarType.Double)
                        {
                            _this.AddMember<double>(linkName, newMembers);
                        }
                        else if (memberType == ScalarType.Int)
                        {
                            _this.AddMember<int>(linkName, newMembers);
                        }
                        else if (memberType == ScalarType.UInt)
                        {
                            _this.AddMember<uint>(linkName, newMembers);
                        }
                        else if (memberType == ScalarType.Bool)
                        {
                            _this.AddMember<bool>(linkName, newMembers);
                        }
                        else if (memberType is VectorType)
                        {
                            switch (((VectorType)memberType).Dimension)
                            {
                                case 2:
                                    _this.AddMember<Vector2>(linkName, newMembers);
                                    break;
                                case 3:
                                    _this.AddMember<Vector3>(linkName, newMembers);
                                    break;
                                case 4:
                                    _this.AddMember<Vector4>(linkName, newMembers);
                                    break;
                            }
                        }
                        else if (member.Type.Name.Text == "Texture2D")
                        {
                            _this.AddMember<Graphics.Texture>(linkName, newMembers);
                        }
                        else if (member.Type.Name.Text == "SamplerState")
                        {
                            _this.AddMember<Graphics.SamplerState>(linkName, newMembers);
                        }
                    }
                }
            }

            _this.Generics = newGenerics;
            _this.CompositionNodes = newCompositionNodes;
            _this.Members = newMembers;
        }
    }
}