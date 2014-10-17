// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Linq;

using SiliconStudio.Paradox.Shaders.Parser.Ast;
using SiliconStudio.Shaders.Ast;
using SiliconStudio.Shaders.Ast.Hlsl;
using SiliconStudio.Shaders.Visitor;

namespace SiliconStudio.Paradox.VisualStudio.Commands.Shaders
{
    internal partial class ShaderKeySourceGenerator
    {
        public ShaderKeySourceGenerator(ShaderKeyClass shaderKeyClass)
        {
            this.ShaderKeyClass = shaderKeyClass;
        }

        public ShaderKeyClass ShaderKeyClass { get; set; }

        internal static ShaderKeyClass GenerateShaderKeyClass(ShaderClassType shaderClassType)
        {
            ShaderKeyClass shaderKeyClass = null;

            // Iterate over variables
            foreach (var decl in shaderClassType.Members.OfType<Variable>())
            {
                if (decl.Qualifiers.Contains(SiliconStudio.Shaders.Ast.Hlsl.StorageQualifier.Extern)
                    || decl.Qualifiers.Contains(SiliconStudio.Shaders.Ast.Hlsl.StorageQualifier.Const)
                    || decl.Qualifiers.Contains(ParadoxStorageQualifier.Stream))
                    continue;

                try
                {
                    if (decl.Attributes.OfType<AttributeDeclaration>().Any(x => x.Name == "RenameLink"))
                        continue;

                    ShaderKeyVariable variable = null;
                    var type = decl.Type.ResolveType();
                    bool isArray = type is ArrayType;
                    bool processInitialValue = false;
                    bool isColor = decl.Attributes.OfType<AttributeDeclaration>().Any(x => x.Name == "Color");

                    if (isArray)
                    {
                        type = ((ArrayType)type).Type.ResolveType();
                    }

                    if (type is ScalarType)
                    {
                        processInitialValue = true;
                        variable = new ShaderKeyVariable(decl.Name, ((ScalarType)type).Type.FullName,
                                                         ShaderKeyVariableCategory.Value);
                    }
                    else if (type is VectorType)
                    {
                        processInitialValue = true;
                        var typeName = "Vector" + ((VectorType)type).Dimension;
                        if (isColor)
                        {
                            if (((VectorType)type).Dimension == 3)
                                typeName = "Color3";
                            else if (((VectorType)type).Dimension == 4)
                                typeName = "Color4";
                            else
                                throw new NotSupportedException("Color attribute is only valid for float3/float4.");
                        }
                        variable = new ShaderKeyVariable(decl.Name, typeName,
                                                         ShaderKeyVariableCategory.Value);
                    }
                    else if (type is MatrixType)
                    {
                        processInitialValue = true;
                        variable = new ShaderKeyVariable(decl.Name, "Matrix", ShaderKeyVariableCategory.Value);
                    }
                    else if (type is TextureType || IsStringInList(type.Name, "Texture1D", "RWTexture1D", "Texture2D", "RWTexture2D", "Texture3D", "RWTexture3D"))
                    {
                        variable = new ShaderKeyVariable(decl.Name, "Texture", ShaderKeyVariableCategory.Resource);
                    }
                    else if (type is StateType && type.Name == "SamplerState")
                    {
                        variable = new ShaderKeyVariable(decl.Name, "SamplerState", ShaderKeyVariableCategory.Resource);
                    }
                    else if (type is GenericType<ObjectType> &&
                             IsStringInList(type.Name, "StructuredBuffer", "RWStructuredBuffer", "ConsumeStructuredBuffer",
                                            "AppendStructuredBuffer"))
                    {
                        variable = new ShaderKeyVariable(decl.Name, "Buffer", ShaderKeyVariableCategory.Resource);
                    }

                    if (isArray && variable != null && variable.Category == ShaderKeyVariableCategory.Value)
                    {
                        variable.Category = ShaderKeyVariableCategory.ArrayValue;
                    }

                    if (variable == null)
                    {
                        throw new NotSupportedException();
                    }

                    if (decl.InitialValue != null && processInitialValue)
                    {
                        variable.InitialValue = decl.InitialValue.ToString();

                        // Add new operator for array
                        if (isArray && variable.InitialValue.Contains("{"))
                            variable.InitialValue = "new " + variable.Type + "[] " + variable.InitialValue;
                    }
                    else if (isArray && processInitialValue)
                    {
                        // Default array initializer
                        var dimensions = ((ArrayType)decl.Type.ResolveType()).Dimensions;
                        if (dimensions.Count != 1)
                            throw new NotSupportedException();
                        var expressionEvaluator = new ExpressionEvaluator();
                        var expressionResult = expressionEvaluator.Evaluate(dimensions[0]);
                        if (expressionResult.HasErrors)
                            throw new InvalidOperationException();
                        variable.InitialValue = "new " + variable.Type + "[" + expressionResult.Value + "]";
                        variable.Type += "[]";
                    }

                    if (variable.InitialValue != null)
                    {
                        // Rename float2/3/4 to Vector2/3/4
                        if (variable.InitialValue.StartsWith("float2")
                            || variable.InitialValue.StartsWith("float3")
                            || variable.InitialValue.StartsWith("float4"))
                            variable.InitialValue = variable.InitialValue.Replace("float", "new Vector");

                        if (isColor)
                        {
                            variable.InitialValue = variable.InitialValue.Replace("Vector3", "Color3");
                            variable.InitialValue = variable.InitialValue.Replace("Vector4", "Color4");
                        }
                    }

                    var variableType = decl.Attributes.OfType<AttributeDeclaration>().Where(x => x.Name == "Type").Select(x => (string)x.Parameters[0].Value).FirstOrDefault();
                    if (variableType != null)
                        variable.Type = variableType;

                    variable.Map = decl.Attributes.OfType<AttributeDeclaration>().Where(x => x.Name == "Map").Select(x => (string)x.Parameters[0].Value).FirstOrDefault();

                    // First time initialization
                    if (shaderKeyClass == null)
                        shaderKeyClass = new ShaderKeyClass(shaderClassType.Name + "Keys");
                    shaderKeyClass.Variables.Add(variable);
                }
                catch (Exception)
                {
                    Console.WriteLine("Could not process variable {0}.{1} of type {2}", shaderClassType.Name, decl.Name,
                                      decl.Type);
                }
            }
            return shaderKeyClass;
        }

        private static bool IsStringInList(string value, params string[] list)
        {
            return list.Any(str => string.Compare(value, str, StringComparison.InvariantCultureIgnoreCase) == 0);
        }
    }
}