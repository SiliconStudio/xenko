// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Shaders.Ast;

namespace SiliconStudio.Xenko.Shaders.Parser.Analysis
{
    [DataContract]
    internal class StatementNodeCouple
    {
        public Statement Statement;
        public Node Node;

        public StatementNodeCouple() : this(null, null) { }

        public StatementNodeCouple(Statement statement, Node node)
        {
            Statement = statement;
            Node = node;
        }
    }
}