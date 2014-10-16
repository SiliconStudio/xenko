// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Globalization;
using SiliconStudio.Shaders.Ast;
using SiliconStudio.Shaders.Ast.Hlsl;
using LayoutQualifier = SiliconStudio.Shaders.Ast.Glsl.LayoutQualifier;

namespace SiliconStudio.Shaders.Writer.Hlsl
{
    /// <summary>
    /// A writer for a shader.
    /// </summary>
    public class HlslToGlslWriter : HlslWriter
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="HlslWriter"/> class. 
        /// </summary>
        /// <param name="useNodeStack">
        /// if set to <c>true</c> [use node stack].
        /// </param>
        public HlslToGlslWriter(bool useNodeStack = false)
            : base(useNodeStack)
        {
            GenerateUniformBlocks = true;
        }

        #endregion

        public bool GenerateUniformBlocks { get; set; }

        public bool TrimFloatSuffix { get; set; }

        #region Public Methods

        /// <inheritdoc/>
        [Visit]
        public override void Visit(Literal literal)
        {
            if (TrimFloatSuffix && literal.Value is float)
                literal.Text = literal.Text.Trim('f', 'F', 'l', 'L');

            base.Visit(literal);
        }

        /// <inheritdoc />
        [Visit]
        public virtual void Visit(Ast.Glsl.InterfaceType interfaceType)
        {
            Write(interfaceType.Qualifiers, true);

            Write(" ");
            Write(interfaceType.Name);
            WriteSpace();

            // Post Attributes
            Write(interfaceType.Attributes, false);

            OpenBrace();

            foreach (var variableDeclaration in interfaceType.Fields)
                VisitDynamic(variableDeclaration);

            CloseBrace(false);

            if (IsDeclaratingVariable.Count == 0 || !IsDeclaratingVariable.Peek())
            {
                Write(";").WriteLine();
            }
        }

        /// <inheritdoc/>
        [Visit]
        public override void Visit(Annotations annotations)
        {
        }

        /// <inheritdoc/>
        [Visit]
        public override void Visit(ClassType classType)
        {
        }

        /// <inheritdoc/>
        [Visit]
        public override void Visit(InterfaceType interfaceType)
        {
        }

        /// <inheritdoc/>
        [Visit]
        public override void Visit(AsmExpression asmExpression)
        {
        }

        /// <inheritdoc/>
        [Visit]
        public override void Visit(ConstantBuffer constantBuffer)
        {
            // Flatten the constant buffers
            if (constantBuffer.Members.Count > 0)
            {
                if (GenerateUniformBlocks)
                {
                    Write(constantBuffer.Qualifiers, true);
                    if (constantBuffer.Register != null)
                    {
                        if (constantBuffer.Qualifiers != Qualifier.None)
                            throw new NotImplementedException();

                        Write("layout(binding = ").Write(constantBuffer.Register.Register.Text).Write(") ");
                    }
                    Write("uniform").Write(" ").Write(constantBuffer.Name).WriteSpace().Write("{").WriteLine();
                    Indent();
                    VisitDynamicList(constantBuffer.Members);
                }
                else
                {
                    Write("// Begin cbuffer ").Write(constantBuffer.Name).WriteLine();
                    foreach (var member in constantBuffer.Members)
                    {
                        // Prefix each variable with "uniform "
                        if (member is Variable)
                        {
                            Write("uniform");
                            Write(" ");
                        }
                        VisitDynamic(member);
                    }
                }

                if (GenerateUniformBlocks)
                {
                    Outdent();
                    Write("};").WriteLine();
                }
                else
                {
                    Write("// End buffer ").Write(constantBuffer.Name).WriteLine();
                }
            }
        }

        /// <inheritdoc/>
        [Visit]
        public override void Visit(Typedef typedef)
        {
        }

        /// <inheritdoc/>
        [Visit]
        public override void Visit(AttributeDeclaration attributeDeclaration)
        {

        }

        /// <inheritdoc/>
        [Visit]
        public override void Visit(CastExpression castExpression)
        {
        }

        /// <summary>
        /// Visits the specified technique.
        /// </summary>
        /// <param name="technique">The technique.</param>
        [Visit]
        public override void Visit(Technique technique)
        {
        }

        /// <inheritdoc />
        [Visit]
        public override void Visit(StateInitializer stateInitializer)
        {
        }

        /// <inheritdoc />
        [Visit]
        public override void Visit(StateExpression stateExpression)
        {
        }

        /// <inheritdoc />
        [Visit]
        public override void Visit(Semantic semantic)
        {
        }

        /// <inheritdoc />
        [Visit]
        public override void Visit(PackOffset packOffset)
        {
        }

        /// <inheritdoc />
        [Visit]
        public override void Visit(RegisterLocation registerLocation)
        {
        }

        /// <inheritdoc />
        [Visit]
        public void Visit(Ast.Glsl.LayoutQualifier layoutQualifier)
        {
            Write("layout(");
            for (int i = 0; i < layoutQualifier.Layouts.Count; i++)
            {
                var layout = layoutQualifier.Layouts[i];
                if (i > 0) Write(",").WriteSpace();
                Write(layout.Name);
                if (layout.Value != null)
                {
                    WriteSpace().Write("=").WriteSpace();
                    Visit((Node)layout.Value);
                }
            }
            Write(")");
            WriteSpace();
        }

        #endregion
    }
}