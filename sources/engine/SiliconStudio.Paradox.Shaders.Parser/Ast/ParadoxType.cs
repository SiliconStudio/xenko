// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Shaders.Ast;

namespace SiliconStudio.Paradox.Shaders.Parser.Ast
{
    public class ParadoxType : TypeBase
    {
        #region Static values

        public static readonly ParadoxType Constants = new ParadoxType("Constants");
        
        public static readonly ParadoxType Input = new ParadoxType("Input");
        
        public static readonly ParadoxType Input2 = new ParadoxType("Input2");
        
        public static readonly ParadoxType Output = new ParadoxType("Output");
        
        public static readonly ParadoxType Streams = new ParadoxType("Streams");

        #endregion

        #region Constructor

        protected ParadoxType() : base() { }

        protected ParadoxType(string name) : base(name) { }

        #endregion
    }
}
