// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using Irony.Parsing;

using SiliconStudio.Xenko.Shaders.Parser.Ast;
using SiliconStudio.Shaders.Ast;
using SiliconStudio.Shaders.Grammar;
using SiliconStudio.Shaders.Grammar.Hlsl;

namespace SiliconStudio.Xenko.Shaders.Parser.Grammar
{
    [Language("hotei2", "5.0", "Xenko2 hlsl grammar")]
    public partial class XenkoGrammar : HlslGrammar
    {
        protected readonly NonTerminal constant_buffer_name = T("constant_buffer_name", CreateConstantBufferNameAst);
        protected readonly NonTerminal semantic_type = T("semantic_type", CreateSemanticTypeAst);
        protected readonly NonTerminal link_type = T("link_type", CreateLinkTypeAst);
        protected readonly NonTerminal member_name = T("member_name", CreateStreamNameAst);
        protected readonly NonTerminal var_type = T("var_type", CreateVarTypeAst);
        protected readonly NonTerminal streams_type = T("streams_type", CreateStreamsType);
        protected readonly NonTerminal foreach_statement = T("foreach_statement", CreateForEachStatementAst);
        protected readonly NonTerminal foreach_params_statement = T("foreach_params_statement", CreateForEachParamsStatementAst);
        protected readonly NonTerminal class_type = T("class_type", CreateClassTypeAst); // TODO: look if really needed
        protected readonly NonTerminal params_block = T("params_block", CreateParametersAst);
        protected readonly NonTerminal shader_block = T("shader_block", CreateShaderBlockAst);
        protected readonly NonTerminal toplevel_declaration_block = T("toplevel_declaration_block", CreateDeclarationBlockAst);
        protected readonly NonTerminal mixin_statement = T("mixin_statement", CreateMixinStatementAst);
        protected readonly NonTerminal using_statement = T("using_statement", CreateUsingStatement);
        protected readonly NonTerminal using_params_statement = T("using_params_statement", CreateUsingParametersStatement);
        protected readonly NonTerminal enum_block = T("enum_block", CreateEnumBlockAst);
        protected readonly NonTerminal enum_item = T("enum_item", CreateEnumItemAst);
        protected readonly NonTerminal enum_item_list = T("enum_item_list", CreateListFromNode<Expression>);
        protected readonly NonTerminal namespace_block = T("namespace_block", CreateNamespaceBlockAst);

        protected readonly NonTerminal class_identifier_or_generic = TT("class_identifier_or_generic");
        protected readonly NonTerminal class_identifier_generic = T("class_identifier_generic", CreateClassIdentifierGenericAst);
        protected readonly NonTerminal class_identifier_generic_parameter_list = T("class_identifier_generic_parameter_list", CreateListFromNode<Variable>);
        protected readonly NonTerminal class_identifier_sub_generic = T("class_identifier_sub_generic");

        public NonTerminal ExpressionNonTerminal
        {
            get
            {
                return expression;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XenkoGrammar"/> class.
        /// </summary>
        public XenkoGrammar()
        {
            SnippetRoots.Add(expression);

            semantic_type.Rule = Keyword("Semantic");
            type.Rule |= semantic_type;

            link_type.Rule = Keyword("LinkType");
            type.Rule |= link_type;

            member_name.Rule = Keyword("MemberName");
            type.Rule |= member_name;

            var_type.Rule = Keyword("var");
            object_type.Rule |= var_type;

            // Add all Streams types
            streams_type.Rule = CreateRuleFromObjectTypes(StreamsType.GetStreams());
            object_type.Rule |= streams_type;

            identifier_extended.Rule |= Keyword("stage");

            // Allow simple types within generics (numbers, etc...)
            //type_name.Rule = identifier_or_generic + new IdentifierResolverHint(true);
            type_name.Rule = identifier_or_generic;
            identifier_sub_generic.Rule |= number;
            //identifier_sub_generic.Rule |= boolean;
            identifier_sub_generic.Rule |= identifier_dot;
            identifier_sub_generic.Rule |= simple_type;

            // Foreach statement
            foreach_statement.Rule = Keyword("foreach") + "(" + type + identifier + Keyword("in") + expression + ")" + statement;
            iteration_statement.Rule |= foreach_statement;

            // Add inheritance qualifiers
            storage_qualifier.Rule |= Keyword("override") | Keyword("abstract") | Keyword("stream") | Keyword("patchstream") | Keyword("stage") | Keyword("clone") | Keyword("compose") | Keyword("internal");

            // cbuffer can have . in their names (logical groups)
            constant_buffer_name.Rule = MakePlusRule(constant_buffer_name, ToTerm("."), identifier_raw);
            constant_buffer_resource.Rule = attribute_qualifier_pre + constant_buffer_resource_type + constant_buffer_name.Opt() + register.Opt() + "{" + declaration.ListOpt() + "}" + semi_opt;

            // override Hlsl class
            class_specifier.AstNodeCreator = CreateShaderClassSpecifierAst;
            class_base_type.AstNodeCreator = CreateShaderClassBaseTypeAst;
            class_base_type_list.AstNodeCreator = CreateListFromNode<ShaderTypeName>;
            class_base_type_list.Rule = MakePlusRule(class_base_type_list, ToTerm(","), class_type);
            class_type.Rule = identifier_or_generic;

            variable_identifier.Rule |= identifier_generic;

            // Allow generic identifier on member expressions
            member_reference_expression.Rule = postfix_expression + "." + identifier_or_generic;

            // ---------------------------------------------------
            // New Mixin System 
            // ---------------------------------------------------
            params_block.Rule = attribute_qualifier_pre + Keyword("params") + identifier_raw + block_statement;

            shader_block.Rule = attribute_qualifier_pre + Keyword("partial").Opt() + Keyword("shader") + identifier_raw + block_statement;

            using_params_statement.Rule = Keyword("using") + Keyword("params") + expression + ";"
                                          | Keyword("using") + Keyword("params") + expression + block_statement;
            

            using_statement.Rule = Keyword("using") + identifier_or_dot + ";";

            foreach_params_statement.Rule = Keyword("foreach") + "(" + Keyword("params") + conditional_expression + ")" + statement;
            iteration_statement.Rule |= foreach_params_statement;

            mixin_statement.Rule =   Keyword("mixin") + Keyword("compose") + expression + ";"
                                   | Keyword("mixin") + Keyword("remove") + expression + ";"
                                   | Keyword("mixin") + Keyword("macro") + expression + ";"
                                   | Keyword("mixin") + Keyword("child") + expression + ";"
                                   | Keyword("mixin") + Keyword("clone") + ";"
                                   | Keyword("mixin") + expression + ";";

            enum_item.Rule = identifier_raw + "=" + conditional_expression
                             | identifier_raw;

            enum_item_list.Rule = MakeStarRule(enum_item_list, ToTerm(","), enum_item);

            enum_block.Rule = attribute_qualifier_pre + Keyword("enum") + identifier_raw + "{" + enum_item_list + "}";

            statement_raw.Rule |= mixin_statement | using_statement | using_params_statement;

            toplevel_declaration_block.Rule = "{" + toplevel_declaration_list + "}";

            namespace_block.Rule = Keyword("namespace") + identifier_or_dot + toplevel_declaration_block;

            toplevel_declaration.Rule |= params_block | shader_block | enum_block | namespace_block | using_statement;

            // override class specifier
            class_specifier.Rule = Keyword("class") + class_identifier_or_generic + class_base_type + "{" + scope_declaration.ListOpt() + "}";
            class_identifier_or_generic.Rule = identifier + new IdentifierResolverHint(true)
                                                | class_identifier_generic + this.ReduceHere();
            class_identifier_generic.Rule = identifier + new GenericResolverHint(_skipTokensInPreview) + "<" + class_identifier_generic_parameter_list + ">";
            class_identifier_generic_parameter_list.Rule = MakePlusRule(class_identifier_generic_parameter_list, ToTerm(","), class_identifier_sub_generic);
            class_identifier_sub_generic.Rule = type + identifier;
            class_identifier_sub_generic.AstNodeCreator = CreateClassIdentifierSubGenericAst;
        }
    }
}