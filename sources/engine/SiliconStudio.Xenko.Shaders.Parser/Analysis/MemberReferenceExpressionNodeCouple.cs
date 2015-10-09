// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Shaders.Ast;

namespace SiliconStudio.Paradox.Shaders.Parser.Analysis
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