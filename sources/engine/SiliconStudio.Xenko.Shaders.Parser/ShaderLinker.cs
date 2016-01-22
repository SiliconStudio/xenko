// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Shaders.Parser.Ast;
using SiliconStudio.Xenko.Shaders.Parser.Mixins;
using SiliconStudio.Xenko.Shaders.Parser.Utility;
using SiliconStudio.Shaders.Ast;
using SiliconStudio.Shaders.Ast.Hlsl;
using SiliconStudio.Shaders.Visitor;
using SiliconStudio.Xenko.Graphics;

using StorageQualifier = SiliconStudio.Shaders.Ast.StorageQualifier;

namespace SiliconStudio.Xenko.Shaders.Parser
{
    /// <summary>
    /// This AST Visitor will look for any "Link" annotation in order to bind EffectVariable to their associated HLSL variables.
    /// </summary>
    internal class ShaderLinker : ShaderVisitor
    {
        private readonly Dictionary<string, SamplerStateDescription> samplers = new Dictionary<string, SamplerStateDescription>();
        private readonly EffectReflection effectReflection;
        private readonly Dictionary<ShaderConstantBufferDescription, List<EffectParameterValueData>> valueBindings = new Dictionary<ShaderConstantBufferDescription, List<EffectParameterValueData>>();
        private readonly ShaderMixinParsingResult parsingResult;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderLinker" /> class.
        /// </summary>
        /// <param name="parsingResult">The parsing result.</param>
        public ShaderLinker(ShaderMixinParsingResult parsingResult)
            : base(true, false)
        {
            this.parsingResult = parsingResult;
            this.effectReflection = parsingResult.Reflection;
        }

        /// <summary>
        /// Gets the samplers.
        /// </summary>
        public IDictionary<string, SamplerStateDescription> Samplers
        {
            get
            {
                return samplers;
            }
        }

        /// <summary>
        /// Runs the linker on the specified Shader.
        /// </summary>
        /// <param name="shader">The shader.</param>
        public void Run(Shader shader)
        {
            PrepareConstantBuffers(shader);
            Visit(shader);
            foreach (var valueBinding in valueBindings)
            {
                valueBinding.Key.Members = valueBinding.Value.ToArray();
            }
        }

        private void PrepareConstantBuffers(Shader shader)
        {
            // Recalculate constant buffers
            // Order first all non-method declarations and then after method declarations
            var otherNodes = shader.Declarations.Where(declaration => !(declaration is MethodDeclaration) && !(declaration is Variable)).ToList();
            var declarations = new List<Node>();
            var variables = shader.Declarations.OfType<Variable>();
            var methods = shader.Declarations.OfType<MethodDeclaration>();
            var newVariables = new List<Node>();

            foreach (var variableGroup in variables)
            {
                foreach (var variable in variableGroup.Instances())
                {
                    declarations.Add(variable);
                }
            }

            declarations.AddRange(otherNodes);
            declarations.AddRange(newVariables);
            declarations.AddRange(methods);

            shader.Declarations = declarations;
        }


        /// <summary>
        /// Visits the specified variable.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <returns>The variable visited</returns>
        [Visit]
        protected void Visit(Variable variable)
        {
            var parameterKey = GetLinkParameterKey(variable);
            if (parameterKey == null) return;

            var resolvedType = variable.Type.ResolveType();
            if (resolvedType is ArrayType)
            {
                resolvedType = ((ArrayType)resolvedType).Type;
            }
            if (resolvedType is StateType)
            {
                var samplerState = SamplerStateDescription.Default;

                var stateInitializer = variable.InitialValue as StateInitializer;
                if (stateInitializer != null)
                {
                    foreach (var samplerField in stateInitializer.Items.OfType<AssignmentExpression>())
                    {
                        string key = samplerField.Target.ToString();
                        string value = samplerField.Value.ToString();

                        if (key == "Filter")
                        {
                            switch (value)
                            {
                                case "COMPARISON_MIN_MAG_LINEAR_MIP_POINT":
                                    samplerState.Filter = TextureFilter.ComparisonMinMagLinearMipPoint;
                                    break;
                                case "COMPARISON_MIN_MAG_MIP_POINT":
                                    samplerState.Filter = TextureFilter.ComparisonPoint;
                                    break;
                                case "MIN_MAG_LINEAR_MIP_POINT":
                                    samplerState.Filter = TextureFilter.MinMagLinearMipPoint;
                                    break;
                                case "MIN_MAG_MIP_LINEAR":
                                    samplerState.Filter = TextureFilter.Linear;
                                    break;
                                case "ANISOTROPIC":
                                    samplerState.Filter = TextureFilter.Anisotropic;
                                    break;
                                case "MIN_MAG_MIP_POINT":
                                    samplerState.Filter = TextureFilter.Point;
                                    break;
                                default:
                                    parsingResult.Error(XenkoMessageCode.SamplerFilterNotSupported, variable.Span, value);
                                    break;
                            }
                        }
                        else if (key == "ComparisonFunc")
                        {
                            CompareFunction compareFunction;
                            Enum.TryParse(value, true, out compareFunction);
                            samplerState.CompareFunction = compareFunction;
                        }
                        else if (key == "AddressU" || key == "AddressV" || key == "AddressW")
                        {
                            TextureAddressMode textureAddressMode;
                            Enum.TryParse(value, true, out textureAddressMode);
                            switch (key)
                            {
                                case "AddressU":
                                    samplerState.AddressU = textureAddressMode;
                                    break;
                                case "AddressV":
                                    samplerState.AddressV = textureAddressMode;
                                    break;
                                case "AddressW":
                                    samplerState.AddressW = textureAddressMode;
                                    break;
                                default:
                                    parsingResult.Error(XenkoMessageCode.SamplerAddressModeNotSupported, variable.Span, key);
                                    break;
                            }
                        }
                        else if (key == "BorderColor")
                        {
                            var borderColor = samplerField.Value as MethodInvocationExpression;
                            if (borderColor != null)
                            {
                                var targetType = borderColor.Target as TypeReferenceExpression;
                                if (targetType != null && targetType.Type.ResolveType() == VectorType.Float4 && borderColor.Arguments.Count == 4)
                                {
                                    var values = new float[4];
                                    for (int i = 0; i < 4; i++)
                                    {
                                        var argValue = borderColor.Arguments[i] as LiteralExpression;
                                        if (argValue != null)
                                        {
                                            values[i] = (float)Convert.ChangeType(argValue.Value, typeof(float));
                                        }
                                        else
                                        {
                                            parsingResult.Error(XenkoMessageCode.SamplerBorderColorNotSupported, variable.Span, borderColor.Arguments[i]);
                                        }
                                    }

                                    samplerState.BorderColor = new Color4(values);
                                }
                                else
                                {
                                    parsingResult.Error(XenkoMessageCode.SamplerBorderColorNotSupported, variable.Span, variable);
                                }
                            }
                            else
                            {
                                parsingResult.Error(XenkoMessageCode.SamplerBorderColorNotSupported, variable.Span, variable);
                            }
                        }
                        else if (key == "MinLOD")
                        {
                            samplerState.MinMipLevel = float.Parse(value);
                        }
                        else if (key == "MaxLOD")
                        {
                            samplerState.MaxMipLevel = float.Parse(value);
                        }
                        else if (key == "MaxAnisotropy")
                        {
                            samplerState.MaxAnisotropy = int.Parse(value);
                        }
                        else
                        {
                            parsingResult.Error(XenkoMessageCode.SamplerFieldNotSupported, variable.Span, variable);
                        }
                    }
                }

                effectReflection.SamplerStates.Add(new EffectSamplerStateBinding(parameterKey.Name, samplerState));
                LinkVariable(effectReflection, variable.Name, parameterKey);
            }
            else if (variable.Type is TextureType || variable.Type is GenericType)
            {
                LinkVariable(effectReflection, variable.Name, parameterKey);
            }
            else
            {
                ParseConstantBufferVariable("$Globals", variable);
            }
        }

        /// <summary>
        /// Visits the specified constant buffer.
        /// </summary>
        /// <param name="constantBuffer">The constant buffer.</param>
        /// <returns></returns>
        [Visit]
        protected void Visit(ConstantBuffer constantBuffer)
        {
            foreach (var variable in constantBuffer.Members.OfType<Variable>().SelectMany(x => x.Instances()))
            {
                ParseConstantBufferVariable(constantBuffer.Name, variable);
            }
        }

        [Visit]
        protected void Visit(MethodDefinition method)
        {
            // Parse stream output declarations (if any)
            // TODO: Currently done twice, one time in ShaderMixer, one time in ShaderLinker
            var streamOutputAttribute = method.Attributes.OfType<AttributeDeclaration>().FirstOrDefault(x => x.Name == "StreamOutput");
            if (streamOutputAttribute != null)
            {
                var rasterizedStream = streamOutputAttribute.Parameters.LastOrDefault();

                // Ignore last parameter if it's not an integer (it means there is no rasterized stream info)
                // We should make a new StreamOutputRasterizedStream attribute instead maybe?
                if (rasterizedStream != null && !(rasterizedStream.Value is int))
                    rasterizedStream = null;

                int[] streamOutputStrides;

                // Parse declarations
                // Everything should be registered in GS_OUTPUT (previous pass in ShaderMixer).
                StreamOutputParser.Parse(effectReflection.ShaderStreamOutputDeclarations, out streamOutputStrides, streamOutputAttribute, ((StructType)FindDeclaration("GS_OUTPUT")).Fields);

                effectReflection.StreamOutputStrides = streamOutputStrides;
                effectReflection.StreamOutputRasterizedStream = rasterizedStream != null ? (int)rasterizedStream.Value : -1;
            }
        }

        /// <inheritdoc/>
        [Visit]
        protected override Node Visit(Node node)
        {
            if (node is IDeclaration)
            {
                var parameterKey = this.GetLinkParameterKey(node);
                if (parameterKey != null)
                    LinkVariable(effectReflection, ((IDeclaration)node).Name, parameterKey);
            }

            node.Childrens(OnProcessor);
            return node;
        }

        private Node OnProcessor(Node nodeArg, ref NodeProcessorContext explorer)
        {
            return VisitDynamic(nodeArg);
        }

        private LocalParameterKey GetLinkParameterKey(Node node)
        {
            var qualifiers = node as IQualifiers;
            var attributable = node as IAttributes;

            if ((qualifiers != null && (qualifiers.Qualifiers.Contains(SiliconStudio.Shaders.Ast.Hlsl.StorageQualifier.Static) ||
                                        qualifiers.Qualifiers.Contains(StorageQualifier.Const) ||
                                        qualifiers.Qualifiers.Contains(SiliconStudio.Shaders.Ast.Hlsl.StorageQualifier.Groupshared)
                                       )) || attributable == null)
            {
                return null;
            }

            bool isColor = attributable.Attributes.OfType<AttributeDeclaration>().Any(x => x.Name == "Color");

            foreach (var annotation in attributable.Attributes.OfType<AttributeDeclaration>())
            {
                if (annotation.Name != "Link" || annotation.Parameters.Count < 1)
                {
                    continue;
                }

                var variableName = (string)annotation.Parameters[0].Value;
                var parameterKey = new LocalParameterKey() {Name = variableName};
                var variable = node as Variable;
                if (variable != null)
                {
                    var variableType = variable.Type;

                    var cbuffer = (ConstantBuffer)variable.GetTag(XenkoTags.ConstantBuffer);
                    if (cbuffer != null && cbuffer.Type == XenkoConstantBufferType.ResourceGroup)
                    {
                        parameterKey.ResourceGroup = cbuffer.Name;
                    }

                    if (variableType.TypeInference.TargetType != null)
                        variableType = variableType.TypeInference.TargetType;

                    if (variableType is ArrayType)
                    {
                        var arrayType = (ArrayType)variableType;
                        variableType = arrayType.Type;
                        parameterKey.Count = (int)((LiteralExpression)arrayType.Dimensions[0]).Literal.Value;

                        if (variableType.TypeInference.TargetType != null)
                            variableType = variableType.TypeInference.TargetType;
                    }
                    
                    if (variableType.IsBuiltIn)
                    {
                        var variableTypeName = variableType.Name.Text.ToLower();

                        switch (variableTypeName)
                        {
                            case "cbuffer":
                                parameterKey.Class = EffectParameterClass.ConstantBuffer;
                                parameterKey.Type = EffectParameterType.ConstantBuffer;
                                break;

                            case "tbuffer":
                                parameterKey.Class = EffectParameterClass.TextureBuffer;
                                parameterKey.Type = EffectParameterType.TextureBuffer;
                                break;

                            case "structuredbuffer":
                                parameterKey.Class = EffectParameterClass.ShaderResourceView;
                                parameterKey.Type = EffectParameterType.StructuredBuffer;
                                break;
                            case "rwstructuredbuffer":
                                parameterKey.Class = EffectParameterClass.UnorderedAccessView;
                                parameterKey.Type = EffectParameterType.RWStructuredBuffer;
                                break;
                            case "consumestructuredbuffer":
                                parameterKey.Class = EffectParameterClass.UnorderedAccessView;
                                parameterKey.Type = EffectParameterType.ConsumeStructuredBuffer;
                                break;
                            case "appendstructuredbuffer":
                                parameterKey.Class = EffectParameterClass.UnorderedAccessView;
                                parameterKey.Type = EffectParameterType.AppendStructuredBuffer;
                                break;
                            case "buffer":
                                parameterKey.Class = EffectParameterClass.ShaderResourceView;
                                parameterKey.Type = EffectParameterType.Buffer;
                                break;
                            case "rwbuffer":
                                parameterKey.Class = EffectParameterClass.UnorderedAccessView;
                                parameterKey.Type = EffectParameterType.RWBuffer;
                                break;
                            case "byteaddressbuffer":
                                parameterKey.Class = EffectParameterClass.ShaderResourceView;
                                parameterKey.Type = EffectParameterType.ByteAddressBuffer;
                                break;
                            case "rwbyteaddressbuffer":
                                parameterKey.Class = EffectParameterClass.UnorderedAccessView;
                                parameterKey.Type = EffectParameterType.RWByteAddressBuffer;
                                break;

                            case "texture1d":
                                parameterKey.Class = EffectParameterClass.ShaderResourceView;
                                parameterKey.Type = EffectParameterType.Texture1D;
                                break;

                            case "texturecube":
                                parameterKey.Class = EffectParameterClass.ShaderResourceView;
                                parameterKey.Type = EffectParameterType.TextureCube;
                                break;

                            case "texture2d":
                                parameterKey.Class = EffectParameterClass.ShaderResourceView;
                                parameterKey.Type = EffectParameterType.Texture2D;
                                break;

                            case "texture3d":
                                parameterKey.Class = EffectParameterClass.ShaderResourceView;
                                parameterKey.Type = EffectParameterType.Texture3D;
                                break;

                            case "rwtexture1d":
                                parameterKey.Class = EffectParameterClass.UnorderedAccessView;
                                parameterKey.Type = EffectParameterType.RWTexture1D;
                                break;

                            case "rwtexture2d":
                                parameterKey.Class = EffectParameterClass.UnorderedAccessView;
                                parameterKey.Type = EffectParameterType.RWTexture2D;
                                break;

                            case "rwtexture3d":
                                parameterKey.Class = EffectParameterClass.UnorderedAccessView;
                                parameterKey.Type = EffectParameterType.RWTexture3D;
                                break;

                            case "samplerstate":
                                parameterKey.Class = EffectParameterClass.Sampler;
                                parameterKey.Type = EffectParameterType.Sampler;
                                break;
                        }
                    }
                    else if (variableType is ScalarType)
                    {
                        // Uint and int are collapsed to int
                        if (variableType == ScalarType.Int || variableType == ScalarType.UInt)
                        {
                            parameterKey.Class = EffectParameterClass.Scalar;
                            parameterKey.Type = variableType == ScalarType.Int ? EffectParameterType.Int : EffectParameterType.UInt;
                        }
                        else if (variableType == ScalarType.Float)
                        {
                            parameterKey.Class = EffectParameterClass.Scalar;
                            parameterKey.Type = EffectParameterType.Float;
                        }
                        else if (variableType == ScalarType.Bool)
                        {
                            parameterKey.Class = EffectParameterClass.Scalar;
                            parameterKey.Type = EffectParameterType.Bool;
                        }

                        parameterKey.RowCount = 1;
                        parameterKey.ColumnCount = 1;
                    }
                    else if (variableType is VectorType)
                    {
                        if (variableType == VectorType.Float2 || variableType == VectorType.Float3 || variableType == VectorType.Float4)
                        {
                            parameterKey.Class = isColor ? EffectParameterClass.Color : EffectParameterClass.Vector;
                            parameterKey.Type = EffectParameterType.Float;
                        }
                        else if (variableType == VectorType.Int2 || variableType == VectorType.Int3 || variableType == VectorType.Int4)
                        {
                            parameterKey.Class = EffectParameterClass.Vector;
                            parameterKey.Type = EffectParameterType.Int;
                        }
                        else if (variableType == VectorType.UInt2 || variableType == VectorType.UInt3 || variableType == VectorType.UInt4)
                        {
                            parameterKey.Class = EffectParameterClass.Vector;
                            parameterKey.Type = EffectParameterType.UInt;
                        }

                        parameterKey.RowCount = 1;
                        parameterKey.ColumnCount = (variableType as VectorType).Dimension;
                    }
                    else if (variableType is MatrixType)
                    {
                        parameterKey.Class = EffectParameterClass.MatrixColumns;
                        parameterKey.Type = EffectParameterType.Float;
                        parameterKey.RowCount = (variableType as MatrixType).RowCount;
                        parameterKey.ColumnCount = (variableType as MatrixType).ColumnCount;
                    }
                    else if (variableType is StructType)
                    {
                        parameterKey.Class = EffectParameterClass.Struct;
                        parameterKey.RowCount = 1;
                        parameterKey.ColumnCount = 1;
                    }
                }

                return parameterKey;
            }

            return null;
        }

        private void ParseConstantBufferVariable(string cbName, Variable variable)
        {
            if (variable.Qualifiers.Contains(SiliconStudio.Shaders.Ast.Hlsl.StorageQualifier.Static) ||
                variable.Qualifiers.Contains(StorageQualifier.Const) ||
                variable.Qualifiers.Contains(SiliconStudio.Shaders.Ast.Hlsl.StorageQualifier.Groupshared)
                )
                return;

            if (variable.Qualifiers.Contains(XenkoStorageQualifier.Stream))
            {
                parsingResult.Error(XenkoMessageCode.StreamVariableWithoutPrefix, variable.Span, variable);
                return;
            }

            foreach (var attribute in variable.Attributes.OfType<AttributeDeclaration>())
            {
                if (attribute.Name == "Link")
                {
                    if (attribute.Parameters.Count != 1)
                    {
                        parsingResult.Error(XenkoMessageCode.LinkArgumentsError, variable.Span);
                    }
                }
            }

            //// Try to resolve key
            var parameterKey = GetLinkParameterKey(variable);

            if (parameterKey != null)
            {
                LinkConstant(cbName, variable, parameterKey);
            }
            else
            {
                parsingResult.Error(XenkoMessageCode.LinkError, variable.Span, variable);
            }
        }

        private static void LinkVariable(EffectReflection reflection, string variableName, LocalParameterKey parameterKey)
        {
            var binding = new EffectParameterResourceData { Param = { KeyName = parameterKey.Name, Class = parameterKey.Class, Type = parameterKey.Type, ResourceGroup = parameterKey.ResourceGroup, RawName = variableName }, SlotStart = -1 };
            reflection.ResourceBindings.Add(binding);
        }

        private void LinkConstant(string cbName, Variable variable, LocalParameterKey parameterKey)
        {
            // If the constant buffer is not present, add it
            var constantBuffer = effectReflection.ConstantBuffers.FirstOrDefault(buffer => buffer.Name == cbName);
            if (constantBuffer == null)
            {
                constantBuffer = new ShaderConstantBufferDescription() {Name = cbName};
                effectReflection.ConstantBuffers.Add(constantBuffer);
                var constantBufferBinding = new EffectParameterResourceData { Param = { KeyName = cbName, Class = EffectParameterClass.ConstantBuffer, Type = EffectParameterType.Buffer, RawName = cbName, ResourceGroup = cbName }, SlotStart = -1 };
                effectReflection.ResourceBindings.Add(constantBufferBinding);
                valueBindings.Add(constantBuffer, new List<EffectParameterValueData>());
            }

            // Get the list of members of this constant buffer
            var members = valueBindings[constantBuffer];

            var binding = new EffectParameterValueData
                {
                    Param =
                        {
                            KeyName = parameterKey.Name,
                            Class = parameterKey.Class,
                            Type = parameterKey.Type,
                            RawName = variable.Name
                        },
                    RowCount = parameterKey.RowCount,
                    ColumnCount = parameterKey.ColumnCount,
                    Count = parameterKey.Count,
                };
            
            members.Add(binding);
        }

        private class LocalParameterKey
        {
            public string Name;

            public string ResourceGroup;

            public EffectParameterClass Class;

            public EffectParameterType Type;

            public int RowCount;

            public int ColumnCount;

            public int Count = 1;
        }
    }
}