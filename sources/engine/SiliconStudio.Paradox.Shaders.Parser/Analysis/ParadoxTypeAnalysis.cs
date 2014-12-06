// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Paradox.Shaders.Parser.Ast;
using SiliconStudio.Shaders.Analysis.Hlsl;
using SiliconStudio.Shaders.Ast;
using SiliconStudio.Shaders.Parser;

namespace SiliconStudio.Paradox.Shaders.Parser.Analysis
{
    public class ParadoxTypeAnalysis : HlslSemanticAnalysis
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

        /// <summary>
        /// Store if it is a structure type, check if it is a ParadoxType
        /// </summary>
        /// <param name="typeName">the TypeName to visit</param>
        /// <returns>the TypeBase object</returns>
        [Visit]
        protected override TypeBase Visit(TypeName typeName)
        {
            if (typeName.Name.Text == "Streams")
                return ParadoxType.Streams;
            if (typeName.Name.Text == "Constants")
                return ParadoxType.Constants;
            if (typeName.Name.Text == "Input")
                return ParadoxType.Input;
            if (typeName.Name.Text == "Input2")
                return ParadoxType.Input2;
            if (typeName.Name.Text == "Output")
                return ParadoxType.Output;

            return base.Visit(typeName);
        }
    }
}
