// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Shaders.Ast.Xenko;
using SiliconStudio.Shaders.Analysis.Hlsl;
using SiliconStudio.Shaders.Parser;

namespace SiliconStudio.Xenko.Shaders.Parser.Analysis
{
    internal class XenkoTypeAnalysis : HlslSemanticAnalysis
    {
        #region Contructor

        public XenkoTypeAnalysis(ParsingResult result)
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
