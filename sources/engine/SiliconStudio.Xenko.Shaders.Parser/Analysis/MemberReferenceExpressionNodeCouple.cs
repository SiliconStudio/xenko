// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Shaders.Ast;

namespace SiliconStudio.Xenko.Shaders.Parser.Analysis
{
    public class MemberReferenceExpressionNodeCouple
    {
        public MemberReferenceExpression Member;
        public Node Node;

        public MemberReferenceExpressionNodeCouple() : this(null, null) { }

        public MemberReferenceExpressionNodeCouple(MemberReferenceExpression member, Node node)
        {
            Member = member;
            Node = node;
        }
    }
}
