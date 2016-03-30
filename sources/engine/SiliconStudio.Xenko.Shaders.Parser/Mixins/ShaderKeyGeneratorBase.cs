// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

using SiliconStudio.Xenko.Shaders.Parser.Ast;
using SiliconStudio.Shaders.Ast;
using SiliconStudio.Shaders.Ast.Hlsl;
using SiliconStudio.Shaders.Visitor;
using SiliconStudio.Shaders.Writer;

using StorageQualifier = SiliconStudio.Shaders.Ast.StorageQualifier;

namespace SiliconStudio.Xenko.Shaders.Parser.Mixins
{
    public class ShaderKeyGeneratorBase : ShaderWriter
    {
        /// <summary>
        /// A flag stating if the currently visited variable is a Color.
        /// </summary>
        protected bool IsColorStatus = false;

        /// <summary>
        /// A flag stating if the currently visited variable is an array.
        /// </summary>
        protected bool IsArrayStatus = false;

        /// <summary>
        /// A flag stating if the initial value of the variable should be processed.
        /// </summary>
        protected bool ProcessInitialValueStatus = false;

        /// <summary>
        /// A flag indicating whether a variable must be transformed to a parameter key
        /// </summary>
        protected bool VariableAsParameterKey = true;

        protected bool IsXkfx = false;

        /// <summary>
        /// Runs the code generation. Results is accessible from <see cref="ShaderWriter.Text"/> property.
        /// </summary>
        public virtual bool Run()
        {
            return true;
        }

        /// <inheritdoc />
        [Visit]
        public override void Visit(Variable variable)
        {
            if (VariableAsParameterKey)
            {
                WriteVariableAsParameterKey(variable);
            }
            else
            {
                if (IsXkfx)
                {
                    base.Visit(variable);
                }
            }
        }

        /// <summary>
        /// Visits the specified namespace block.
        /// </summary>
        /// <param name="namespaceBlock">The namespace block.</param>
        [Visit]
        protected virtual void Visit(NamespaceBlock namespaceBlock)
        {
            WriteLinkLine(namespaceBlock);
            Write("namespace ").Write(namespaceBlock.Name);
            OpenBrace();
            foreach (Node node in namespaceBlock.Body)
            {
                VisitDynamic(node);
            }
            CloseBrace();
        }

        //[Visit]
        //protected virtual void Visit(ConstantBuffer constantBuffer)
        //{
        //    Visit((Node)constantBuffer);
        //}

        internal bool IsParameterKey(Variable variable)
        {
            // Don't generate a parameter key for variable stored storage qualifier: extern, const, compose, stream
            if (variable.Qualifiers.Contains(SiliconStudio.Shaders.Ast.Hlsl.StorageQualifier.Extern)
                || variable.Qualifiers.Contains(StorageQualifier.Const)
                || variable.Qualifiers.Contains(XenkoStorageQualifier.Compose)
                || variable.Qualifiers.Contains(XenkoStorageQualifier.PatchStream)
                || variable.Qualifiers.Contains(SiliconStudio.Shaders.Ast.Hlsl.StorageQualifier.Groupshared)
                || variable.Qualifiers.Contains(XenkoStorageQualifier.Stream))
                return false;

            // Don't generate a parameter key for [Link] or [RenameLink]
            if (variable.Attributes.OfType<AttributeDeclaration>().Any(x => x.Name == "RenameLink" || x.Name == "Link"))
                return false;

            return true;
        }

        protected void WriteVariableAsParameterKey(Variable variable)
        {
            if (!IsParameterKey(variable))
            {
                return;
            }

            IsColorStatus = variable.Attributes.OfType<AttributeDeclaration>().Any(x => x.Name == "Color");
            ProcessInitialValueStatus = false;
            IsArrayStatus = false;

            var variableType = variable.Attributes.OfType<AttributeDeclaration>().Where(x => x.Name == "Type").Select(x => (string)x.Parameters[0].Value).FirstOrDefault();
            var variableMap = variable.Attributes.OfType<AttributeDeclaration>().Where(x => x.Name == "Map").Select(x => (string)x.Parameters[0].Value).FirstOrDefault();

            // ParameterKey shouldn't contain only the underlying type in case of arrays (we use slots)
            var parameterType = variable.Type;

            string parameterKeyType;
            if (IsXkfx)
            {
                parameterKeyType = "Permutation";
            }
            else
            {
                while (parameterType is ArrayType)
                {
                    parameterType = ((ArrayType)parameterType).Type;
                }

                if (parameterType is ObjectType || IsTextureType(parameterType) || IsBufferType(parameterType))
                {
                    parameterKeyType = "Object";
                }
                else
                {
                    parameterKeyType = "Value";
                }
            }

            Write($"public static readonly {parameterKeyType}ParameterKey<");
            if (variableType == null)
                VisitDynamic(parameterType);
            else
                Write(variableType);
            Write("> ");
            Write(variable.Name);
            Write(" = ");
            if (variableMap == null)
            {
                Write($"ParameterKeys.New{parameterKeyType}<");
                if (variableType == null)
                    VisitDynamic(parameterType);
                else
                    Write(variableType);
                Write(">(");
                if (ProcessInitialValueStatus && variable.InitialValue != null)
                {
                    var initialValueString = variable.InitialValue.ToString();

                    if (initialValueString != "null")
                    {
                        if (IsArrayStatus)
                        {
                            initialValueString = variable.Type.ToString() + initialValueString;
                        }

                        // Rename float2/3/4 to Vector2/3/4
                        if (initialValueString.StartsWith("float2")
                            || initialValueString.StartsWith("float3")
                            || initialValueString.StartsWith("float4"))
                            initialValueString = initialValueString.Replace("float", "new Vector");
                        else if (IsArrayStatus)
                        {
                            initialValueString = "new " + initialValueString;
                        }

                        if (IsColorStatus)
                        {
                            initialValueString = initialValueString.Replace("Vector3", "Color3");
                            initialValueString = initialValueString.Replace("Vector4", "Color4");
                        }
                    }
                    Write(initialValueString);
                }
                Write(")");
            }
            else
            {
                Write(variableMap);
            }
            WriteLine(";");

            IsColorStatus = false;
            IsArrayStatus = false;
            ProcessInitialValueStatus = false;
        }

        /// <summary>
        /// Visits the specified type.
        /// </summary>
        /// <param name="typeName">the type.</param>
        [Visit]
        public override void Visit(TypeName typeName)
        {
            var type = typeName.ResolveType();
            if (ReferenceEquals(typeName, type))
            {
                base.Visit(typeName);
                ProcessInitialValueStatus = true;
            }
            else
            {
                VisitDynamic(type);
            }
        }

        /// <inheritdoc />
        [Visit]
        public override void Visit(ScalarType scalarType)
        {
            base.Visit(scalarType);
            ProcessInitialValueStatus = true;
        }

        /// <summary>
        /// Visits the specified type.
        /// </summary>
        /// <param name="type">the type.</param>
        [Visit]
        protected virtual void Visit(VectorType type)
        {
            var finalTypeName = "Vector" + type.Dimension;
            if (IsColorStatus)
            {
                if (type.Dimension == 3)
                    finalTypeName = "Color3";
                else if (type.Dimension == 4)
                    finalTypeName = "Color4";
                else
                    throw new NotSupportedException("Color attribute is only valid for float3/float4.");
            }
            Write(finalTypeName);
            ProcessInitialValueStatus = true;
        }

        /// <summary>
        /// Visits the specified type.
        /// </summary>
        /// <param name="type">the type.</param>
        [Visit]
        protected virtual void Visit(MatrixType type)
        {
            Write("Matrix");
            ProcessInitialValueStatus = true;
        }

        /// <summary>
        /// Visits the specified type.
        /// </summary>
        /// <param name="type">the type.</param>
        [Visit]
        protected virtual void Visit(TextureType type)
        {
            Write("Texture");
        }

        /// <summary>
        /// Visits the specified type.
        /// </summary>
        /// <param name="type">the type.</param>
        [Visit]
        protected virtual void Visit(StateType type)
        {
            Write("SamplerState");
        }

        /// <summary>
        /// Visits the specified type.
        /// </summary>
        /// <param name="type">the type.</param>
        [Visit]
        public override void Visit(ArrayType type)
        {
            var dimensions = type.Dimensions;
            if (dimensions.Count != 1)
                throw new NotSupportedException();
            /*
            var expressionEvaluator = new ExpressionEvaluator();
            if (dimensions.All(x => !(x is EmptyExpression)))
            {
                var expressionResult = expressionEvaluator.Evaluate(dimensions[0]);
                if (expressionResult.HasErrors)
                    throw new InvalidOperationException();
                Write(expressionResult.Value.ToString());
            }
            */
            VisitDynamic(type.Type);
            Write("[]");
            ProcessInitialValueStatus = true;
            IsArrayStatus = true;
        }

        /// <summary>
        /// Visits the specified type.
        /// </summary>
        /// <param name="type">the type.</param>
        [Visit]
        protected virtual void Visit(TypeBase type)
        {
            Write(type.Name);
            ProcessInitialValueStatus = true;
        }

        protected static bool IsStringInList(string value, params string[] list)
        {
            return list.Any(str => CultureInfo.InvariantCulture.CompareInfo.Compare(value, str, CompareOptions.IgnoreCase) == 0);
        }

        protected static bool IsTextureType(TypeBase type)
        {
            // TODO we should improve AST type system
            return type is TextureType || (type is GenericType && type.Name.Text.Contains("Texture"));
        }

        protected static bool IsBufferType(TypeBase type)
        {
            // TODO we should improve AST type system
            return type is GenericType && type.Name.Text.Contains("Buffer");
        }
    }
}
