// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
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
