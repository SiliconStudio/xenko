// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Paradox.Shaders.Parser.Ast;
using SiliconStudio.Shaders.Analysis.Hlsl;
using SiliconStudio.Shaders.Parser;

namespace SiliconStudio.Paradox.Shaders.Parser.Analysis
{
    internal class ParadoxTypeAnalysis : HlslSemanticAnalysis
    {
        #region Contructor

        public ParadoxTypeAnalysis(ParsingResult result)
            : base(result)
        {
            SetupHlslAnalyzer();
        }

        #endregion

        public void Run(ShaderClassType shaderClassType)
        {
            Visit(shaderClassType);
        }
    }
}
