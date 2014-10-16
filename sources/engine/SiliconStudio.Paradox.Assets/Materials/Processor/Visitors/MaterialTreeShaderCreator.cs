// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using SiliconStudio.Assets;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Assets.Materials.Nodes;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Assets.Materials.Processor.Visitors
{
    public class MaterialTreeShaderCreator : MaterialBaseVisitor
    {
        #region Private constants
        
        //private const string BackgroundCompositionName = "backgroundName";
        //private const string ForegroundCompositionName = "foregroundName";
        private const string BackgroundCompositionName = "color1";
        private const string ForegroundCompositionName = "color2";

        #endregion
        
        #region Private members

        /// <summary>
        /// The shader build statuses.
        /// </summary>
        private Dictionary<string, ShaderBuildStatus> shaderBuildStatuses;

        /// <summary>
        /// Flag to change some shaders.
        /// </summary>
        private bool shaderForReduction = false;

        /// <summary>
        /// A flag stating if this is for displacement purpose.
        /// </summary>
        private bool displacementShader = false;

        /// <summary>
        /// The constant values.
        /// </summary>
        private ParameterCollection constantValues;

        #endregion

        #region Public members

        /// <summary>
        /// All the shaders.
        /// </summary>
        public Dictionary<string, ShaderSource> ModelShaderSources;

        /// <summary>
        /// The error logger.
        /// </summary>
        public readonly LoggerResult Logger;
        
        #endregion

        #region Public methods

        public MaterialTreeShaderCreator(MaterialDescription mat) : base(mat)
        {
            shaderBuildStatuses = mat.Nodes.ToDictionary(x => x.Key, x => ShaderBuildStatus.None);
            ModelShaderSources = new Dictionary<string, ShaderSource>();
            Logger = new LoggerResult();
        }
        
        /// <summary>
        /// Generate one shader.
        /// </summary>
        public ShaderMixinSource GenerateShaderForReduction(IMaterialNode materialNode)
        {
            shaderForReduction = true;

            var textureVisitor = new MaterialTextureVisitor(Material);
            var allTextures  = textureVisitor.GetAllTextureValues(materialNode);
            textureVisitor.AssignDefaultTextureKeys(allTextures.Distinct(), null);
            return GetShaderMixinSource(GetShaderSource(materialNode));
        }

        /// <summary>
        /// Generate all the shaders for this model, assign keys for texture and samplers.
        /// </summary>
        /// <returns>A dictionary of the shaders.</returns>
        public ParameterCollection GenerateModelShaders()
        {
            AssignModelTextureKeys();
            ModelShaderSources = new Dictionary<string, ShaderSource>();
            constantValues = new ParameterCollection();
            shaderBuildStatuses = Material.Nodes.ToDictionary(x => x.Key, x => ShaderBuildStatus.None);
            var result = new ParameterCollection();
            shaderForReduction = false;
            foreach (var reference in Material.ColorNodes)
            {
                if (reference.Key != null)
                {
                    if (reference.Value != null)
                    {
                        BeginShaderCreation(reference.Value, reference.Key == MaterialParameters.DisplacementMap);

                        ShaderSource shaderSource;
                        if (ModelShaderSources.TryGetValue(reference.Value, out shaderSource))
                        {
                            var sms = GetShaderMixinSource(shaderSource);
                            if (sms != null)
                            {
                                result.Add(reference.Key, sms);
                                continue;
                            }
                        }
                    }
                    Logger.Error("[Material] Shader creation failed. The key " + reference.Key.Name + " did not produce any shader.");
                }
                else
                {
                    Logger.Error("[Material] Shader creation failed. The key " + reference.Key.Name + " in ColorNodes is not a ShaderMixinSource parameter key.");
                }
            }
            constantValues.CopyTo(result);
            return result;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Assign the default texture keys to this model.
        /// </summary>
        private void AssignModelTextureKeys()
        {
            var textureVisitor = new MaterialTextureVisitor(Material);
            var allTextures = new List<MaterialTextureNode>();
            var allSampler = new List<NodeParameterSampler>();
            foreach (var referenceName in Material.ColorNodes.Select(x => x.Value))
            {
                var startNode = Material.FindNode(referenceName);
                if (startNode != null)
                {
                    allTextures.AddRange(textureVisitor.GetAllTextureValuesWithGenerics(startNode));
                    allSampler.AddRange(textureVisitor.GetAllSamplerValues(startNode));
                }
            }
            textureVisitor.AssignDefaultTextureKeys(allTextures.Distinct(), allSampler.Distinct());
        }

        /// <summary>
        /// Creates shaders with multiple keys.
        /// </summary>
        /// <param name="referenceName">The name of the reference.</param>
        /// <param name="useForDisplacement">A flag stating that this shader will be used for displacement.</param>
        private void BeginShaderCreation(string referenceName, bool useForDisplacement)
        {
            if (referenceName == null || !shaderBuildStatuses.ContainsKey(referenceName))
                return;

            displacementShader = useForDisplacement;

            var status = shaderBuildStatuses[referenceName];

            if (status == ShaderBuildStatus.None)
            {
                var node = Material.FindNode(referenceName);
                if (node == null)
                {
                    Logger.Error("[Material] There is no node with the name " + referenceName + ".");
                    shaderBuildStatuses[referenceName] = ShaderBuildStatus.Completed;
                    return;
                }

                shaderBuildStatuses[referenceName] = ShaderBuildStatus.InProgress;
                ModelShaderSources[referenceName] = GetShaderSource(node);
                shaderBuildStatuses[referenceName] = ShaderBuildStatus.Completed;
            }
            else if (status == ShaderBuildStatus.InProgress)
            {
                shaderBuildStatuses[referenceName] = ShaderBuildStatus.Completed;
                Logger.Error("[Material] The node reference " + referenceName + " is part of a cycle.");
            }
        }

        /// <summary>
        /// Gets the shader source.
        /// </summary>
        /// <param name="node">The node to process.</param>
        /// <returns>The shader source.</returns>
        private ShaderSource GetShaderSource(IMaterialNode node)
        {
            if (node == null)
                return new ShaderClassSource("ComputeColor");

            if (node is MaterialFloatNode)
                return GetShaderSource(node as MaterialFloatNode);
            if (node is MaterialFloat4Node)
                return GetShaderSource(node as MaterialFloat4Node);
            if (node is MaterialColorNode)
                return GetShaderSource(node as MaterialColorNode);
            if (node is MaterialTextureNode)
                return GetShaderSource(node as MaterialTextureNode);
            if (node is MaterialShaderClassNode)
                return GetShaderSource(node as MaterialShaderClassNode);
            if (node is MaterialBinaryNode)
                return GetShaderSource(node as MaterialBinaryNode);
            if (node is MaterialReferenceNode)
            {
                var referenceName = (node as MaterialReferenceNode).Name;
                if (shaderForReduction)
                {
                    var refNode = Material.FindNode(referenceName);
                    return GetShaderSource(refNode);
                }
                else
                {
                    if (referenceName == null)
                    {
                        Logger.Warning("[Material] The MaterialReferenceNode [" + node + "] doesn't reference anything.");
                        return null;
                    }

                    BeginShaderCreation(referenceName, displacementShader);

                    ShaderSource shaderSource;
                    if (!ModelShaderSources.TryGetValue(referenceName, out shaderSource))
                        return null;

                    return shaderSource;
                }
            }

            // TODO: error?
            throw new Exception("[Material] An unsupported material node was encountered during shader creation.");
        }

        /// <summary>
        /// Build the ShaderMixinSource to evaluate the binaryNode.
        /// </summary>
        /// <param name="node">The MaterialFloatNode binaryNode used as source to find the ShaderMixinSource.</param>
        /// <returns>The corresponding ShaderMixinSource.</returns>
        private ShaderSource GetShaderSource(MaterialFloatNode node)
        {
            if (!node.IsReducible && node.Key != null)
            {
                constantValues.Set(node.Key, node.Value);
                return new ShaderClassSource("ComputeColorConstantFloatLink", node.Key);
            }

            return new ShaderClassSource("ComputeColorFixed", GetAsShaderString(node.Value));
        }

        /// <summary>
        /// Build the ShaderMixinSource to evaluate the binaryNode.
        /// </summary>
        /// <param name="node">The MaterialFloat4Node binaryNode used as source to find the ShaderMixinSource.</param>
        /// <returns>The corresponding ShaderMixinSource.</returns>
        private ShaderSource GetShaderSource(MaterialColorNode node)
        {
            if (!node.IsReducible && node.Key != null)
            {
                constantValues.Set(node.Key, node.Value);
                return new ShaderClassSource("ComputeColorConstantColorLink", node.Key);
            }

            return new ShaderClassSource("ComputeColorFixed", GetAsShaderString(node.Value));
        }

        /// <summary>
        /// Build the ShaderMixinSource to evaluate the binaryNode.
        /// </summary>
        /// <param name="node">The MaterialFloat4Node binaryNode used as source to find the ShaderMixinSource.</param>
        /// <returns>The corresponding ShaderMixinSource.</returns>
        private ShaderSource GetShaderSource(MaterialFloat4Node node)
        {
            if (!node.IsReducible && node.Key != null)
            {
                constantValues.Set(node.Key, node.Value);
                return new ShaderClassSource("ComputeColorConstantLink", node.Key);
            }

            return new ShaderClassSource("ComputeColorFixed", GetAsShaderString(node.Value));
        }

        /// <summary>
        /// Build the ShaderMixinSource to evaluate the binaryNode.
        /// </summary>
        /// <param name="node">The MaterialTextureNode binaryNode used as source to find the ShaderMixinSource.</param>
        /// <returns>The corresponding ShaderMixinSource.</returns>
        private ShaderSource GetShaderSource(MaterialTextureNode node)
        {
            string usedTexcoord;
            if (shaderForReduction)
                usedTexcoord = "TEXCOORD0";
            else
                usedTexcoord = "TEXCOORD" + GetTextureIndex(node.TexcoordIndex);

            // "TTEXTURE", "TStream"
            ShaderClassSource shaderSource;
            if (displacementShader)
                shaderSource = new ShaderClassSource("ComputeColorTextureDisplacement", node.UsedParameterKey, usedTexcoord);
            else if (node.Offset != Vector2.Zero)
                shaderSource = new ShaderClassSource("ComputeColorTextureScaledOffsetSampler", node.UsedParameterKey, usedTexcoord, GetAsShaderString(node.Scale), GetAsShaderString(node.Offset), node.Sampler.SamplerParameterKey);
            else if (node.Scale != Vector2.One)
                shaderSource = new ShaderClassSource("ComputeColorTextureScaledSampler", node.UsedParameterKey, usedTexcoord, GetAsShaderString(node.Scale), node.Sampler.SamplerParameterKey);
            else
                shaderSource = new ShaderClassSource("ComputeColorTextureSampler", node.UsedParameterKey, usedTexcoord, node.Sampler.SamplerParameterKey);

            return shaderSource;
        }

        /// <summary>
        /// Build the ShaderMixinSource to evaluate the binaryNode.
        /// </summary>
        /// <param name="node">The MaterialShaderClassNode binaryNode used as source to find the ShaderMixinSource.</param>
        /// <returns>The corresponding ShaderMixinSource.</returns>
        private ShaderSource GetShaderSource(MaterialShaderClassNode node)
        {
            if (!node.MixinReference.HasLocation())
                return new ShaderClassSource("ComputeColor");
            var mixinName = Path.GetFileNameWithoutExtension(node.MixinReference.Location);

            object[] generics = null;
            if (node.Generics.Count > 0)
            {
                // TODO: correct generic order
                var mixinGenerics = new List<object>();
                foreach (var genericKey in node.Generics.Keys)
                {
                    var generic = node.Generics[genericKey];
                    if (generic is NodeParameterTexture)
                    {
                        var textureReference = ((NodeParameterTexture)generic).Reference;
                        var foundNode = Material.FindNode(textureReference);
                        while (foundNode != null && !(foundNode is MaterialTextureNode))
                        {
                            var refNode = foundNode as MaterialReferenceNode;
                            if (refNode == null)
                                break;

                            foundNode = Material.FindNode(refNode.Name);
                        }

                        var foundTextureNode = foundNode as MaterialTextureNode;
                        if (foundTextureNode == null || foundTextureNode.UsedParameterKey == null)
                        {
                            Logger.Warning("[Material] The generic texture reference in node [" + node + "] is incorrect.");
                            mixinGenerics.Add("Texturing.Texture0");
                        }
                        else
                            mixinGenerics.Add(foundTextureNode.UsedParameterKey.ToString());
                    }
                    else if (generic is NodeParameterSampler)
                    {
                        var pk = ((NodeParameterSampler)generic).SamplerParameterKey;
                        if (pk == null)
                        {
                            Logger.Warning("[Material] The generic sampler reference in node [" + node + "] is incorrect.");
                            mixinGenerics.Add("Texturing.Sampler");
                        }
                        else
                            mixinGenerics.Add(pk.ToString());
                    }
                    else if (generic is NodeParameterFloat)
                        mixinGenerics.Add(((NodeParameterFloat)generic).Value.ToString(CultureInfo.InvariantCulture));
                    else if (generic is NodeParameterInt)
                        mixinGenerics.Add(((NodeParameterInt)generic).Value.ToString(CultureInfo.InvariantCulture));
                    else if (generic is NodeParameterFloat2)
                        mixinGenerics.Add(GetAsShaderString(((NodeParameterFloat2)generic).Value));
                    else if (generic is NodeParameterFloat3)
                        mixinGenerics.Add(GetAsShaderString(((NodeParameterFloat3)generic).Value));
                    else if (generic is NodeParameterFloat4)
                        mixinGenerics.Add(GetAsShaderString(((NodeParameterFloat4)generic).Value));
                    else if (generic is NodeParameter)
                        mixinGenerics.Add(((NodeParameter)generic).Reference);
                    else
                        throw new Exception("[Material] Unknown node type: " + generic.GetType());
                }
                generics = mixinGenerics.ToArray();
            }
            
            var shaderClassSource = new ShaderClassSource(mixinName, generics);

            if (node.CompositionNodes.Count == 0)
                return shaderClassSource;
            
            var mixin = new ShaderMixinSource();
            mixin.Mixins.Add(shaderClassSource);

            foreach (var comp in node.CompositionNodes)
            {
                if (comp.Value != null)
                {
                    var compShader = GetShaderSource(comp.Value);
                    if (compShader != null)
                        mixin.Compositions.Add(comp.Key, compShader);
                }
            }

            return mixin;
        }

        /// <summary>
        /// Build the ShaderMixinSource to evaluate the binaryNode.
        /// </summary>
        /// <param name="binaryNode">The MaterialBinaryNode binaryNode used as source to find the ShaderMixinSource.</param>
        /// <returns>The corresponding ShaderMixinSource.</returns>
        private ShaderSource GetShaderSource(MaterialBinaryNode binaryNode)
        {
            var leftShaderSource = GetShaderSource(binaryNode.LeftChild);
            var rightShaderSource = GetShaderSource(binaryNode.RightChild);

            var shaderSource = new ShaderClassSource(GetCorrespondingShaderSourceName(binaryNode.Operand));
            var mixin = new ShaderMixinSource();
            mixin.Mixins.Add(shaderSource);
            if (leftShaderSource != null)
                mixin.AddComposition(BackgroundCompositionName, leftShaderSource);
            if (binaryNode.Operand != MaterialBinaryOperand.None && binaryNode.Operand != MaterialBinaryOperand.Opaque && rightShaderSource != null)
                mixin.AddComposition(ForegroundCompositionName, rightShaderSource);

            return mixin;
        }

        #endregion

        #region Private static methods

        private static int GetTextureIndex(TextureCoordinate texcoord)
        {
            switch (texcoord)
            {
                case TextureCoordinate.Texcoord0:
                    return 0;
                case TextureCoordinate.Texcoord1:
                    return 1;
                case TextureCoordinate.Texcoord2:
                    return 2;
                case TextureCoordinate.Texcoord3:
                    return 3;
                case TextureCoordinate.Texcoord4:
                    return 4;
                case TextureCoordinate.Texcoord5:
                    return 5;
                case TextureCoordinate.Texcoord6:
                    return 6;
                case TextureCoordinate.Texcoord7:
                    return 7;
                case TextureCoordinate.Texcoord8:
                    return 8;
                case TextureCoordinate.Texcoord9:
                    return 9;
                case TextureCoordinate.TexcoordNone:
                default:
                    throw new ArgumentOutOfRangeException("texcoord");
            }
        }

        /// <summary>
        /// Get the name of the ShaderClassSource corresponding to the operation
        /// </summary>
        /// <param name="materialBinaryOperand">The operand.</param>
        /// <returns>The name of the ShaderClassSource.</returns>
        private static string GetCorrespondingShaderSourceName(MaterialBinaryOperand materialBinaryOperand)
        {
            switch (materialBinaryOperand)
            {
                case MaterialBinaryOperand.Add:
                    return "ComputeColorAdd3ds"; //TODO: change this (ComputeColorAdd?)
                case MaterialBinaryOperand.Average:
                    return "ComputeColorAverage";
                case MaterialBinaryOperand.Color:
                    return "ComputeColorColor";
                case MaterialBinaryOperand.ColorBurn:
                    return "ComputeColorColorBurn";
                case MaterialBinaryOperand.ColorDodge:
                    return "ComputeColorColorDodge";
                case MaterialBinaryOperand.Darken:
                    return "ComputeColorDarken3ds"; //"ComputeColorDarkenMaya" //TODO: change this
                case MaterialBinaryOperand.Desaturate:
                    return "ComputeColorDesaturate";
                case MaterialBinaryOperand.Difference:
                    return "ComputeColorDifference3ds"; //"ComputeColorDifferenceMaya" //TODO: change this
                case MaterialBinaryOperand.Divide:
                    return "ComputeColorDivide";
                case MaterialBinaryOperand.Exclusion:
                    return "ComputeColorExclusion";
                case MaterialBinaryOperand.HardLight:
                    return "ComputeColorHardLight";
                case MaterialBinaryOperand.HardMix:
                    return "ComputeColorHardMix";
                case MaterialBinaryOperand.Hue:
                    return "ComputeColorHue";
                case MaterialBinaryOperand.Illuminate:
                    return "ComputeColorIlluminate";
                case MaterialBinaryOperand.In:
                    return "ComputeColorIn";
                case MaterialBinaryOperand.Lighten:
                    return "ComputeColorLighten3ds"; //"ComputeColorLightenMaya" //TODO: change this
                case MaterialBinaryOperand.LinearBurn:
                    return "ComputeColorLinearBurn";
                case MaterialBinaryOperand.LinearDodge:
                    return "ComputeColorLinearDodge";
                case MaterialBinaryOperand.Mask:
                    return "ComputeColorMask";
                case MaterialBinaryOperand.Multiply:
                    return "ComputeColorMultiply"; //return "ComputeColorMultiply3ds"; //"ComputeColorMultiplyMaya" //TODO: change this
                case MaterialBinaryOperand.None:
                    return "ComputeColorNone";
                case MaterialBinaryOperand.Opaque:
                    return "ComputeColorOpaque";
                case MaterialBinaryOperand.Out:
                    return "ComputeColorOut";
                case MaterialBinaryOperand.Over:
                    return "ComputeColorOver3ds"; //TODO: change this to "ComputeColorLerpAlpha"
                case MaterialBinaryOperand.Overlay:
                    return "ComputeColorOverlay3ds"; //"ComputeColorOverlayMaya" //TODO: change this
                case MaterialBinaryOperand.PinLight:
                    return "ComputeColorPinLight";
                case MaterialBinaryOperand.Saturate:
                    return "ComputeColorSaturate";
                case MaterialBinaryOperand.Saturation:
                    return "ComputeColorSaturation";
                case MaterialBinaryOperand.Screen:
                    return "ComputeColorScreen";
                case MaterialBinaryOperand.SoftLight:
                    return "ComputeColorSoftLight";
                case MaterialBinaryOperand.Subtract:
                    return "ComputeColorSubtract3ds"; //"ComputeColorOverlayMaya" //TODO: change this
                case MaterialBinaryOperand.SubstituteAlpha:
                    return "ComputeColorSubstituteAlpha";
                default:
                    throw new ArgumentOutOfRangeException("materialBinaryOperand");
            }
        }

        private static string GetAsShaderString(Vector2 v)
        {
            return String.Format(CultureInfo.InvariantCulture, "float2({0}, {1})", v.X, v.Y);
        }

        private static string GetAsShaderString(Vector3 v)
        {
            return String.Format(CultureInfo.InvariantCulture, "float3({0}, {1}, {2})", v.X, v.Y, v.Z);
        }

        private static string GetAsShaderString(Vector4 v)
        {
            return String.Format(CultureInfo.InvariantCulture, "float4({0}, {1}, {2}, {3})", v.X, v.Y, v.Z, v.W);
        }

        private static string GetAsShaderString(Color4 c)
        {
            return String.Format(CultureInfo.InvariantCulture, "float4({0}, {1}, {2}, {3})", c.R, c.G, c.B, c.A);
        }

        private static string GetAsShaderString(float f)
        {
            return String.Format(CultureInfo.InvariantCulture, "float4({0}, {0}, {0}, {0})", f);
        }

        private static string GetAsShaderString(object obj)
        {
            return obj.ToString();
        }

        /// <summary>
        /// Build a encapsuling ShaderMixinSource if necessary.
        /// </summary>
        /// <param name="shaderSource">The input ShaderSource.</param>
        /// <returns>A ShaderMixinSource</returns>
        private static ShaderMixinSource GetShaderMixinSource(ShaderSource shaderSource)
        {
            if (shaderSource is ShaderClassSource)
            {
                var mixin = new ShaderMixinSource();
                mixin.Mixins.Add((ShaderClassSource)shaderSource);
                return mixin;
            }
            if (shaderSource is ShaderMixinSource)
                return (ShaderMixinSource)shaderSource;

            return null;
        }

        #endregion

        private enum ShaderBuildStatus
        {
            None,
            InProgress,
            Completed
        }
    }
}
