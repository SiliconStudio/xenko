// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Globalization;
using SiliconStudio.Shaders.Ast;
using SiliconStudio.Shaders.Ast.Hlsl;
using SiliconStudio.Shaders.Writer.Hlsl;
using LayoutQualifier = SiliconStudio.Shaders.Ast.Glsl.LayoutQualifier;

namespace SiliconStudio.Shaders.Convertor
{
    /// <summary>
    /// A writer for a shader.
    /// </summary>
    public class HlslToGlslWriter : HlslWriter
    {
        private readonly GlslShaderPlatform shaderPlatform;
        private readonly int shaderVersion;
        private readonly PipelineStage pipelineStage;

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="HlslWriter"/> class. 
        /// </summary>
        /// <param name="useNodeStack">
        /// if set to <c>true</c> [use node stack].
        /// </param>
        public HlslToGlslWriter(GlslShaderPlatform shaderPlatform, int shaderVersion, PipelineStage pipelineStage, bool useNodeStack = false)
            : base(useNodeStack)
        {
            this.shaderPlatform = shaderPlatform;
            this.shaderVersion = shaderVersion;
            this.pipelineStage = pipelineStage;

            if (shaderPlatform == GlslShaderPlatform.OpenGLES)
            {
                TrimFloatSuffix = true;

                GenerateUniformBlocks = shaderVersion >= 300;
                SupportsTextureBuffer = shaderVersion >= 320;
            }
        }

        #endregion

        public bool GenerateUniformBlocks { get; set; } = true;

        public bool TrimFloatSuffix { get; set; } = false;

        public bool SupportsTextureBuffer { get; set; } = true;

        public string ExtraHeaders { get; set; }

        #region Public Methods

        /// <inheritdoc/>
        [Visit]
        public override void Visit(Shader shader)
        {
            // #version
            Write("#version ");
            Write(shaderVersion.ToString());

            // ES3+ expects "es" at the end of #version
            if (shaderPlatform == GlslShaderPlatform.OpenGLES && shaderVersion >= 300)
                Write(" es");

            WriteLine();
            WriteLine();

            if (shaderPlatform == GlslShaderPlatform.OpenGLES)
            {
                WriteLine("precision highp float;");

                if (shaderVersion >= 300)
                {
                    WriteLine("precision lowp sampler3D;");
                    WriteLine("precision lowp samplerCubeShadow;");
                    WriteLine("precision lowp sampler2DShadow;");
                    WriteLine("precision lowp sampler2DArray;");
                    WriteLine("precision lowp sampler2DArrayShadow;");
                    WriteLine("precision lowp isampler2D;");
                    WriteLine("precision lowp isampler3D;");
                    WriteLine("precision lowp isamplerCube;");
                    WriteLine("precision lowp isampler2DArray;");
                    WriteLine("precision lowp usampler2D;");
                    WriteLine("precision lowp usampler3D;");
                    WriteLine("precision lowp usamplerCube;");
                    WriteLine("precision lowp usampler2DArray;");
                }

                if (shaderVersion >= 320 || SupportsTextureBuffer)
                {
                    WriteLine("precision lowp samplerBuffer;");
                    WriteLine("precision lowp isamplerBuffer;");
                    WriteLine("precision lowp usamplerBuffer;");
                }

                WriteLine();

                if (shaderVersion < 320 && SupportsTextureBuffer)
                {
                    // In ES 3.1 and previous, we use texelFetchBuffer in case it needs to be remapped into something else by user
                    WriteLine("#define texelFetchBuffer(sampler, P) texelFetch(sampler, P)");
                }
            }

            if (ExtraHeaders != null)
                WriteLine(ExtraHeaders);

            if (shader == null)
            {
                // null entry point for pixel shader means no pixel shader. In that case, we return a default function.
                // TODO: support that directly in HlslToGlslConvertor?
                if (pipelineStage == PipelineStage.Pixel && shaderPlatform == GlslShaderPlatform.OpenGLES && shaderVersion >= 300)
                {
                    WriteLine("out float fragmentdepth; void main(){ fragmentdepth = gl_FragCoord.z; }");
                }
                else
                {
                    throw new NotSupportedException($"Can't output empty {pipelineStage} shader for platform {shaderPlatform} version {shaderVersion}.");
                }
            }
            else
            {
                base.Visit(shader);
            }
        }

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
