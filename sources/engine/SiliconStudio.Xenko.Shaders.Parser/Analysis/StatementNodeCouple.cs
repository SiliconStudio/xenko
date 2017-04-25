// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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
