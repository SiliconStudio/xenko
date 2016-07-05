



using SiliconStudio.Shaders.Ast;
using SiliconStudio.Shaders.Visitor;

namespace SiliconStudio.Shaders.Visitor
{
    public partial class ShaderVisitor<TResult>
    {
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Xenko.ClassIdentifierGeneric classIdentifierGeneric)
        {
            return DefaultVisit(classIdentifierGeneric);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Xenko.EnumType enumType)
        {
            return DefaultVisit(enumType);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Xenko.ForEachStatement forEachStatement)
        {
            return DefaultVisit(forEachStatement);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Xenko.ImportBlockStatement importBlockStatement)
        {
            return DefaultVisit(importBlockStatement);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Xenko.LinkType linkType)
        {
            return DefaultVisit(linkType);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Xenko.LiteralIdentifier literalIdentifier)
        {
            return DefaultVisit(literalIdentifier);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Xenko.MemberName memberName)
        {
            return DefaultVisit(memberName);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Xenko.MixinStatement mixinStatement)
        {
            return DefaultVisit(mixinStatement);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Xenko.NamespaceBlock namespaceBlock)
        {
            return DefaultVisit(namespaceBlock);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Xenko.ParametersBlock parametersBlock)
        {
            return DefaultVisit(parametersBlock);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Xenko.SemanticType semanticType)
        {
            return DefaultVisit(semanticType);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Xenko.ShaderBlock shaderBlock)
        {
            return DefaultVisit(shaderBlock);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Xenko.ShaderClassType shaderClassType)
        {
            return DefaultVisit(shaderClassType);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Xenko.ShaderRootClassType shaderRootClassType)
        {
            return DefaultVisit(shaderRootClassType);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Xenko.ShaderTypeName shaderTypeName)
        {
            return DefaultVisit(shaderTypeName);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Xenko.TypeIdentifier typeIdentifier)
        {
            return DefaultVisit(typeIdentifier);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Xenko.UsingParametersStatement usingParametersStatement)
        {
            return DefaultVisit(usingParametersStatement);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Xenko.UsingStatement usingStatement)
        {
            return DefaultVisit(usingStatement);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Xenko.VarType varType)
        {
            return DefaultVisit(varType);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Xenko.XenkoConstantBufferType xenkoConstantBufferType)
        {
            return DefaultVisit(xenkoConstantBufferType);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.ArrayInitializerExpression arrayInitializerExpression)
        {
            return DefaultVisit(arrayInitializerExpression);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.ArrayType arrayType)
        {
            return DefaultVisit(arrayType);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.AssignmentExpression assignmentExpression)
        {
            return DefaultVisit(assignmentExpression);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.BinaryExpression binaryExpression)
        {
            return DefaultVisit(binaryExpression);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.BlockStatement blockStatement)
        {
            return DefaultVisit(blockStatement);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.CaseStatement caseStatement)
        {
            return DefaultVisit(caseStatement);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.CompositeEnum compositeEnum)
        {
            return DefaultVisit(compositeEnum);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.ConditionalExpression conditionalExpression)
        {
            return DefaultVisit(conditionalExpression);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.EmptyStatement emptyStatement)
        {
            return DefaultVisit(emptyStatement);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.EmptyExpression emptyExpression)
        {
            return DefaultVisit(emptyExpression);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Glsl.LayoutKeyValue layoutKeyValue)
        {
            return DefaultVisit(layoutKeyValue);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Glsl.LayoutQualifier layoutQualifier)
        {
            return DefaultVisit(layoutQualifier);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Glsl.InterfaceType interfaceType)
        {
            return DefaultVisit(interfaceType);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Hlsl.ClassType classType)
        {
            return DefaultVisit(classType);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Hlsl.IdentifierGeneric identifierGeneric)
        {
            return DefaultVisit(identifierGeneric);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Hlsl.IdentifierNs identifierNs)
        {
            return DefaultVisit(identifierNs);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Hlsl.IdentifierDot identifierDot)
        {
            return DefaultVisit(identifierDot);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Hlsl.TextureType textureType)
        {
            return DefaultVisit(textureType);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Hlsl.Annotations annotations)
        {
            return DefaultVisit(annotations);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Hlsl.AsmExpression asmExpression)
        {
            return DefaultVisit(asmExpression);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Hlsl.AttributeDeclaration attributeDeclaration)
        {
            return DefaultVisit(attributeDeclaration);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Hlsl.CastExpression castExpression)
        {
            return DefaultVisit(castExpression);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Hlsl.CompileExpression compileExpression)
        {
            return DefaultVisit(compileExpression);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Hlsl.ConstantBuffer constantBuffer)
        {
            return DefaultVisit(constantBuffer);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Hlsl.ConstantBufferType constantBufferType)
        {
            return DefaultVisit(constantBufferType);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Hlsl.InterfaceType interfaceType)
        {
            return DefaultVisit(interfaceType);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Hlsl.PackOffset packOffset)
        {
            return DefaultVisit(packOffset);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Hlsl.Pass pass)
        {
            return DefaultVisit(pass);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Hlsl.RegisterLocation registerLocation)
        {
            return DefaultVisit(registerLocation);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Hlsl.Semantic semantic)
        {
            return DefaultVisit(semantic);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Hlsl.StateExpression stateExpression)
        {
            return DefaultVisit(stateExpression);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Hlsl.StateInitializer stateInitializer)
        {
            return DefaultVisit(stateInitializer);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Hlsl.Technique technique)
        {
            return DefaultVisit(technique);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Hlsl.Typedef typedef)
        {
            return DefaultVisit(typedef);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.ExpressionList expressionList)
        {
            return DefaultVisit(expressionList);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.GenericDeclaration genericDeclaration)
        {
            return DefaultVisit(genericDeclaration);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.GenericParameterType genericParameterType)
        {
            return DefaultVisit(genericParameterType);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.DeclarationStatement declarationStatement)
        {
            return DefaultVisit(declarationStatement);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.ExpressionStatement expressionStatement)
        {
            return DefaultVisit(expressionStatement);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.ForStatement forStatement)
        {
            return DefaultVisit(forStatement);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.GenericType genericType)
        {
            return DefaultVisit(genericType);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Identifier identifier)
        {
            return DefaultVisit(identifier);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.IfStatement ifStatement)
        {
            return DefaultVisit(ifStatement);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.IndexerExpression indexerExpression)
        {
            return DefaultVisit(indexerExpression);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.KeywordExpression keywordExpression)
        {
            return DefaultVisit(keywordExpression);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Literal literal)
        {
            return DefaultVisit(literal);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.LiteralExpression literalExpression)
        {
            return DefaultVisit(literalExpression);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.MatrixType matrixType)
        {
            return DefaultVisit(matrixType);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.MemberReferenceExpression memberReferenceExpression)
        {
            return DefaultVisit(memberReferenceExpression);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.MethodDeclaration methodDeclaration)
        {
            return DefaultVisit(methodDeclaration);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.MethodDefinition methodDefinition)
        {
            return DefaultVisit(methodDefinition);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.MethodInvocationExpression methodInvocationExpression)
        {
            return DefaultVisit(methodInvocationExpression);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.ObjectType objectType)
        {
            return DefaultVisit(objectType);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Parameter parameter)
        {
            return DefaultVisit(parameter);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.ParenthesizedExpression parenthesizedExpression)
        {
            return DefaultVisit(parenthesizedExpression);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Qualifier qualifier)
        {
            return DefaultVisit(qualifier);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.ReturnStatement returnStatement)
        {
            return DefaultVisit(returnStatement);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.ScalarType scalarType)
        {
            return DefaultVisit(scalarType);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Shader shader)
        {
            return DefaultVisit(shader);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.StatementList statementList)
        {
            return DefaultVisit(statementList);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.StructType structType)
        {
            return DefaultVisit(structType);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.SwitchCaseGroup switchCaseGroup)
        {
            return DefaultVisit(switchCaseGroup);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.SwitchStatement switchStatement)
        {
            return DefaultVisit(switchStatement);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.TypeName typeName)
        {
            return DefaultVisit(typeName);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.TypeReferenceExpression typeReferenceExpression)
        {
            return DefaultVisit(typeReferenceExpression);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.UnaryExpression unaryExpression)
        {
            return DefaultVisit(unaryExpression);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.Variable variable)
        {
            return DefaultVisit(variable);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.VariableReferenceExpression variableReferenceExpression)
        {
            return DefaultVisit(variableReferenceExpression);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.VectorType vectorType)
        {
            return DefaultVisit(vectorType);
        }
        public virtual TResult Visit(SiliconStudio.Shaders.Ast.WhileStatement whileStatement)
        {
            return DefaultVisit(whileStatement);
        }
    }

    public partial class ShaderRewriter
    {
        public override Node Visit(SiliconStudio.Shaders.Ast.Xenko.ClassIdentifierGeneric classIdentifierGeneric)
        {
            VisitList(classIdentifierGeneric.Indices);
            VisitList(classIdentifierGeneric.Generics);
            return base.Visit(classIdentifierGeneric);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Xenko.EnumType enumType)
        {
            VisitList(enumType.Attributes);
            enumType.Name = (SiliconStudio.Shaders.Ast.Identifier)VisitDynamic(enumType.Name);
            enumType.Qualifiers = (SiliconStudio.Shaders.Ast.Qualifier)VisitDynamic(enumType.Qualifiers);
            VisitList(enumType.Values);
            return base.Visit(enumType);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Xenko.ForEachStatement forEachStatement)
        {
            VisitList(forEachStatement.Attributes);
            forEachStatement.Collection = (SiliconStudio.Shaders.Ast.Expression)VisitDynamic(forEachStatement.Collection);
            forEachStatement.Variable = (SiliconStudio.Shaders.Ast.Variable)VisitDynamic(forEachStatement.Variable);
            forEachStatement.Body = (SiliconStudio.Shaders.Ast.Statement)VisitDynamic(forEachStatement.Body);
            return base.Visit(forEachStatement);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Xenko.ImportBlockStatement importBlockStatement)
        {
            VisitList(importBlockStatement.Attributes);
            importBlockStatement.Statements = (SiliconStudio.Shaders.Ast.StatementList)VisitDynamic(importBlockStatement.Statements);
            return base.Visit(importBlockStatement);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Xenko.LinkType linkType)
        {
            VisitList(linkType.Attributes);
            linkType.Name = (SiliconStudio.Shaders.Ast.Identifier)VisitDynamic(linkType.Name);
            linkType.Qualifiers = (SiliconStudio.Shaders.Ast.Qualifier)VisitDynamic(linkType.Qualifiers);
            return base.Visit(linkType);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Xenko.LiteralIdentifier literalIdentifier)
        {
            VisitList(literalIdentifier.Indices);
            literalIdentifier.Value = (SiliconStudio.Shaders.Ast.Literal)VisitDynamic(literalIdentifier.Value);
            return base.Visit(literalIdentifier);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Xenko.MemberName memberName)
        {
            VisitList(memberName.Attributes);
            memberName.Name = (SiliconStudio.Shaders.Ast.Identifier)VisitDynamic(memberName.Name);
            memberName.Qualifiers = (SiliconStudio.Shaders.Ast.Qualifier)VisitDynamic(memberName.Qualifiers);
            return base.Visit(memberName);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Xenko.MixinStatement mixinStatement)
        {
            VisitList(mixinStatement.Attributes);
            mixinStatement.Value = (SiliconStudio.Shaders.Ast.Expression)VisitDynamic(mixinStatement.Value);
            return base.Visit(mixinStatement);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Xenko.NamespaceBlock namespaceBlock)
        {
            VisitList(namespaceBlock.Attributes);
            namespaceBlock.Name = (SiliconStudio.Shaders.Ast.Identifier)VisitDynamic(namespaceBlock.Name);
            namespaceBlock.Qualifiers = (SiliconStudio.Shaders.Ast.Qualifier)VisitDynamic(namespaceBlock.Qualifiers);
            VisitList(namespaceBlock.Body);
            return base.Visit(namespaceBlock);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Xenko.ParametersBlock parametersBlock)
        {
            parametersBlock.Name = (SiliconStudio.Shaders.Ast.Identifier)VisitDynamic(parametersBlock.Name);
            parametersBlock.Body = (SiliconStudio.Shaders.Ast.BlockStatement)VisitDynamic(parametersBlock.Body);
            return base.Visit(parametersBlock);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Xenko.SemanticType semanticType)
        {
            VisitList(semanticType.Attributes);
            semanticType.Name = (SiliconStudio.Shaders.Ast.Identifier)VisitDynamic(semanticType.Name);
            semanticType.Qualifiers = (SiliconStudio.Shaders.Ast.Qualifier)VisitDynamic(semanticType.Qualifiers);
            return base.Visit(semanticType);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Xenko.ShaderBlock shaderBlock)
        {
            VisitList(shaderBlock.Attributes);
            shaderBlock.Name = (SiliconStudio.Shaders.Ast.Identifier)VisitDynamic(shaderBlock.Name);
            shaderBlock.Qualifiers = (SiliconStudio.Shaders.Ast.Qualifier)VisitDynamic(shaderBlock.Qualifiers);
            shaderBlock.Body = (SiliconStudio.Shaders.Ast.BlockStatement)VisitDynamic(shaderBlock.Body);
            return base.Visit(shaderBlock);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Xenko.ShaderClassType shaderClassType)
        {
            VisitList(shaderClassType.Attributes);
            shaderClassType.Name = (SiliconStudio.Shaders.Ast.Identifier)VisitDynamic(shaderClassType.Name);
            shaderClassType.Qualifiers = (SiliconStudio.Shaders.Ast.Qualifier)VisitDynamic(shaderClassType.Qualifiers);
            VisitList(shaderClassType.BaseClasses);
            VisitList(shaderClassType.GenericParameters);
            VisitList(shaderClassType.GenericArguments);
            VisitList(shaderClassType.Members);
            VisitList(shaderClassType.ShaderGenerics);
            return base.Visit(shaderClassType);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Xenko.ShaderRootClassType shaderRootClassType)
        {
            VisitList(shaderRootClassType.Attributes);
            shaderRootClassType.Name = (SiliconStudio.Shaders.Ast.Identifier)VisitDynamic(shaderRootClassType.Name);
            shaderRootClassType.Qualifiers = (SiliconStudio.Shaders.Ast.Qualifier)VisitDynamic(shaderRootClassType.Qualifiers);
            VisitList(shaderRootClassType.BaseClasses);
            VisitList(shaderRootClassType.GenericParameters);
            VisitList(shaderRootClassType.GenericArguments);
            VisitList(shaderRootClassType.Members);
            VisitList(shaderRootClassType.ShaderGenerics);
            return base.Visit(shaderRootClassType);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Xenko.ShaderTypeName shaderTypeName)
        {
            VisitList(shaderTypeName.Attributes);
            shaderTypeName.Name = (SiliconStudio.Shaders.Ast.Identifier)VisitDynamic(shaderTypeName.Name);
            shaderTypeName.Qualifiers = (SiliconStudio.Shaders.Ast.Qualifier)VisitDynamic(shaderTypeName.Qualifiers);
            return base.Visit(shaderTypeName);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Xenko.TypeIdentifier typeIdentifier)
        {
            VisitList(typeIdentifier.Indices);
            typeIdentifier.Type = (SiliconStudio.Shaders.Ast.TypeBase)VisitDynamic(typeIdentifier.Type);
            return base.Visit(typeIdentifier);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Xenko.UsingParametersStatement usingParametersStatement)
        {
            VisitList(usingParametersStatement.Attributes);
            usingParametersStatement.Name = (SiliconStudio.Shaders.Ast.Expression)VisitDynamic(usingParametersStatement.Name);
            usingParametersStatement.Body = (SiliconStudio.Shaders.Ast.BlockStatement)VisitDynamic(usingParametersStatement.Body);
            return base.Visit(usingParametersStatement);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Xenko.UsingStatement usingStatement)
        {
            VisitList(usingStatement.Attributes);
            usingStatement.Name = (SiliconStudio.Shaders.Ast.Identifier)VisitDynamic(usingStatement.Name);
            return base.Visit(usingStatement);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Xenko.VarType varType)
        {
            VisitList(varType.Attributes);
            varType.Name = (SiliconStudio.Shaders.Ast.Identifier)VisitDynamic(varType.Name);
            varType.Qualifiers = (SiliconStudio.Shaders.Ast.Qualifier)VisitDynamic(varType.Qualifiers);
            return base.Visit(varType);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Xenko.XenkoConstantBufferType xenkoConstantBufferType)
        {
            return base.Visit(xenkoConstantBufferType);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.ArrayInitializerExpression arrayInitializerExpression)
        {
            VisitList(arrayInitializerExpression.Items);
            return base.Visit(arrayInitializerExpression);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.ArrayType arrayType)
        {
            VisitList(arrayType.Attributes);
            arrayType.Name = (SiliconStudio.Shaders.Ast.Identifier)VisitDynamic(arrayType.Name);
            arrayType.Qualifiers = (SiliconStudio.Shaders.Ast.Qualifier)VisitDynamic(arrayType.Qualifiers);
            VisitList(arrayType.Dimensions);
            arrayType.Type = (SiliconStudio.Shaders.Ast.TypeBase)VisitDynamic(arrayType.Type);
            return base.Visit(arrayType);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.AssignmentExpression assignmentExpression)
        {
            assignmentExpression.Target = (SiliconStudio.Shaders.Ast.Expression)VisitDynamic(assignmentExpression.Target);
            assignmentExpression.Value = (SiliconStudio.Shaders.Ast.Expression)VisitDynamic(assignmentExpression.Value);
            return base.Visit(assignmentExpression);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.BinaryExpression binaryExpression)
        {
            binaryExpression.Left = (SiliconStudio.Shaders.Ast.Expression)VisitDynamic(binaryExpression.Left);
            binaryExpression.Right = (SiliconStudio.Shaders.Ast.Expression)VisitDynamic(binaryExpression.Right);
            return base.Visit(binaryExpression);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.BlockStatement blockStatement)
        {
            VisitList(blockStatement.Attributes);
            blockStatement.Statements = (SiliconStudio.Shaders.Ast.StatementList)VisitDynamic(blockStatement.Statements);
            return base.Visit(blockStatement);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.CaseStatement caseStatement)
        {
            VisitList(caseStatement.Attributes);
            caseStatement.Case = (SiliconStudio.Shaders.Ast.Expression)VisitDynamic(caseStatement.Case);
            return base.Visit(caseStatement);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.CompositeEnum compositeEnum)
        {
            return base.Visit(compositeEnum);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.ConditionalExpression conditionalExpression)
        {
            conditionalExpression.Condition = (SiliconStudio.Shaders.Ast.Expression)VisitDynamic(conditionalExpression.Condition);
            conditionalExpression.Left = (SiliconStudio.Shaders.Ast.Expression)VisitDynamic(conditionalExpression.Left);
            conditionalExpression.Right = (SiliconStudio.Shaders.Ast.Expression)VisitDynamic(conditionalExpression.Right);
            return base.Visit(conditionalExpression);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.EmptyStatement emptyStatement)
        {
            VisitList(emptyStatement.Attributes);
            return base.Visit(emptyStatement);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.EmptyExpression emptyExpression)
        {
            return base.Visit(emptyExpression);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Glsl.LayoutKeyValue layoutKeyValue)
        {
            layoutKeyValue.Name = (SiliconStudio.Shaders.Ast.Identifier)VisitDynamic(layoutKeyValue.Name);
            layoutKeyValue.Value = (SiliconStudio.Shaders.Ast.LiteralExpression)VisitDynamic(layoutKeyValue.Value);
            return base.Visit(layoutKeyValue);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Glsl.LayoutQualifier layoutQualifier)
        {
            VisitList(layoutQualifier.Layouts);
            return base.Visit(layoutQualifier);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Glsl.InterfaceType interfaceType)
        {
            VisitList(interfaceType.Attributes);
            interfaceType.Name = (SiliconStudio.Shaders.Ast.Identifier)VisitDynamic(interfaceType.Name);
            interfaceType.Qualifiers = (SiliconStudio.Shaders.Ast.Qualifier)VisitDynamic(interfaceType.Qualifiers);
            VisitList(interfaceType.Fields);
            return base.Visit(interfaceType);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Hlsl.ClassType classType)
        {
            VisitList(classType.Attributes);
            classType.Name = (SiliconStudio.Shaders.Ast.Identifier)VisitDynamic(classType.Name);
            classType.Qualifiers = (SiliconStudio.Shaders.Ast.Qualifier)VisitDynamic(classType.Qualifiers);
            VisitList(classType.BaseClasses);
            VisitList(classType.GenericParameters);
            VisitList(classType.GenericArguments);
            VisitList(classType.Members);
            return base.Visit(classType);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Hlsl.IdentifierGeneric identifierGeneric)
        {
            VisitList(identifierGeneric.Indices);
            VisitList(identifierGeneric.Identifiers);
            return base.Visit(identifierGeneric);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Hlsl.IdentifierNs identifierNs)
        {
            VisitList(identifierNs.Indices);
            VisitList(identifierNs.Identifiers);
            return base.Visit(identifierNs);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Hlsl.IdentifierDot identifierDot)
        {
            VisitList(identifierDot.Indices);
            VisitList(identifierDot.Identifiers);
            return base.Visit(identifierDot);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Hlsl.TextureType textureType)
        {
            VisitList(textureType.Attributes);
            textureType.Name = (SiliconStudio.Shaders.Ast.Identifier)VisitDynamic(textureType.Name);
            textureType.Qualifiers = (SiliconStudio.Shaders.Ast.Qualifier)VisitDynamic(textureType.Qualifiers);
            return base.Visit(textureType);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Hlsl.Annotations annotations)
        {
            VisitList(annotations.Variables);
            return base.Visit(annotations);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Hlsl.AsmExpression asmExpression)
        {
            return base.Visit(asmExpression);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Hlsl.AttributeDeclaration attributeDeclaration)
        {
            attributeDeclaration.Name = (SiliconStudio.Shaders.Ast.Identifier)VisitDynamic(attributeDeclaration.Name);
            VisitList(attributeDeclaration.Parameters);
            return base.Visit(attributeDeclaration);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Hlsl.CastExpression castExpression)
        {
            castExpression.From = (SiliconStudio.Shaders.Ast.Expression)VisitDynamic(castExpression.From);
            castExpression.Target = (SiliconStudio.Shaders.Ast.TypeBase)VisitDynamic(castExpression.Target);
            return base.Visit(castExpression);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Hlsl.CompileExpression compileExpression)
        {
            compileExpression.Function = (SiliconStudio.Shaders.Ast.Expression)VisitDynamic(compileExpression.Function);
            compileExpression.Profile = (SiliconStudio.Shaders.Ast.Identifier)VisitDynamic(compileExpression.Profile);
            return base.Visit(compileExpression);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Hlsl.ConstantBuffer constantBuffer)
        {
            VisitList(constantBuffer.Attributes);
            constantBuffer.Type = (SiliconStudio.Shaders.Ast.Hlsl.ConstantBufferType)VisitDynamic(constantBuffer.Type);
            VisitList(constantBuffer.Members);
            constantBuffer.Name = (SiliconStudio.Shaders.Ast.Identifier)VisitDynamic(constantBuffer.Name);
            constantBuffer.Register = (SiliconStudio.Shaders.Ast.Hlsl.RegisterLocation)VisitDynamic(constantBuffer.Register);
            constantBuffer.Qualifiers = (SiliconStudio.Shaders.Ast.Qualifier)VisitDynamic(constantBuffer.Qualifiers);
            return base.Visit(constantBuffer);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Hlsl.ConstantBufferType constantBufferType)
        {
            return base.Visit(constantBufferType);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Hlsl.InterfaceType interfaceType)
        {
            VisitList(interfaceType.Attributes);
            interfaceType.Name = (SiliconStudio.Shaders.Ast.Identifier)VisitDynamic(interfaceType.Name);
            interfaceType.Qualifiers = (SiliconStudio.Shaders.Ast.Qualifier)VisitDynamic(interfaceType.Qualifiers);
            VisitList(interfaceType.GenericParameters);
            VisitList(interfaceType.GenericArguments);
            VisitList(interfaceType.Methods);
            return base.Visit(interfaceType);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Hlsl.PackOffset packOffset)
        {
            packOffset.Value = (SiliconStudio.Shaders.Ast.Identifier)VisitDynamic(packOffset.Value);
            return base.Visit(packOffset);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Hlsl.Pass pass)
        {
            VisitList(pass.Attributes);
            VisitList(pass.Items);
            pass.Name = (SiliconStudio.Shaders.Ast.Identifier)VisitDynamic(pass.Name);
            return base.Visit(pass);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Hlsl.RegisterLocation registerLocation)
        {
            registerLocation.Profile = (SiliconStudio.Shaders.Ast.Identifier)VisitDynamic(registerLocation.Profile);
            registerLocation.Register = (SiliconStudio.Shaders.Ast.Identifier)VisitDynamic(registerLocation.Register);
            return base.Visit(registerLocation);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Hlsl.Semantic semantic)
        {
            semantic.Name = (SiliconStudio.Shaders.Ast.Identifier)VisitDynamic(semantic.Name);
            return base.Visit(semantic);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Hlsl.StateExpression stateExpression)
        {
            stateExpression.Initializer = (SiliconStudio.Shaders.Ast.Hlsl.StateInitializer)VisitDynamic(stateExpression.Initializer);
            stateExpression.StateType = (SiliconStudio.Shaders.Ast.TypeBase)VisitDynamic(stateExpression.StateType);
            return base.Visit(stateExpression);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Hlsl.StateInitializer stateInitializer)
        {
            VisitList(stateInitializer.Items);
            return base.Visit(stateInitializer);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Hlsl.Technique technique)
        {
            technique.Type = (SiliconStudio.Shaders.Ast.Identifier)VisitDynamic(technique.Type);
            VisitList(technique.Attributes);
            technique.Name = (SiliconStudio.Shaders.Ast.Identifier)VisitDynamic(technique.Name);
            VisitList(technique.Passes);
            return base.Visit(technique);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Hlsl.Typedef typedef)
        {
            VisitList(typedef.Attributes);
            typedef.Name = (SiliconStudio.Shaders.Ast.Identifier)VisitDynamic(typedef.Name);
            typedef.Qualifiers = (SiliconStudio.Shaders.Ast.Qualifier)VisitDynamic(typedef.Qualifiers);
            VisitList(typedef.SubDeclarators);
            typedef.Type = (SiliconStudio.Shaders.Ast.TypeBase)VisitDynamic(typedef.Type);
            return base.Visit(typedef);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.ExpressionList expressionList)
        {
            VisitList(expressionList.Expressions);
            return base.Visit(expressionList);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.GenericDeclaration genericDeclaration)
        {
            genericDeclaration.Name = (SiliconStudio.Shaders.Ast.Identifier)VisitDynamic(genericDeclaration.Name);
            return base.Visit(genericDeclaration);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.GenericParameterType genericParameterType)
        {
            VisitList(genericParameterType.Attributes);
            genericParameterType.Name = (SiliconStudio.Shaders.Ast.Identifier)VisitDynamic(genericParameterType.Name);
            genericParameterType.Qualifiers = (SiliconStudio.Shaders.Ast.Qualifier)VisitDynamic(genericParameterType.Qualifiers);
            return base.Visit(genericParameterType);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.DeclarationStatement declarationStatement)
        {
            VisitList(declarationStatement.Attributes);
            declarationStatement.Content = (SiliconStudio.Shaders.Ast.Node)VisitDynamic(declarationStatement.Content);
            return base.Visit(declarationStatement);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.ExpressionStatement expressionStatement)
        {
            VisitList(expressionStatement.Attributes);
            expressionStatement.Expression = (SiliconStudio.Shaders.Ast.Expression)VisitDynamic(expressionStatement.Expression);
            return base.Visit(expressionStatement);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.ForStatement forStatement)
        {
            VisitList(forStatement.Attributes);
            forStatement.Start = (SiliconStudio.Shaders.Ast.Statement)VisitDynamic(forStatement.Start);
            forStatement.Condition = (SiliconStudio.Shaders.Ast.Expression)VisitDynamic(forStatement.Condition);
            forStatement.Next = (SiliconStudio.Shaders.Ast.Expression)VisitDynamic(forStatement.Next);
            forStatement.Body = (SiliconStudio.Shaders.Ast.Statement)VisitDynamic(forStatement.Body);
            return base.Visit(forStatement);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.GenericType genericType)
        {
            VisitList(genericType.Attributes);
            genericType.Name = (SiliconStudio.Shaders.Ast.Identifier)VisitDynamic(genericType.Name);
            genericType.Qualifiers = (SiliconStudio.Shaders.Ast.Qualifier)VisitDynamic(genericType.Qualifiers);
            VisitList(genericType.Parameters);
            return base.Visit(genericType);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Identifier identifier)
        {
            VisitList(identifier.Indices);
            return base.Visit(identifier);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.IfStatement ifStatement)
        {
            VisitList(ifStatement.Attributes);
            ifStatement.Condition = (SiliconStudio.Shaders.Ast.Expression)VisitDynamic(ifStatement.Condition);
            ifStatement.Else = (SiliconStudio.Shaders.Ast.Statement)VisitDynamic(ifStatement.Else);
            ifStatement.Then = (SiliconStudio.Shaders.Ast.Statement)VisitDynamic(ifStatement.Then);
            return base.Visit(ifStatement);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.IndexerExpression indexerExpression)
        {
            indexerExpression.Index = (SiliconStudio.Shaders.Ast.Expression)VisitDynamic(indexerExpression.Index);
            indexerExpression.Target = (SiliconStudio.Shaders.Ast.Expression)VisitDynamic(indexerExpression.Target);
            return base.Visit(indexerExpression);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.KeywordExpression keywordExpression)
        {
            keywordExpression.Name = (SiliconStudio.Shaders.Ast.Identifier)VisitDynamic(keywordExpression.Name);
            return base.Visit(keywordExpression);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Literal literal)
        {
            VisitList(literal.SubLiterals);
            return base.Visit(literal);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.LiteralExpression literalExpression)
        {
            literalExpression.Literal = (SiliconStudio.Shaders.Ast.Literal)VisitDynamic(literalExpression.Literal);
            return base.Visit(literalExpression);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.MatrixType matrixType)
        {
            VisitList(matrixType.Attributes);
            matrixType.Name = (SiliconStudio.Shaders.Ast.Identifier)VisitDynamic(matrixType.Name);
            matrixType.Qualifiers = (SiliconStudio.Shaders.Ast.Qualifier)VisitDynamic(matrixType.Qualifiers);
            VisitList(matrixType.Parameters);
            matrixType.Type = (SiliconStudio.Shaders.Ast.TypeBase)VisitDynamic(matrixType.Type);
            return base.Visit(matrixType);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.MemberReferenceExpression memberReferenceExpression)
        {
            memberReferenceExpression.Member = (SiliconStudio.Shaders.Ast.Identifier)VisitDynamic(memberReferenceExpression.Member);
            memberReferenceExpression.Target = (SiliconStudio.Shaders.Ast.Expression)VisitDynamic(memberReferenceExpression.Target);
            return base.Visit(memberReferenceExpression);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.MethodDeclaration methodDeclaration)
        {
            VisitList(methodDeclaration.Attributes);
            methodDeclaration.Name = (SiliconStudio.Shaders.Ast.Identifier)VisitDynamic(methodDeclaration.Name);
            VisitList(methodDeclaration.Parameters);
            methodDeclaration.Qualifiers = (SiliconStudio.Shaders.Ast.Qualifier)VisitDynamic(methodDeclaration.Qualifiers);
            methodDeclaration.ReturnType = (SiliconStudio.Shaders.Ast.TypeBase)VisitDynamic(methodDeclaration.ReturnType);
            return base.Visit(methodDeclaration);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.MethodDefinition methodDefinition)
        {
            VisitList(methodDefinition.Attributes);
            methodDefinition.Name = (SiliconStudio.Shaders.Ast.Identifier)VisitDynamic(methodDefinition.Name);
            VisitList(methodDefinition.Parameters);
            methodDefinition.Qualifiers = (SiliconStudio.Shaders.Ast.Qualifier)VisitDynamic(methodDefinition.Qualifiers);
            methodDefinition.ReturnType = (SiliconStudio.Shaders.Ast.TypeBase)VisitDynamic(methodDefinition.ReturnType);
            methodDefinition.Body = (SiliconStudio.Shaders.Ast.StatementList)VisitDynamic(methodDefinition.Body);
            return base.Visit(methodDefinition);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.MethodInvocationExpression methodInvocationExpression)
        {
            methodInvocationExpression.Target = (SiliconStudio.Shaders.Ast.Expression)VisitDynamic(methodInvocationExpression.Target);
            VisitList(methodInvocationExpression.Arguments);
            return base.Visit(methodInvocationExpression);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.ObjectType objectType)
        {
            VisitList(objectType.Attributes);
            objectType.Name = (SiliconStudio.Shaders.Ast.Identifier)VisitDynamic(objectType.Name);
            objectType.Qualifiers = (SiliconStudio.Shaders.Ast.Qualifier)VisitDynamic(objectType.Qualifiers);
            return base.Visit(objectType);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Parameter parameter)
        {
            VisitList(parameter.Attributes);
            parameter.Qualifiers = (SiliconStudio.Shaders.Ast.Qualifier)VisitDynamic(parameter.Qualifiers);
            parameter.Type = (SiliconStudio.Shaders.Ast.TypeBase)VisitDynamic(parameter.Type);
            parameter.InitialValue = (SiliconStudio.Shaders.Ast.Expression)VisitDynamic(parameter.InitialValue);
            parameter.Name = (SiliconStudio.Shaders.Ast.Identifier)VisitDynamic(parameter.Name);
            VisitList(parameter.SubVariables);
            return base.Visit(parameter);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.ParenthesizedExpression parenthesizedExpression)
        {
            parenthesizedExpression.Content = (SiliconStudio.Shaders.Ast.Expression)VisitDynamic(parenthesizedExpression.Content);
            return base.Visit(parenthesizedExpression);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Qualifier qualifier)
        {
            return base.Visit(qualifier);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.ReturnStatement returnStatement)
        {
            VisitList(returnStatement.Attributes);
            returnStatement.Value = (SiliconStudio.Shaders.Ast.Expression)VisitDynamic(returnStatement.Value);
            return base.Visit(returnStatement);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.ScalarType scalarType)
        {
            VisitList(scalarType.Attributes);
            scalarType.Name = (SiliconStudio.Shaders.Ast.Identifier)VisitDynamic(scalarType.Name);
            scalarType.Qualifiers = (SiliconStudio.Shaders.Ast.Qualifier)VisitDynamic(scalarType.Qualifiers);
            return base.Visit(scalarType);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Shader shader)
        {
            VisitList(shader.Declarations);
            return base.Visit(shader);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.StatementList statementList)
        {
            VisitList(statementList.Attributes);
            VisitList(statementList.Statements);
            return base.Visit(statementList);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.StructType structType)
        {
            VisitList(structType.Attributes);
            structType.Name = (SiliconStudio.Shaders.Ast.Identifier)VisitDynamic(structType.Name);
            structType.Qualifiers = (SiliconStudio.Shaders.Ast.Qualifier)VisitDynamic(structType.Qualifiers);
            VisitList(structType.Fields);
            return base.Visit(structType);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.SwitchCaseGroup switchCaseGroup)
        {
            VisitList(switchCaseGroup.Cases);
            switchCaseGroup.Statements = (SiliconStudio.Shaders.Ast.StatementList)VisitDynamic(switchCaseGroup.Statements);
            return base.Visit(switchCaseGroup);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.SwitchStatement switchStatement)
        {
            VisitList(switchStatement.Attributes);
            switchStatement.Condition = (SiliconStudio.Shaders.Ast.Expression)VisitDynamic(switchStatement.Condition);
            VisitList(switchStatement.Groups);
            return base.Visit(switchStatement);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.TypeName typeName)
        {
            VisitList(typeName.Attributes);
            typeName.Name = (SiliconStudio.Shaders.Ast.Identifier)VisitDynamic(typeName.Name);
            typeName.Qualifiers = (SiliconStudio.Shaders.Ast.Qualifier)VisitDynamic(typeName.Qualifiers);
            return base.Visit(typeName);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.TypeReferenceExpression typeReferenceExpression)
        {
            typeReferenceExpression.Type = (SiliconStudio.Shaders.Ast.TypeBase)VisitDynamic(typeReferenceExpression.Type);
            return base.Visit(typeReferenceExpression);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.UnaryExpression unaryExpression)
        {
            unaryExpression.Expression = (SiliconStudio.Shaders.Ast.Expression)VisitDynamic(unaryExpression.Expression);
            return base.Visit(unaryExpression);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Variable variable)
        {
            VisitList(variable.Attributes);
            variable.Qualifiers = (SiliconStudio.Shaders.Ast.Qualifier)VisitDynamic(variable.Qualifiers);
            variable.Type = (SiliconStudio.Shaders.Ast.TypeBase)VisitDynamic(variable.Type);
            variable.InitialValue = (SiliconStudio.Shaders.Ast.Expression)VisitDynamic(variable.InitialValue);
            variable.Name = (SiliconStudio.Shaders.Ast.Identifier)VisitDynamic(variable.Name);
            VisitList(variable.SubVariables);
            return base.Visit(variable);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.VariableReferenceExpression variableReferenceExpression)
        {
            variableReferenceExpression.Name = (SiliconStudio.Shaders.Ast.Identifier)VisitDynamic(variableReferenceExpression.Name);
            return base.Visit(variableReferenceExpression);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.VectorType vectorType)
        {
            VisitList(vectorType.Attributes);
            vectorType.Name = (SiliconStudio.Shaders.Ast.Identifier)VisitDynamic(vectorType.Name);
            vectorType.Qualifiers = (SiliconStudio.Shaders.Ast.Qualifier)VisitDynamic(vectorType.Qualifiers);
            VisitList(vectorType.Parameters);
            vectorType.Type = (SiliconStudio.Shaders.Ast.TypeBase)VisitDynamic(vectorType.Type);
            return base.Visit(vectorType);
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.WhileStatement whileStatement)
        {
            VisitList(whileStatement.Attributes);
            whileStatement.Condition = (SiliconStudio.Shaders.Ast.Expression)VisitDynamic(whileStatement.Condition);
            whileStatement.Statement = (SiliconStudio.Shaders.Ast.Statement)VisitDynamic(whileStatement.Statement);
            return base.Visit(whileStatement);
        }
    }

    public partial class ShaderCloner
    {
        public override Node Visit(SiliconStudio.Shaders.Ast.Xenko.ClassIdentifierGeneric classIdentifierGeneric)
        {
            classIdentifierGeneric = (SiliconStudio.Shaders.Ast.Xenko.ClassIdentifierGeneric)base.Visit(classIdentifierGeneric);
            return new SiliconStudio.Shaders.Ast.Xenko.ClassIdentifierGeneric
            {
                Span = classIdentifierGeneric.Span,
                Indices = classIdentifierGeneric.Indices,
                IsSpecialReference = classIdentifierGeneric.IsSpecialReference,
                Text = classIdentifierGeneric.Text,
                Generics = classIdentifierGeneric.Generics,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Xenko.EnumType enumType)
        {
            enumType = (SiliconStudio.Shaders.Ast.Xenko.EnumType)base.Visit(enumType);
            return new SiliconStudio.Shaders.Ast.Xenko.EnumType
            {
                Span = enumType.Span,
                Attributes = enumType.Attributes,
                TypeInference = enumType.TypeInference,
                Name = enumType.Name,
                Qualifiers = enumType.Qualifiers,
                IsBuiltIn = enumType.IsBuiltIn,
                Values = enumType.Values,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Xenko.ForEachStatement forEachStatement)
        {
            forEachStatement = (SiliconStudio.Shaders.Ast.Xenko.ForEachStatement)base.Visit(forEachStatement);
            return new SiliconStudio.Shaders.Ast.Xenko.ForEachStatement
            {
                Span = forEachStatement.Span,
                Attributes = forEachStatement.Attributes,
                Collection = forEachStatement.Collection,
                Variable = forEachStatement.Variable,
                Body = forEachStatement.Body,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Xenko.ImportBlockStatement importBlockStatement)
        {
            importBlockStatement = (SiliconStudio.Shaders.Ast.Xenko.ImportBlockStatement)base.Visit(importBlockStatement);
            return new SiliconStudio.Shaders.Ast.Xenko.ImportBlockStatement
            {
                Span = importBlockStatement.Span,
                Attributes = importBlockStatement.Attributes,
                Statements = importBlockStatement.Statements,
                Name = importBlockStatement.Name,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Xenko.LinkType linkType)
        {
            linkType = (SiliconStudio.Shaders.Ast.Xenko.LinkType)base.Visit(linkType);
            return new SiliconStudio.Shaders.Ast.Xenko.LinkType
            {
                Span = linkType.Span,
                Attributes = linkType.Attributes,
                TypeInference = linkType.TypeInference,
                Name = linkType.Name,
                Qualifiers = linkType.Qualifiers,
                IsBuiltIn = linkType.IsBuiltIn,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Xenko.LiteralIdentifier literalIdentifier)
        {
            literalIdentifier = (SiliconStudio.Shaders.Ast.Xenko.LiteralIdentifier)base.Visit(literalIdentifier);
            return new SiliconStudio.Shaders.Ast.Xenko.LiteralIdentifier
            {
                Span = literalIdentifier.Span,
                Indices = literalIdentifier.Indices,
                IsSpecialReference = literalIdentifier.IsSpecialReference,
                Text = literalIdentifier.Text,
                Value = literalIdentifier.Value,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Xenko.MemberName memberName)
        {
            memberName = (SiliconStudio.Shaders.Ast.Xenko.MemberName)base.Visit(memberName);
            return new SiliconStudio.Shaders.Ast.Xenko.MemberName
            {
                Span = memberName.Span,
                Attributes = memberName.Attributes,
                TypeInference = memberName.TypeInference,
                Name = memberName.Name,
                Qualifiers = memberName.Qualifiers,
                IsBuiltIn = memberName.IsBuiltIn,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Xenko.MixinStatement mixinStatement)
        {
            mixinStatement = (SiliconStudio.Shaders.Ast.Xenko.MixinStatement)base.Visit(mixinStatement);
            return new SiliconStudio.Shaders.Ast.Xenko.MixinStatement
            {
                Span = mixinStatement.Span,
                Attributes = mixinStatement.Attributes,
                Type = mixinStatement.Type,
                Value = mixinStatement.Value,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Xenko.NamespaceBlock namespaceBlock)
        {
            namespaceBlock = (SiliconStudio.Shaders.Ast.Xenko.NamespaceBlock)base.Visit(namespaceBlock);
            return new SiliconStudio.Shaders.Ast.Xenko.NamespaceBlock
            {
                Span = namespaceBlock.Span,
                Attributes = namespaceBlock.Attributes,
                TypeInference = namespaceBlock.TypeInference,
                Name = namespaceBlock.Name,
                Qualifiers = namespaceBlock.Qualifiers,
                IsBuiltIn = namespaceBlock.IsBuiltIn,
                Body = namespaceBlock.Body,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Xenko.ParametersBlock parametersBlock)
        {
            parametersBlock = (SiliconStudio.Shaders.Ast.Xenko.ParametersBlock)base.Visit(parametersBlock);
            return new SiliconStudio.Shaders.Ast.Xenko.ParametersBlock
            {
                Span = parametersBlock.Span,
                Name = parametersBlock.Name,
                Body = parametersBlock.Body,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Xenko.SemanticType semanticType)
        {
            semanticType = (SiliconStudio.Shaders.Ast.Xenko.SemanticType)base.Visit(semanticType);
            return new SiliconStudio.Shaders.Ast.Xenko.SemanticType
            {
                Span = semanticType.Span,
                Attributes = semanticType.Attributes,
                TypeInference = semanticType.TypeInference,
                Name = semanticType.Name,
                Qualifiers = semanticType.Qualifiers,
                IsBuiltIn = semanticType.IsBuiltIn,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Xenko.ShaderBlock shaderBlock)
        {
            shaderBlock = (SiliconStudio.Shaders.Ast.Xenko.ShaderBlock)base.Visit(shaderBlock);
            return new SiliconStudio.Shaders.Ast.Xenko.ShaderBlock
            {
                Span = shaderBlock.Span,
                Attributes = shaderBlock.Attributes,
                TypeInference = shaderBlock.TypeInference,
                Name = shaderBlock.Name,
                Qualifiers = shaderBlock.Qualifiers,
                IsBuiltIn = shaderBlock.IsBuiltIn,
                IsPartial = shaderBlock.IsPartial,
                Body = shaderBlock.Body,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Xenko.ShaderClassType shaderClassType)
        {
            shaderClassType = (SiliconStudio.Shaders.Ast.Xenko.ShaderClassType)base.Visit(shaderClassType);
            return new SiliconStudio.Shaders.Ast.Xenko.ShaderClassType
            {
                Span = shaderClassType.Span,
                Attributes = shaderClassType.Attributes,
                TypeInference = shaderClassType.TypeInference,
                Name = shaderClassType.Name,
                Qualifiers = shaderClassType.Qualifiers,
                IsBuiltIn = shaderClassType.IsBuiltIn,
                AlternativeNames = shaderClassType.AlternativeNames,
                BaseClasses = shaderClassType.BaseClasses,
                GenericParameters = shaderClassType.GenericParameters,
                GenericArguments = shaderClassType.GenericArguments,
                Members = shaderClassType.Members,
                ShaderGenerics = shaderClassType.ShaderGenerics,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Xenko.ShaderRootClassType shaderRootClassType)
        {
            shaderRootClassType = (SiliconStudio.Shaders.Ast.Xenko.ShaderRootClassType)base.Visit(shaderRootClassType);
            return new SiliconStudio.Shaders.Ast.Xenko.ShaderRootClassType
            {
                Span = shaderRootClassType.Span,
                Attributes = shaderRootClassType.Attributes,
                TypeInference = shaderRootClassType.TypeInference,
                Name = shaderRootClassType.Name,
                Qualifiers = shaderRootClassType.Qualifiers,
                IsBuiltIn = shaderRootClassType.IsBuiltIn,
                AlternativeNames = shaderRootClassType.AlternativeNames,
                BaseClasses = shaderRootClassType.BaseClasses,
                GenericParameters = shaderRootClassType.GenericParameters,
                GenericArguments = shaderRootClassType.GenericArguments,
                Members = shaderRootClassType.Members,
                ShaderGenerics = shaderRootClassType.ShaderGenerics,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Xenko.ShaderTypeName shaderTypeName)
        {
            shaderTypeName = (SiliconStudio.Shaders.Ast.Xenko.ShaderTypeName)base.Visit(shaderTypeName);
            return new SiliconStudio.Shaders.Ast.Xenko.ShaderTypeName
            {
                Span = shaderTypeName.Span,
                Attributes = shaderTypeName.Attributes,
                TypeInference = shaderTypeName.TypeInference,
                Name = shaderTypeName.Name,
                Qualifiers = shaderTypeName.Qualifiers,
                IsBuiltIn = shaderTypeName.IsBuiltIn,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Xenko.TypeIdentifier typeIdentifier)
        {
            typeIdentifier = (SiliconStudio.Shaders.Ast.Xenko.TypeIdentifier)base.Visit(typeIdentifier);
            return new SiliconStudio.Shaders.Ast.Xenko.TypeIdentifier
            {
                Span = typeIdentifier.Span,
                Indices = typeIdentifier.Indices,
                IsSpecialReference = typeIdentifier.IsSpecialReference,
                Text = typeIdentifier.Text,
                Type = typeIdentifier.Type,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Xenko.UsingParametersStatement usingParametersStatement)
        {
            usingParametersStatement = (SiliconStudio.Shaders.Ast.Xenko.UsingParametersStatement)base.Visit(usingParametersStatement);
            return new SiliconStudio.Shaders.Ast.Xenko.UsingParametersStatement
            {
                Span = usingParametersStatement.Span,
                Attributes = usingParametersStatement.Attributes,
                Name = usingParametersStatement.Name,
                Body = usingParametersStatement.Body,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Xenko.UsingStatement usingStatement)
        {
            usingStatement = (SiliconStudio.Shaders.Ast.Xenko.UsingStatement)base.Visit(usingStatement);
            return new SiliconStudio.Shaders.Ast.Xenko.UsingStatement
            {
                Span = usingStatement.Span,
                Attributes = usingStatement.Attributes,
                Name = usingStatement.Name,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Xenko.VarType varType)
        {
            varType = (SiliconStudio.Shaders.Ast.Xenko.VarType)base.Visit(varType);
            return new SiliconStudio.Shaders.Ast.Xenko.VarType
            {
                Span = varType.Span,
                Attributes = varType.Attributes,
                TypeInference = varType.TypeInference,
                Name = varType.Name,
                Qualifiers = varType.Qualifiers,
                IsBuiltIn = varType.IsBuiltIn,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Xenko.XenkoConstantBufferType xenkoConstantBufferType)
        {
            xenkoConstantBufferType = (SiliconStudio.Shaders.Ast.Xenko.XenkoConstantBufferType)base.Visit(xenkoConstantBufferType);
            return new SiliconStudio.Shaders.Ast.Xenko.XenkoConstantBufferType
            {
                Span = xenkoConstantBufferType.Span,
                IsFlag = xenkoConstantBufferType.IsFlag,
                Key = xenkoConstantBufferType.Key,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.ArrayInitializerExpression arrayInitializerExpression)
        {
            arrayInitializerExpression = (SiliconStudio.Shaders.Ast.ArrayInitializerExpression)base.Visit(arrayInitializerExpression);
            return new SiliconStudio.Shaders.Ast.ArrayInitializerExpression
            {
                Span = arrayInitializerExpression.Span,
                TypeInference = arrayInitializerExpression.TypeInference,
                Items = arrayInitializerExpression.Items,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.ArrayType arrayType)
        {
            arrayType = (SiliconStudio.Shaders.Ast.ArrayType)base.Visit(arrayType);
            return new SiliconStudio.Shaders.Ast.ArrayType
            {
                Span = arrayType.Span,
                Attributes = arrayType.Attributes,
                TypeInference = arrayType.TypeInference,
                Name = arrayType.Name,
                Qualifiers = arrayType.Qualifiers,
                IsBuiltIn = arrayType.IsBuiltIn,
                Dimensions = arrayType.Dimensions,
                Type = arrayType.Type,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.AssignmentExpression assignmentExpression)
        {
            assignmentExpression = (SiliconStudio.Shaders.Ast.AssignmentExpression)base.Visit(assignmentExpression);
            return new SiliconStudio.Shaders.Ast.AssignmentExpression
            {
                Span = assignmentExpression.Span,
                TypeInference = assignmentExpression.TypeInference,
                Operator = assignmentExpression.Operator,
                Target = assignmentExpression.Target,
                Value = assignmentExpression.Value,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.BinaryExpression binaryExpression)
        {
            binaryExpression = (SiliconStudio.Shaders.Ast.BinaryExpression)base.Visit(binaryExpression);
            return new SiliconStudio.Shaders.Ast.BinaryExpression
            {
                Span = binaryExpression.Span,
                TypeInference = binaryExpression.TypeInference,
                Left = binaryExpression.Left,
                Operator = binaryExpression.Operator,
                Right = binaryExpression.Right,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.BlockStatement blockStatement)
        {
            blockStatement = (SiliconStudio.Shaders.Ast.BlockStatement)base.Visit(blockStatement);
            return new SiliconStudio.Shaders.Ast.BlockStatement
            {
                Span = blockStatement.Span,
                Attributes = blockStatement.Attributes,
                Statements = blockStatement.Statements,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.CaseStatement caseStatement)
        {
            caseStatement = (SiliconStudio.Shaders.Ast.CaseStatement)base.Visit(caseStatement);
            return new SiliconStudio.Shaders.Ast.CaseStatement
            {
                Span = caseStatement.Span,
                Attributes = caseStatement.Attributes,
                Case = caseStatement.Case,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.CompositeEnum compositeEnum)
        {
            compositeEnum = (SiliconStudio.Shaders.Ast.CompositeEnum)base.Visit(compositeEnum);
            return new SiliconStudio.Shaders.Ast.CompositeEnum
            {
                Span = compositeEnum.Span,
                IsFlag = compositeEnum.IsFlag,
                Key = compositeEnum.Key,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.ConditionalExpression conditionalExpression)
        {
            conditionalExpression = (SiliconStudio.Shaders.Ast.ConditionalExpression)base.Visit(conditionalExpression);
            return new SiliconStudio.Shaders.Ast.ConditionalExpression
            {
                Span = conditionalExpression.Span,
                TypeInference = conditionalExpression.TypeInference,
                Condition = conditionalExpression.Condition,
                Left = conditionalExpression.Left,
                Right = conditionalExpression.Right,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.EmptyStatement emptyStatement)
        {
            emptyStatement = (SiliconStudio.Shaders.Ast.EmptyStatement)base.Visit(emptyStatement);
            return new SiliconStudio.Shaders.Ast.EmptyStatement
            {
                Span = emptyStatement.Span,
                Attributes = emptyStatement.Attributes,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.EmptyExpression emptyExpression)
        {
            emptyExpression = (SiliconStudio.Shaders.Ast.EmptyExpression)base.Visit(emptyExpression);
            return new SiliconStudio.Shaders.Ast.EmptyExpression
            {
                Span = emptyExpression.Span,
                TypeInference = emptyExpression.TypeInference,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Glsl.LayoutKeyValue layoutKeyValue)
        {
            layoutKeyValue = (SiliconStudio.Shaders.Ast.Glsl.LayoutKeyValue)base.Visit(layoutKeyValue);
            return new SiliconStudio.Shaders.Ast.Glsl.LayoutKeyValue
            {
                Span = layoutKeyValue.Span,
                Name = layoutKeyValue.Name,
                Value = layoutKeyValue.Value,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Glsl.LayoutQualifier layoutQualifier)
        {
            layoutQualifier = (SiliconStudio.Shaders.Ast.Glsl.LayoutQualifier)base.Visit(layoutQualifier);
            return new SiliconStudio.Shaders.Ast.Glsl.LayoutQualifier
            {
                Span = layoutQualifier.Span,
                IsFlag = layoutQualifier.IsFlag,
                Key = layoutQualifier.Key,
                IsPost = layoutQualifier.IsPost,
                Layouts = layoutQualifier.Layouts,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Glsl.InterfaceType interfaceType)
        {
            interfaceType = (SiliconStudio.Shaders.Ast.Glsl.InterfaceType)base.Visit(interfaceType);
            return new SiliconStudio.Shaders.Ast.Glsl.InterfaceType
            {
                Span = interfaceType.Span,
                Attributes = interfaceType.Attributes,
                TypeInference = interfaceType.TypeInference,
                Name = interfaceType.Name,
                Qualifiers = interfaceType.Qualifiers,
                IsBuiltIn = interfaceType.IsBuiltIn,
                Fields = interfaceType.Fields,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Hlsl.ClassType classType)
        {
            classType = (SiliconStudio.Shaders.Ast.Hlsl.ClassType)base.Visit(classType);
            return new SiliconStudio.Shaders.Ast.Hlsl.ClassType
            {
                Span = classType.Span,
                Attributes = classType.Attributes,
                TypeInference = classType.TypeInference,
                Name = classType.Name,
                Qualifiers = classType.Qualifiers,
                IsBuiltIn = classType.IsBuiltIn,
                AlternativeNames = classType.AlternativeNames,
                BaseClasses = classType.BaseClasses,
                GenericParameters = classType.GenericParameters,
                GenericArguments = classType.GenericArguments,
                Members = classType.Members,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Hlsl.IdentifierGeneric identifierGeneric)
        {
            identifierGeneric = (SiliconStudio.Shaders.Ast.Hlsl.IdentifierGeneric)base.Visit(identifierGeneric);
            return new SiliconStudio.Shaders.Ast.Hlsl.IdentifierGeneric
            {
                Span = identifierGeneric.Span,
                Indices = identifierGeneric.Indices,
                IsSpecialReference = identifierGeneric.IsSpecialReference,
                Text = identifierGeneric.Text,
                Identifiers = identifierGeneric.Identifiers,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Hlsl.IdentifierNs identifierNs)
        {
            identifierNs = (SiliconStudio.Shaders.Ast.Hlsl.IdentifierNs)base.Visit(identifierNs);
            return new SiliconStudio.Shaders.Ast.Hlsl.IdentifierNs
            {
                Span = identifierNs.Span,
                Indices = identifierNs.Indices,
                IsSpecialReference = identifierNs.IsSpecialReference,
                Text = identifierNs.Text,
                Identifiers = identifierNs.Identifiers,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Hlsl.IdentifierDot identifierDot)
        {
            identifierDot = (SiliconStudio.Shaders.Ast.Hlsl.IdentifierDot)base.Visit(identifierDot);
            return new SiliconStudio.Shaders.Ast.Hlsl.IdentifierDot
            {
                Span = identifierDot.Span,
                Indices = identifierDot.Indices,
                IsSpecialReference = identifierDot.IsSpecialReference,
                Text = identifierDot.Text,
                Identifiers = identifierDot.Identifiers,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Hlsl.TextureType textureType)
        {
            textureType = (SiliconStudio.Shaders.Ast.Hlsl.TextureType)base.Visit(textureType);
            return new SiliconStudio.Shaders.Ast.Hlsl.TextureType
            {
                Span = textureType.Span,
                Attributes = textureType.Attributes,
                TypeInference = textureType.TypeInference,
                Name = textureType.Name,
                Qualifiers = textureType.Qualifiers,
                IsBuiltIn = textureType.IsBuiltIn,
                AlternativeNames = textureType.AlternativeNames,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Hlsl.Annotations annotations)
        {
            annotations = (SiliconStudio.Shaders.Ast.Hlsl.Annotations)base.Visit(annotations);
            return new SiliconStudio.Shaders.Ast.Hlsl.Annotations
            {
                Span = annotations.Span,
                Variables = annotations.Variables,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Hlsl.AsmExpression asmExpression)
        {
            asmExpression = (SiliconStudio.Shaders.Ast.Hlsl.AsmExpression)base.Visit(asmExpression);
            return new SiliconStudio.Shaders.Ast.Hlsl.AsmExpression
            {
                Span = asmExpression.Span,
                TypeInference = asmExpression.TypeInference,
                Text = asmExpression.Text,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Hlsl.AttributeDeclaration attributeDeclaration)
        {
            attributeDeclaration = (SiliconStudio.Shaders.Ast.Hlsl.AttributeDeclaration)base.Visit(attributeDeclaration);
            return new SiliconStudio.Shaders.Ast.Hlsl.AttributeDeclaration
            {
                Span = attributeDeclaration.Span,
                Name = attributeDeclaration.Name,
                Parameters = attributeDeclaration.Parameters,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Hlsl.CastExpression castExpression)
        {
            castExpression = (SiliconStudio.Shaders.Ast.Hlsl.CastExpression)base.Visit(castExpression);
            return new SiliconStudio.Shaders.Ast.Hlsl.CastExpression
            {
                Span = castExpression.Span,
                TypeInference = castExpression.TypeInference,
                From = castExpression.From,
                Target = castExpression.Target,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Hlsl.CompileExpression compileExpression)
        {
            compileExpression = (SiliconStudio.Shaders.Ast.Hlsl.CompileExpression)base.Visit(compileExpression);
            return new SiliconStudio.Shaders.Ast.Hlsl.CompileExpression
            {
                Span = compileExpression.Span,
                TypeInference = compileExpression.TypeInference,
                Function = compileExpression.Function,
                Profile = compileExpression.Profile,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Hlsl.ConstantBuffer constantBuffer)
        {
            constantBuffer = (SiliconStudio.Shaders.Ast.Hlsl.ConstantBuffer)base.Visit(constantBuffer);
            return new SiliconStudio.Shaders.Ast.Hlsl.ConstantBuffer
            {
                Span = constantBuffer.Span,
                Attributes = constantBuffer.Attributes,
                Type = constantBuffer.Type,
                Members = constantBuffer.Members,
                Name = constantBuffer.Name,
                Register = constantBuffer.Register,
                Qualifiers = constantBuffer.Qualifiers,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Hlsl.ConstantBufferType constantBufferType)
        {
            constantBufferType = (SiliconStudio.Shaders.Ast.Hlsl.ConstantBufferType)base.Visit(constantBufferType);
            return new SiliconStudio.Shaders.Ast.Hlsl.ConstantBufferType
            {
                Span = constantBufferType.Span,
                IsFlag = constantBufferType.IsFlag,
                Key = constantBufferType.Key,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Hlsl.InterfaceType interfaceType)
        {
            interfaceType = (SiliconStudio.Shaders.Ast.Hlsl.InterfaceType)base.Visit(interfaceType);
            return new SiliconStudio.Shaders.Ast.Hlsl.InterfaceType
            {
                Span = interfaceType.Span,
                Attributes = interfaceType.Attributes,
                TypeInference = interfaceType.TypeInference,
                Name = interfaceType.Name,
                Qualifiers = interfaceType.Qualifiers,
                IsBuiltIn = interfaceType.IsBuiltIn,
                AlternativeNames = interfaceType.AlternativeNames,
                GenericParameters = interfaceType.GenericParameters,
                GenericArguments = interfaceType.GenericArguments,
                Methods = interfaceType.Methods,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Hlsl.PackOffset packOffset)
        {
            packOffset = (SiliconStudio.Shaders.Ast.Hlsl.PackOffset)base.Visit(packOffset);
            return new SiliconStudio.Shaders.Ast.Hlsl.PackOffset
            {
                Span = packOffset.Span,
                IsFlag = packOffset.IsFlag,
                Key = packOffset.Key,
                IsPost = packOffset.IsPost,
                Value = packOffset.Value,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Hlsl.Pass pass)
        {
            pass = (SiliconStudio.Shaders.Ast.Hlsl.Pass)base.Visit(pass);
            return new SiliconStudio.Shaders.Ast.Hlsl.Pass
            {
                Span = pass.Span,
                Attributes = pass.Attributes,
                Items = pass.Items,
                Name = pass.Name,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Hlsl.RegisterLocation registerLocation)
        {
            registerLocation = (SiliconStudio.Shaders.Ast.Hlsl.RegisterLocation)base.Visit(registerLocation);
            return new SiliconStudio.Shaders.Ast.Hlsl.RegisterLocation
            {
                Span = registerLocation.Span,
                IsFlag = registerLocation.IsFlag,
                Key = registerLocation.Key,
                IsPost = registerLocation.IsPost,
                Profile = registerLocation.Profile,
                Register = registerLocation.Register,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Hlsl.Semantic semantic)
        {
            semantic = (SiliconStudio.Shaders.Ast.Hlsl.Semantic)base.Visit(semantic);
            return new SiliconStudio.Shaders.Ast.Hlsl.Semantic
            {
                Span = semantic.Span,
                IsFlag = semantic.IsFlag,
                Key = semantic.Key,
                IsPost = semantic.IsPost,
                Name = semantic.Name,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Hlsl.StateExpression stateExpression)
        {
            stateExpression = (SiliconStudio.Shaders.Ast.Hlsl.StateExpression)base.Visit(stateExpression);
            return new SiliconStudio.Shaders.Ast.Hlsl.StateExpression
            {
                Span = stateExpression.Span,
                TypeInference = stateExpression.TypeInference,
                Initializer = stateExpression.Initializer,
                StateType = stateExpression.StateType,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Hlsl.StateInitializer stateInitializer)
        {
            stateInitializer = (SiliconStudio.Shaders.Ast.Hlsl.StateInitializer)base.Visit(stateInitializer);
            return new SiliconStudio.Shaders.Ast.Hlsl.StateInitializer
            {
                Span = stateInitializer.Span,
                TypeInference = stateInitializer.TypeInference,
                Items = stateInitializer.Items,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Hlsl.Technique technique)
        {
            technique = (SiliconStudio.Shaders.Ast.Hlsl.Technique)base.Visit(technique);
            return new SiliconStudio.Shaders.Ast.Hlsl.Technique
            {
                Span = technique.Span,
                Type = technique.Type,
                Attributes = technique.Attributes,
                Name = technique.Name,
                Passes = technique.Passes,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Hlsl.Typedef typedef)
        {
            typedef = (SiliconStudio.Shaders.Ast.Hlsl.Typedef)base.Visit(typedef);
            return new SiliconStudio.Shaders.Ast.Hlsl.Typedef
            {
                Span = typedef.Span,
                Attributes = typedef.Attributes,
                TypeInference = typedef.TypeInference,
                Name = typedef.Name,
                Qualifiers = typedef.Qualifiers,
                IsBuiltIn = typedef.IsBuiltIn,
                SubDeclarators = typedef.SubDeclarators,
                Type = typedef.Type,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.ExpressionList expressionList)
        {
            expressionList = (SiliconStudio.Shaders.Ast.ExpressionList)base.Visit(expressionList);
            return new SiliconStudio.Shaders.Ast.ExpressionList
            {
                Span = expressionList.Span,
                TypeInference = expressionList.TypeInference,
                Expressions = expressionList.Expressions,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.GenericDeclaration genericDeclaration)
        {
            genericDeclaration = (SiliconStudio.Shaders.Ast.GenericDeclaration)base.Visit(genericDeclaration);
            return new SiliconStudio.Shaders.Ast.GenericDeclaration
            {
                Span = genericDeclaration.Span,
                Name = genericDeclaration.Name,
                Holder = genericDeclaration.Holder,
                Index = genericDeclaration.Index,
                IsUsingBase = genericDeclaration.IsUsingBase,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.GenericParameterType genericParameterType)
        {
            genericParameterType = (SiliconStudio.Shaders.Ast.GenericParameterType)base.Visit(genericParameterType);
            return new SiliconStudio.Shaders.Ast.GenericParameterType
            {
                Span = genericParameterType.Span,
                Attributes = genericParameterType.Attributes,
                TypeInference = genericParameterType.TypeInference,
                Name = genericParameterType.Name,
                Qualifiers = genericParameterType.Qualifiers,
                IsBuiltIn = genericParameterType.IsBuiltIn,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.DeclarationStatement declarationStatement)
        {
            declarationStatement = (SiliconStudio.Shaders.Ast.DeclarationStatement)base.Visit(declarationStatement);
            return new SiliconStudio.Shaders.Ast.DeclarationStatement
            {
                Span = declarationStatement.Span,
                Attributes = declarationStatement.Attributes,
                Content = declarationStatement.Content,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.ExpressionStatement expressionStatement)
        {
            expressionStatement = (SiliconStudio.Shaders.Ast.ExpressionStatement)base.Visit(expressionStatement);
            return new SiliconStudio.Shaders.Ast.ExpressionStatement
            {
                Span = expressionStatement.Span,
                Attributes = expressionStatement.Attributes,
                Expression = expressionStatement.Expression,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.ForStatement forStatement)
        {
            forStatement = (SiliconStudio.Shaders.Ast.ForStatement)base.Visit(forStatement);
            return new SiliconStudio.Shaders.Ast.ForStatement
            {
                Span = forStatement.Span,
                Attributes = forStatement.Attributes,
                Start = forStatement.Start,
                Condition = forStatement.Condition,
                Next = forStatement.Next,
                Body = forStatement.Body,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.GenericType genericType)
        {
            genericType = (SiliconStudio.Shaders.Ast.GenericType)base.Visit(genericType);
            return new SiliconStudio.Shaders.Ast.GenericType
            {
                Span = genericType.Span,
                Attributes = genericType.Attributes,
                TypeInference = genericType.TypeInference,
                Name = genericType.Name,
                Qualifiers = genericType.Qualifiers,
                IsBuiltIn = genericType.IsBuiltIn,
                ParameterTypes = genericType.ParameterTypes,
                Parameters = genericType.Parameters,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Identifier identifier)
        {
            identifier = (SiliconStudio.Shaders.Ast.Identifier)base.Visit(identifier);
            return new SiliconStudio.Shaders.Ast.Identifier
            {
                Span = identifier.Span,
                Indices = identifier.Indices,
                IsSpecialReference = identifier.IsSpecialReference,
                Text = identifier.Text,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.IfStatement ifStatement)
        {
            ifStatement = (SiliconStudio.Shaders.Ast.IfStatement)base.Visit(ifStatement);
            return new SiliconStudio.Shaders.Ast.IfStatement
            {
                Span = ifStatement.Span,
                Attributes = ifStatement.Attributes,
                Condition = ifStatement.Condition,
                Else = ifStatement.Else,
                Then = ifStatement.Then,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.IndexerExpression indexerExpression)
        {
            indexerExpression = (SiliconStudio.Shaders.Ast.IndexerExpression)base.Visit(indexerExpression);
            return new SiliconStudio.Shaders.Ast.IndexerExpression
            {
                Span = indexerExpression.Span,
                TypeInference = indexerExpression.TypeInference,
                Index = indexerExpression.Index,
                Target = indexerExpression.Target,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.KeywordExpression keywordExpression)
        {
            keywordExpression = (SiliconStudio.Shaders.Ast.KeywordExpression)base.Visit(keywordExpression);
            return new SiliconStudio.Shaders.Ast.KeywordExpression
            {
                Span = keywordExpression.Span,
                TypeInference = keywordExpression.TypeInference,
                Name = keywordExpression.Name,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Literal literal)
        {
            literal = (SiliconStudio.Shaders.Ast.Literal)base.Visit(literal);
            return new SiliconStudio.Shaders.Ast.Literal
            {
                Span = literal.Span,
                Text = literal.Text,
                Value = literal.Value,
                SubLiterals = literal.SubLiterals,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.LiteralExpression literalExpression)
        {
            literalExpression = (SiliconStudio.Shaders.Ast.LiteralExpression)base.Visit(literalExpression);
            return new SiliconStudio.Shaders.Ast.LiteralExpression
            {
                Span = literalExpression.Span,
                TypeInference = literalExpression.TypeInference,
                Literal = literalExpression.Literal,
                Text = literalExpression.Text,
                Value = literalExpression.Value,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.MatrixType matrixType)
        {
            matrixType = (SiliconStudio.Shaders.Ast.MatrixType)base.Visit(matrixType);
            return new SiliconStudio.Shaders.Ast.MatrixType
            {
                Span = matrixType.Span,
                Attributes = matrixType.Attributes,
                TypeInference = matrixType.TypeInference,
                Name = matrixType.Name,
                Qualifiers = matrixType.Qualifiers,
                IsBuiltIn = matrixType.IsBuiltIn,
                ParameterTypes = matrixType.ParameterTypes,
                Parameters = matrixType.Parameters,
                RowCount = matrixType.RowCount,
                ColumnCount = matrixType.ColumnCount,
                Type = matrixType.Type,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.MemberReferenceExpression memberReferenceExpression)
        {
            memberReferenceExpression = (SiliconStudio.Shaders.Ast.MemberReferenceExpression)base.Visit(memberReferenceExpression);
            return new SiliconStudio.Shaders.Ast.MemberReferenceExpression
            {
                Span = memberReferenceExpression.Span,
                TypeInference = memberReferenceExpression.TypeInference,
                Member = memberReferenceExpression.Member,
                Target = memberReferenceExpression.Target,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.MethodDeclaration methodDeclaration)
        {
            methodDeclaration = (SiliconStudio.Shaders.Ast.MethodDeclaration)base.Visit(methodDeclaration);
            return new SiliconStudio.Shaders.Ast.MethodDeclaration
            {
                Span = methodDeclaration.Span,
                Attributes = methodDeclaration.Attributes,
                Name = methodDeclaration.Name,
                ParameterConstraints = methodDeclaration.ParameterConstraints,
                Parameters = methodDeclaration.Parameters,
                Qualifiers = methodDeclaration.Qualifiers,
                ReturnType = methodDeclaration.ReturnType,
                IsBuiltin = methodDeclaration.IsBuiltin,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.MethodDefinition methodDefinition)
        {
            methodDefinition = (SiliconStudio.Shaders.Ast.MethodDefinition)base.Visit(methodDefinition);
            return new SiliconStudio.Shaders.Ast.MethodDefinition
            {
                Span = methodDefinition.Span,
                Attributes = methodDefinition.Attributes,
                Name = methodDefinition.Name,
                ParameterConstraints = methodDefinition.ParameterConstraints,
                Parameters = methodDefinition.Parameters,
                Qualifiers = methodDefinition.Qualifiers,
                ReturnType = methodDefinition.ReturnType,
                IsBuiltin = methodDefinition.IsBuiltin,
                Body = methodDefinition.Body,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.MethodInvocationExpression methodInvocationExpression)
        {
            methodInvocationExpression = (SiliconStudio.Shaders.Ast.MethodInvocationExpression)base.Visit(methodInvocationExpression);
            return new SiliconStudio.Shaders.Ast.MethodInvocationExpression
            {
                Span = methodInvocationExpression.Span,
                TypeInference = methodInvocationExpression.TypeInference,
                Target = methodInvocationExpression.Target,
                Arguments = methodInvocationExpression.Arguments,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.ObjectType objectType)
        {
            objectType = (SiliconStudio.Shaders.Ast.ObjectType)base.Visit(objectType);
            return new SiliconStudio.Shaders.Ast.ObjectType
            {
                Span = objectType.Span,
                Attributes = objectType.Attributes,
                TypeInference = objectType.TypeInference,
                Name = objectType.Name,
                Qualifiers = objectType.Qualifiers,
                IsBuiltIn = objectType.IsBuiltIn,
                AlternativeNames = objectType.AlternativeNames,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Parameter parameter)
        {
            parameter = (SiliconStudio.Shaders.Ast.Parameter)base.Visit(parameter);
            return new SiliconStudio.Shaders.Ast.Parameter
            {
                Span = parameter.Span,
                Attributes = parameter.Attributes,
                Qualifiers = parameter.Qualifiers,
                Type = parameter.Type,
                InitialValue = parameter.InitialValue,
                Name = parameter.Name,
                SubVariables = parameter.SubVariables,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.ParenthesizedExpression parenthesizedExpression)
        {
            parenthesizedExpression = (SiliconStudio.Shaders.Ast.ParenthesizedExpression)base.Visit(parenthesizedExpression);
            return new SiliconStudio.Shaders.Ast.ParenthesizedExpression
            {
                Span = parenthesizedExpression.Span,
                TypeInference = parenthesizedExpression.TypeInference,
                Content = parenthesizedExpression.Content,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Qualifier qualifier)
        {
            qualifier = (SiliconStudio.Shaders.Ast.Qualifier)base.Visit(qualifier);
            return new SiliconStudio.Shaders.Ast.Qualifier
            {
                Span = qualifier.Span,
                IsFlag = qualifier.IsFlag,
                Key = qualifier.Key,
                IsPost = qualifier.IsPost,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.ReturnStatement returnStatement)
        {
            returnStatement = (SiliconStudio.Shaders.Ast.ReturnStatement)base.Visit(returnStatement);
            return new SiliconStudio.Shaders.Ast.ReturnStatement
            {
                Span = returnStatement.Span,
                Attributes = returnStatement.Attributes,
                Value = returnStatement.Value,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.ScalarType scalarType)
        {
            scalarType = (SiliconStudio.Shaders.Ast.ScalarType)base.Visit(scalarType);
            return new SiliconStudio.Shaders.Ast.ScalarType
            {
                Span = scalarType.Span,
                Attributes = scalarType.Attributes,
                TypeInference = scalarType.TypeInference,
                Name = scalarType.Name,
                Qualifiers = scalarType.Qualifiers,
                IsBuiltIn = scalarType.IsBuiltIn,
                Type = scalarType.Type,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Shader shader)
        {
            shader = (SiliconStudio.Shaders.Ast.Shader)base.Visit(shader);
            return new SiliconStudio.Shaders.Ast.Shader
            {
                Span = shader.Span,
                Declarations = shader.Declarations,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.StatementList statementList)
        {
            statementList = (SiliconStudio.Shaders.Ast.StatementList)base.Visit(statementList);
            return new SiliconStudio.Shaders.Ast.StatementList
            {
                Span = statementList.Span,
                Attributes = statementList.Attributes,
                Statements = statementList.Statements,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.StructType structType)
        {
            structType = (SiliconStudio.Shaders.Ast.StructType)base.Visit(structType);
            return new SiliconStudio.Shaders.Ast.StructType
            {
                Span = structType.Span,
                Attributes = structType.Attributes,
                TypeInference = structType.TypeInference,
                Name = structType.Name,
                Qualifiers = structType.Qualifiers,
                IsBuiltIn = structType.IsBuiltIn,
                Fields = structType.Fields,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.SwitchCaseGroup switchCaseGroup)
        {
            switchCaseGroup = (SiliconStudio.Shaders.Ast.SwitchCaseGroup)base.Visit(switchCaseGroup);
            return new SiliconStudio.Shaders.Ast.SwitchCaseGroup
            {
                Span = switchCaseGroup.Span,
                Cases = switchCaseGroup.Cases,
                Statements = switchCaseGroup.Statements,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.SwitchStatement switchStatement)
        {
            switchStatement = (SiliconStudio.Shaders.Ast.SwitchStatement)base.Visit(switchStatement);
            return new SiliconStudio.Shaders.Ast.SwitchStatement
            {
                Span = switchStatement.Span,
                Attributes = switchStatement.Attributes,
                Condition = switchStatement.Condition,
                Groups = switchStatement.Groups,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.TypeName typeName)
        {
            typeName = (SiliconStudio.Shaders.Ast.TypeName)base.Visit(typeName);
            return new SiliconStudio.Shaders.Ast.TypeName
            {
                Span = typeName.Span,
                Attributes = typeName.Attributes,
                TypeInference = typeName.TypeInference,
                Name = typeName.Name,
                Qualifiers = typeName.Qualifiers,
                IsBuiltIn = typeName.IsBuiltIn,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.TypeReferenceExpression typeReferenceExpression)
        {
            typeReferenceExpression = (SiliconStudio.Shaders.Ast.TypeReferenceExpression)base.Visit(typeReferenceExpression);
            return new SiliconStudio.Shaders.Ast.TypeReferenceExpression
            {
                Span = typeReferenceExpression.Span,
                TypeInference = typeReferenceExpression.TypeInference,
                Type = typeReferenceExpression.Type,
                Declaration = typeReferenceExpression.Declaration,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.UnaryExpression unaryExpression)
        {
            unaryExpression = (SiliconStudio.Shaders.Ast.UnaryExpression)base.Visit(unaryExpression);
            return new SiliconStudio.Shaders.Ast.UnaryExpression
            {
                Span = unaryExpression.Span,
                TypeInference = unaryExpression.TypeInference,
                Operator = unaryExpression.Operator,
                Expression = unaryExpression.Expression,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.Variable variable)
        {
            variable = (SiliconStudio.Shaders.Ast.Variable)base.Visit(variable);
            return new SiliconStudio.Shaders.Ast.Variable
            {
                Span = variable.Span,
                Attributes = variable.Attributes,
                Qualifiers = variable.Qualifiers,
                Type = variable.Type,
                InitialValue = variable.InitialValue,
                Name = variable.Name,
                SubVariables = variable.SubVariables,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.VariableReferenceExpression variableReferenceExpression)
        {
            variableReferenceExpression = (SiliconStudio.Shaders.Ast.VariableReferenceExpression)base.Visit(variableReferenceExpression);
            return new SiliconStudio.Shaders.Ast.VariableReferenceExpression
            {
                Span = variableReferenceExpression.Span,
                TypeInference = variableReferenceExpression.TypeInference,
                Name = variableReferenceExpression.Name,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.VectorType vectorType)
        {
            vectorType = (SiliconStudio.Shaders.Ast.VectorType)base.Visit(vectorType);
            return new SiliconStudio.Shaders.Ast.VectorType
            {
                Span = vectorType.Span,
                Attributes = vectorType.Attributes,
                TypeInference = vectorType.TypeInference,
                Name = vectorType.Name,
                Qualifiers = vectorType.Qualifiers,
                IsBuiltIn = vectorType.IsBuiltIn,
                ParameterTypes = vectorType.ParameterTypes,
                Parameters = vectorType.Parameters,
                Dimension = vectorType.Dimension,
                Type = vectorType.Type,
            };
        }
        public override Node Visit(SiliconStudio.Shaders.Ast.WhileStatement whileStatement)
        {
            whileStatement = (SiliconStudio.Shaders.Ast.WhileStatement)base.Visit(whileStatement);
            return new SiliconStudio.Shaders.Ast.WhileStatement
            {
                Span = whileStatement.Span,
                Attributes = whileStatement.Attributes,
                Condition = whileStatement.Condition,
                IsDoWhile = whileStatement.IsDoWhile,
                Statement = whileStatement.Statement,
            };
        }
    }

    public partial class ShaderVisitor
    {
        public virtual void Visit(SiliconStudio.Shaders.Ast.Xenko.ClassIdentifierGeneric classIdentifierGeneric)
        {
            DefaultVisit(classIdentifierGeneric);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Xenko.EnumType enumType)
        {
            DefaultVisit(enumType);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Xenko.ForEachStatement forEachStatement)
        {
            DefaultVisit(forEachStatement);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Xenko.ImportBlockStatement importBlockStatement)
        {
            DefaultVisit(importBlockStatement);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Xenko.LinkType linkType)
        {
            DefaultVisit(linkType);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Xenko.LiteralIdentifier literalIdentifier)
        {
            DefaultVisit(literalIdentifier);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Xenko.MemberName memberName)
        {
            DefaultVisit(memberName);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Xenko.MixinStatement mixinStatement)
        {
            DefaultVisit(mixinStatement);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Xenko.NamespaceBlock namespaceBlock)
        {
            DefaultVisit(namespaceBlock);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Xenko.ParametersBlock parametersBlock)
        {
            DefaultVisit(parametersBlock);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Xenko.SemanticType semanticType)
        {
            DefaultVisit(semanticType);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Xenko.ShaderBlock shaderBlock)
        {
            DefaultVisit(shaderBlock);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Xenko.ShaderClassType shaderClassType)
        {
            DefaultVisit(shaderClassType);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Xenko.ShaderRootClassType shaderRootClassType)
        {
            DefaultVisit(shaderRootClassType);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Xenko.ShaderTypeName shaderTypeName)
        {
            DefaultVisit(shaderTypeName);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Xenko.TypeIdentifier typeIdentifier)
        {
            DefaultVisit(typeIdentifier);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Xenko.UsingParametersStatement usingParametersStatement)
        {
            DefaultVisit(usingParametersStatement);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Xenko.UsingStatement usingStatement)
        {
            DefaultVisit(usingStatement);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Xenko.VarType varType)
        {
            DefaultVisit(varType);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Xenko.XenkoConstantBufferType xenkoConstantBufferType)
        {
            DefaultVisit(xenkoConstantBufferType);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.ArrayInitializerExpression arrayInitializerExpression)
        {
            DefaultVisit(arrayInitializerExpression);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.ArrayType arrayType)
        {
            DefaultVisit(arrayType);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.AssignmentExpression assignmentExpression)
        {
            DefaultVisit(assignmentExpression);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.BinaryExpression binaryExpression)
        {
            DefaultVisit(binaryExpression);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.BlockStatement blockStatement)
        {
            DefaultVisit(blockStatement);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.CaseStatement caseStatement)
        {
            DefaultVisit(caseStatement);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.CompositeEnum compositeEnum)
        {
            DefaultVisit(compositeEnum);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.ConditionalExpression conditionalExpression)
        {
            DefaultVisit(conditionalExpression);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.EmptyStatement emptyStatement)
        {
            DefaultVisit(emptyStatement);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.EmptyExpression emptyExpression)
        {
            DefaultVisit(emptyExpression);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Glsl.LayoutKeyValue layoutKeyValue)
        {
            DefaultVisit(layoutKeyValue);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Glsl.LayoutQualifier layoutQualifier)
        {
            DefaultVisit(layoutQualifier);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Glsl.InterfaceType interfaceType)
        {
            DefaultVisit(interfaceType);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Hlsl.ClassType classType)
        {
            DefaultVisit(classType);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Hlsl.IdentifierGeneric identifierGeneric)
        {
            DefaultVisit(identifierGeneric);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Hlsl.IdentifierNs identifierNs)
        {
            DefaultVisit(identifierNs);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Hlsl.IdentifierDot identifierDot)
        {
            DefaultVisit(identifierDot);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Hlsl.TextureType textureType)
        {
            DefaultVisit(textureType);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Hlsl.Annotations annotations)
        {
            DefaultVisit(annotations);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Hlsl.AsmExpression asmExpression)
        {
            DefaultVisit(asmExpression);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Hlsl.AttributeDeclaration attributeDeclaration)
        {
            DefaultVisit(attributeDeclaration);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Hlsl.CastExpression castExpression)
        {
            DefaultVisit(castExpression);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Hlsl.CompileExpression compileExpression)
        {
            DefaultVisit(compileExpression);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Hlsl.ConstantBuffer constantBuffer)
        {
            DefaultVisit(constantBuffer);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Hlsl.ConstantBufferType constantBufferType)
        {
            DefaultVisit(constantBufferType);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Hlsl.InterfaceType interfaceType)
        {
            DefaultVisit(interfaceType);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Hlsl.PackOffset packOffset)
        {
            DefaultVisit(packOffset);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Hlsl.Pass pass)
        {
            DefaultVisit(pass);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Hlsl.RegisterLocation registerLocation)
        {
            DefaultVisit(registerLocation);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Hlsl.Semantic semantic)
        {
            DefaultVisit(semantic);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Hlsl.StateExpression stateExpression)
        {
            DefaultVisit(stateExpression);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Hlsl.StateInitializer stateInitializer)
        {
            DefaultVisit(stateInitializer);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Hlsl.Technique technique)
        {
            DefaultVisit(technique);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Hlsl.Typedef typedef)
        {
            DefaultVisit(typedef);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.ExpressionList expressionList)
        {
            DefaultVisit(expressionList);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.GenericDeclaration genericDeclaration)
        {
            DefaultVisit(genericDeclaration);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.GenericParameterType genericParameterType)
        {
            DefaultVisit(genericParameterType);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.DeclarationStatement declarationStatement)
        {
            DefaultVisit(declarationStatement);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.ExpressionStatement expressionStatement)
        {
            DefaultVisit(expressionStatement);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.ForStatement forStatement)
        {
            DefaultVisit(forStatement);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.GenericType genericType)
        {
            DefaultVisit(genericType);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Identifier identifier)
        {
            DefaultVisit(identifier);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.IfStatement ifStatement)
        {
            DefaultVisit(ifStatement);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.IndexerExpression indexerExpression)
        {
            DefaultVisit(indexerExpression);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.KeywordExpression keywordExpression)
        {
            DefaultVisit(keywordExpression);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Literal literal)
        {
            DefaultVisit(literal);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.LiteralExpression literalExpression)
        {
            DefaultVisit(literalExpression);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.MatrixType matrixType)
        {
            DefaultVisit(matrixType);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.MemberReferenceExpression memberReferenceExpression)
        {
            DefaultVisit(memberReferenceExpression);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.MethodDeclaration methodDeclaration)
        {
            DefaultVisit(methodDeclaration);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.MethodDefinition methodDefinition)
        {
            DefaultVisit(methodDefinition);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.MethodInvocationExpression methodInvocationExpression)
        {
            DefaultVisit(methodInvocationExpression);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.ObjectType objectType)
        {
            DefaultVisit(objectType);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Parameter parameter)
        {
            DefaultVisit(parameter);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.ParenthesizedExpression parenthesizedExpression)
        {
            DefaultVisit(parenthesizedExpression);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Qualifier qualifier)
        {
            DefaultVisit(qualifier);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.ReturnStatement returnStatement)
        {
            DefaultVisit(returnStatement);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.ScalarType scalarType)
        {
            DefaultVisit(scalarType);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Shader shader)
        {
            DefaultVisit(shader);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.StatementList statementList)
        {
            DefaultVisit(statementList);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.StructType structType)
        {
            DefaultVisit(structType);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.SwitchCaseGroup switchCaseGroup)
        {
            DefaultVisit(switchCaseGroup);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.SwitchStatement switchStatement)
        {
            DefaultVisit(switchStatement);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.TypeName typeName)
        {
            DefaultVisit(typeName);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.TypeReferenceExpression typeReferenceExpression)
        {
            DefaultVisit(typeReferenceExpression);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.UnaryExpression unaryExpression)
        {
            DefaultVisit(unaryExpression);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.Variable variable)
        {
            DefaultVisit(variable);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.VariableReferenceExpression variableReferenceExpression)
        {
            DefaultVisit(variableReferenceExpression);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.VectorType vectorType)
        {
            DefaultVisit(vectorType);
        }
        public virtual void Visit(SiliconStudio.Shaders.Ast.WhileStatement whileStatement)
        {
            DefaultVisit(whileStatement);
        }
    }

    public partial class ShaderWalker
    {
        public override void Visit(SiliconStudio.Shaders.Ast.Xenko.ClassIdentifierGeneric classIdentifierGeneric)
        {
            VisitList(classIdentifierGeneric.Indices);
            VisitList(classIdentifierGeneric.Generics);
            base.Visit(classIdentifierGeneric);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Xenko.EnumType enumType)
        {
            VisitList(enumType.Attributes);
            VisitDynamic(enumType.Name);
            VisitDynamic(enumType.Qualifiers);
            VisitList(enumType.Values);
            base.Visit(enumType);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Xenko.ForEachStatement forEachStatement)
        {
            VisitList(forEachStatement.Attributes);
            VisitDynamic(forEachStatement.Collection);
            VisitDynamic(forEachStatement.Variable);
            VisitDynamic(forEachStatement.Body);
            base.Visit(forEachStatement);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Xenko.ImportBlockStatement importBlockStatement)
        {
            VisitList(importBlockStatement.Attributes);
            VisitDynamic(importBlockStatement.Statements);
            base.Visit(importBlockStatement);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Xenko.LinkType linkType)
        {
            VisitList(linkType.Attributes);
            VisitDynamic(linkType.Name);
            VisitDynamic(linkType.Qualifiers);
            base.Visit(linkType);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Xenko.LiteralIdentifier literalIdentifier)
        {
            VisitList(literalIdentifier.Indices);
            VisitDynamic(literalIdentifier.Value);
            base.Visit(literalIdentifier);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Xenko.MemberName memberName)
        {
            VisitList(memberName.Attributes);
            VisitDynamic(memberName.Name);
            VisitDynamic(memberName.Qualifiers);
            base.Visit(memberName);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Xenko.MixinStatement mixinStatement)
        {
            VisitList(mixinStatement.Attributes);
            VisitDynamic(mixinStatement.Value);
            base.Visit(mixinStatement);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Xenko.NamespaceBlock namespaceBlock)
        {
            VisitList(namespaceBlock.Attributes);
            VisitDynamic(namespaceBlock.Name);
            VisitDynamic(namespaceBlock.Qualifiers);
            VisitList(namespaceBlock.Body);
            base.Visit(namespaceBlock);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Xenko.ParametersBlock parametersBlock)
        {
            VisitDynamic(parametersBlock.Name);
            VisitDynamic(parametersBlock.Body);
            base.Visit(parametersBlock);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Xenko.SemanticType semanticType)
        {
            VisitList(semanticType.Attributes);
            VisitDynamic(semanticType.Name);
            VisitDynamic(semanticType.Qualifiers);
            base.Visit(semanticType);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Xenko.ShaderBlock shaderBlock)
        {
            VisitList(shaderBlock.Attributes);
            VisitDynamic(shaderBlock.Name);
            VisitDynamic(shaderBlock.Qualifiers);
            VisitDynamic(shaderBlock.Body);
            base.Visit(shaderBlock);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Xenko.ShaderClassType shaderClassType)
        {
            VisitList(shaderClassType.Attributes);
            VisitDynamic(shaderClassType.Name);
            VisitDynamic(shaderClassType.Qualifiers);
            VisitList(shaderClassType.BaseClasses);
            VisitList(shaderClassType.GenericParameters);
            VisitList(shaderClassType.GenericArguments);
            VisitList(shaderClassType.Members);
            VisitList(shaderClassType.ShaderGenerics);
            base.Visit(shaderClassType);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Xenko.ShaderRootClassType shaderRootClassType)
        {
            VisitList(shaderRootClassType.Attributes);
            VisitDynamic(shaderRootClassType.Name);
            VisitDynamic(shaderRootClassType.Qualifiers);
            VisitList(shaderRootClassType.BaseClasses);
            VisitList(shaderRootClassType.GenericParameters);
            VisitList(shaderRootClassType.GenericArguments);
            VisitList(shaderRootClassType.Members);
            VisitList(shaderRootClassType.ShaderGenerics);
            base.Visit(shaderRootClassType);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Xenko.ShaderTypeName shaderTypeName)
        {
            VisitList(shaderTypeName.Attributes);
            VisitDynamic(shaderTypeName.Name);
            VisitDynamic(shaderTypeName.Qualifiers);
            base.Visit(shaderTypeName);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Xenko.TypeIdentifier typeIdentifier)
        {
            VisitList(typeIdentifier.Indices);
            VisitDynamic(typeIdentifier.Type);
            base.Visit(typeIdentifier);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Xenko.UsingParametersStatement usingParametersStatement)
        {
            VisitList(usingParametersStatement.Attributes);
            VisitDynamic(usingParametersStatement.Name);
            VisitDynamic(usingParametersStatement.Body);
            base.Visit(usingParametersStatement);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Xenko.UsingStatement usingStatement)
        {
            VisitList(usingStatement.Attributes);
            VisitDynamic(usingStatement.Name);
            base.Visit(usingStatement);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Xenko.VarType varType)
        {
            VisitList(varType.Attributes);
            VisitDynamic(varType.Name);
            VisitDynamic(varType.Qualifiers);
            base.Visit(varType);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Xenko.XenkoConstantBufferType xenkoConstantBufferType)
        {
            base.Visit(xenkoConstantBufferType);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.ArrayInitializerExpression arrayInitializerExpression)
        {
            VisitList(arrayInitializerExpression.Items);
            base.Visit(arrayInitializerExpression);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.ArrayType arrayType)
        {
            VisitList(arrayType.Attributes);
            VisitDynamic(arrayType.Name);
            VisitDynamic(arrayType.Qualifiers);
            VisitList(arrayType.Dimensions);
            VisitDynamic(arrayType.Type);
            base.Visit(arrayType);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.AssignmentExpression assignmentExpression)
        {
            VisitDynamic(assignmentExpression.Target);
            VisitDynamic(assignmentExpression.Value);
            base.Visit(assignmentExpression);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.BinaryExpression binaryExpression)
        {
            VisitDynamic(binaryExpression.Left);
            VisitDynamic(binaryExpression.Right);
            base.Visit(binaryExpression);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.BlockStatement blockStatement)
        {
            VisitList(blockStatement.Attributes);
            VisitDynamic(blockStatement.Statements);
            base.Visit(blockStatement);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.CaseStatement caseStatement)
        {
            VisitList(caseStatement.Attributes);
            VisitDynamic(caseStatement.Case);
            base.Visit(caseStatement);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.CompositeEnum compositeEnum)
        {
            base.Visit(compositeEnum);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.ConditionalExpression conditionalExpression)
        {
            VisitDynamic(conditionalExpression.Condition);
            VisitDynamic(conditionalExpression.Left);
            VisitDynamic(conditionalExpression.Right);
            base.Visit(conditionalExpression);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.EmptyStatement emptyStatement)
        {
            VisitList(emptyStatement.Attributes);
            base.Visit(emptyStatement);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.EmptyExpression emptyExpression)
        {
            base.Visit(emptyExpression);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Glsl.LayoutKeyValue layoutKeyValue)
        {
            VisitDynamic(layoutKeyValue.Name);
            VisitDynamic(layoutKeyValue.Value);
            base.Visit(layoutKeyValue);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Glsl.LayoutQualifier layoutQualifier)
        {
            VisitList(layoutQualifier.Layouts);
            base.Visit(layoutQualifier);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Glsl.InterfaceType interfaceType)
        {
            VisitList(interfaceType.Attributes);
            VisitDynamic(interfaceType.Name);
            VisitDynamic(interfaceType.Qualifiers);
            VisitList(interfaceType.Fields);
            base.Visit(interfaceType);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Hlsl.ClassType classType)
        {
            VisitList(classType.Attributes);
            VisitDynamic(classType.Name);
            VisitDynamic(classType.Qualifiers);
            VisitList(classType.BaseClasses);
            VisitList(classType.GenericParameters);
            VisitList(classType.GenericArguments);
            VisitList(classType.Members);
            base.Visit(classType);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Hlsl.IdentifierGeneric identifierGeneric)
        {
            VisitList(identifierGeneric.Indices);
            VisitList(identifierGeneric.Identifiers);
            base.Visit(identifierGeneric);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Hlsl.IdentifierNs identifierNs)
        {
            VisitList(identifierNs.Indices);
            VisitList(identifierNs.Identifiers);
            base.Visit(identifierNs);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Hlsl.IdentifierDot identifierDot)
        {
            VisitList(identifierDot.Indices);
            VisitList(identifierDot.Identifiers);
            base.Visit(identifierDot);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Hlsl.TextureType textureType)
        {
            VisitList(textureType.Attributes);
            VisitDynamic(textureType.Name);
            VisitDynamic(textureType.Qualifiers);
            base.Visit(textureType);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Hlsl.Annotations annotations)
        {
            VisitList(annotations.Variables);
            base.Visit(annotations);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Hlsl.AsmExpression asmExpression)
        {
            base.Visit(asmExpression);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Hlsl.AttributeDeclaration attributeDeclaration)
        {
            VisitDynamic(attributeDeclaration.Name);
            VisitList(attributeDeclaration.Parameters);
            base.Visit(attributeDeclaration);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Hlsl.CastExpression castExpression)
        {
            VisitDynamic(castExpression.From);
            VisitDynamic(castExpression.Target);
            base.Visit(castExpression);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Hlsl.CompileExpression compileExpression)
        {
            VisitDynamic(compileExpression.Function);
            VisitDynamic(compileExpression.Profile);
            base.Visit(compileExpression);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Hlsl.ConstantBuffer constantBuffer)
        {
            VisitList(constantBuffer.Attributes);
            VisitDynamic(constantBuffer.Type);
            VisitList(constantBuffer.Members);
            VisitDynamic(constantBuffer.Name);
            VisitDynamic(constantBuffer.Register);
            VisitDynamic(constantBuffer.Qualifiers);
            base.Visit(constantBuffer);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Hlsl.ConstantBufferType constantBufferType)
        {
            base.Visit(constantBufferType);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Hlsl.InterfaceType interfaceType)
        {
            VisitList(interfaceType.Attributes);
            VisitDynamic(interfaceType.Name);
            VisitDynamic(interfaceType.Qualifiers);
            VisitList(interfaceType.GenericParameters);
            VisitList(interfaceType.GenericArguments);
            VisitList(interfaceType.Methods);
            base.Visit(interfaceType);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Hlsl.PackOffset packOffset)
        {
            VisitDynamic(packOffset.Value);
            base.Visit(packOffset);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Hlsl.Pass pass)
        {
            VisitList(pass.Attributes);
            VisitList(pass.Items);
            VisitDynamic(pass.Name);
            base.Visit(pass);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Hlsl.RegisterLocation registerLocation)
        {
            VisitDynamic(registerLocation.Profile);
            VisitDynamic(registerLocation.Register);
            base.Visit(registerLocation);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Hlsl.Semantic semantic)
        {
            VisitDynamic(semantic.Name);
            base.Visit(semantic);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Hlsl.StateExpression stateExpression)
        {
            VisitDynamic(stateExpression.Initializer);
            VisitDynamic(stateExpression.StateType);
            base.Visit(stateExpression);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Hlsl.StateInitializer stateInitializer)
        {
            VisitList(stateInitializer.Items);
            base.Visit(stateInitializer);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Hlsl.Technique technique)
        {
            VisitDynamic(technique.Type);
            VisitList(technique.Attributes);
            VisitDynamic(technique.Name);
            VisitList(technique.Passes);
            base.Visit(technique);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Hlsl.Typedef typedef)
        {
            VisitList(typedef.Attributes);
            VisitDynamic(typedef.Name);
            VisitDynamic(typedef.Qualifiers);
            VisitList(typedef.SubDeclarators);
            VisitDynamic(typedef.Type);
            base.Visit(typedef);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.ExpressionList expressionList)
        {
            VisitList(expressionList.Expressions);
            base.Visit(expressionList);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.GenericDeclaration genericDeclaration)
        {
            VisitDynamic(genericDeclaration.Name);
            base.Visit(genericDeclaration);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.GenericParameterType genericParameterType)
        {
            VisitList(genericParameterType.Attributes);
            VisitDynamic(genericParameterType.Name);
            VisitDynamic(genericParameterType.Qualifiers);
            base.Visit(genericParameterType);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.DeclarationStatement declarationStatement)
        {
            VisitList(declarationStatement.Attributes);
            VisitDynamic(declarationStatement.Content);
            base.Visit(declarationStatement);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.ExpressionStatement expressionStatement)
        {
            VisitList(expressionStatement.Attributes);
            VisitDynamic(expressionStatement.Expression);
            base.Visit(expressionStatement);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.ForStatement forStatement)
        {
            VisitList(forStatement.Attributes);
            VisitDynamic(forStatement.Start);
            VisitDynamic(forStatement.Condition);
            VisitDynamic(forStatement.Next);
            VisitDynamic(forStatement.Body);
            base.Visit(forStatement);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.GenericType genericType)
        {
            VisitList(genericType.Attributes);
            VisitDynamic(genericType.Name);
            VisitDynamic(genericType.Qualifiers);
            VisitList(genericType.Parameters);
            base.Visit(genericType);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Identifier identifier)
        {
            VisitList(identifier.Indices);
            base.Visit(identifier);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.IfStatement ifStatement)
        {
            VisitList(ifStatement.Attributes);
            VisitDynamic(ifStatement.Condition);
            VisitDynamic(ifStatement.Else);
            VisitDynamic(ifStatement.Then);
            base.Visit(ifStatement);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.IndexerExpression indexerExpression)
        {
            VisitDynamic(indexerExpression.Index);
            VisitDynamic(indexerExpression.Target);
            base.Visit(indexerExpression);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.KeywordExpression keywordExpression)
        {
            VisitDynamic(keywordExpression.Name);
            base.Visit(keywordExpression);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Literal literal)
        {
            VisitList(literal.SubLiterals);
            base.Visit(literal);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.LiteralExpression literalExpression)
        {
            VisitDynamic(literalExpression.Literal);
            base.Visit(literalExpression);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.MatrixType matrixType)
        {
            VisitList(matrixType.Attributes);
            VisitDynamic(matrixType.Name);
            VisitDynamic(matrixType.Qualifiers);
            VisitList(matrixType.Parameters);
            VisitDynamic(matrixType.Type);
            base.Visit(matrixType);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.MemberReferenceExpression memberReferenceExpression)
        {
            VisitDynamic(memberReferenceExpression.Member);
            VisitDynamic(memberReferenceExpression.Target);
            base.Visit(memberReferenceExpression);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.MethodDeclaration methodDeclaration)
        {
            VisitList(methodDeclaration.Attributes);
            VisitDynamic(methodDeclaration.Name);
            VisitList(methodDeclaration.Parameters);
            VisitDynamic(methodDeclaration.Qualifiers);
            VisitDynamic(methodDeclaration.ReturnType);
            base.Visit(methodDeclaration);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.MethodDefinition methodDefinition)
        {
            VisitList(methodDefinition.Attributes);
            VisitDynamic(methodDefinition.Name);
            VisitList(methodDefinition.Parameters);
            VisitDynamic(methodDefinition.Qualifiers);
            VisitDynamic(methodDefinition.ReturnType);
            VisitDynamic(methodDefinition.Body);
            base.Visit(methodDefinition);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.MethodInvocationExpression methodInvocationExpression)
        {
            VisitDynamic(methodInvocationExpression.Target);
            VisitList(methodInvocationExpression.Arguments);
            base.Visit(methodInvocationExpression);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.ObjectType objectType)
        {
            VisitList(objectType.Attributes);
            VisitDynamic(objectType.Name);
            VisitDynamic(objectType.Qualifiers);
            base.Visit(objectType);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Parameter parameter)
        {
            VisitList(parameter.Attributes);
            VisitDynamic(parameter.Qualifiers);
            VisitDynamic(parameter.Type);
            VisitDynamic(parameter.InitialValue);
            VisitDynamic(parameter.Name);
            VisitList(parameter.SubVariables);
            base.Visit(parameter);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.ParenthesizedExpression parenthesizedExpression)
        {
            VisitDynamic(parenthesizedExpression.Content);
            base.Visit(parenthesizedExpression);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Qualifier qualifier)
        {
            base.Visit(qualifier);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.ReturnStatement returnStatement)
        {
            VisitList(returnStatement.Attributes);
            VisitDynamic(returnStatement.Value);
            base.Visit(returnStatement);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.ScalarType scalarType)
        {
            VisitList(scalarType.Attributes);
            VisitDynamic(scalarType.Name);
            VisitDynamic(scalarType.Qualifiers);
            base.Visit(scalarType);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Shader shader)
        {
            VisitList(shader.Declarations);
            base.Visit(shader);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.StatementList statementList)
        {
            VisitList(statementList.Attributes);
            VisitList(statementList.Statements);
            base.Visit(statementList);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.StructType structType)
        {
            VisitList(structType.Attributes);
            VisitDynamic(structType.Name);
            VisitDynamic(structType.Qualifiers);
            VisitList(structType.Fields);
            base.Visit(structType);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.SwitchCaseGroup switchCaseGroup)
        {
            VisitList(switchCaseGroup.Cases);
            VisitDynamic(switchCaseGroup.Statements);
            base.Visit(switchCaseGroup);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.SwitchStatement switchStatement)
        {
            VisitList(switchStatement.Attributes);
            VisitDynamic(switchStatement.Condition);
            VisitList(switchStatement.Groups);
            base.Visit(switchStatement);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.TypeName typeName)
        {
            VisitList(typeName.Attributes);
            VisitDynamic(typeName.Name);
            VisitDynamic(typeName.Qualifiers);
            base.Visit(typeName);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.TypeReferenceExpression typeReferenceExpression)
        {
            VisitDynamic(typeReferenceExpression.Type);
            base.Visit(typeReferenceExpression);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.UnaryExpression unaryExpression)
        {
            VisitDynamic(unaryExpression.Expression);
            base.Visit(unaryExpression);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.Variable variable)
        {
            VisitList(variable.Attributes);
            VisitDynamic(variable.Qualifiers);
            VisitDynamic(variable.Type);
            VisitDynamic(variable.InitialValue);
            VisitDynamic(variable.Name);
            VisitList(variable.SubVariables);
            base.Visit(variable);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.VariableReferenceExpression variableReferenceExpression)
        {
            VisitDynamic(variableReferenceExpression.Name);
            base.Visit(variableReferenceExpression);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.VectorType vectorType)
        {
            VisitList(vectorType.Attributes);
            VisitDynamic(vectorType.Name);
            VisitDynamic(vectorType.Qualifiers);
            VisitList(vectorType.Parameters);
            VisitDynamic(vectorType.Type);
            base.Visit(vectorType);
        }
        public override void Visit(SiliconStudio.Shaders.Ast.WhileStatement whileStatement)
        {
            VisitList(whileStatement.Attributes);
            VisitDynamic(whileStatement.Condition);
            VisitDynamic(whileStatement.Statement);
            base.Visit(whileStatement);
        }
    }
}

namespace SiliconStudio.Shaders.Ast.Xenko
{
    public partial class ClassIdentifierGeneric
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast.Xenko
{
    public partial class EnumType
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast.Xenko
{
    public partial class ForEachStatement
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast.Xenko
{
    public partial class ImportBlockStatement
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast.Xenko
{
    public partial class LinkType
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast.Xenko
{
    public partial class LiteralIdentifier
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast.Xenko
{
    public partial class MemberName
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast.Xenko
{
    public partial class MixinStatement
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast.Xenko
{
    public partial class NamespaceBlock
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast.Xenko
{
    public partial class ParametersBlock
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast.Xenko
{
    public partial class SemanticType
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast.Xenko
{
    public partial class ShaderBlock
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast.Xenko
{
    public partial class ShaderClassType
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast.Xenko
{
    public partial class ShaderRootClassType
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast.Xenko
{
    public partial class ShaderTypeName
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast.Xenko
{
    public partial class TypeIdentifier
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast.Xenko
{
    public partial class UsingParametersStatement
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast.Xenko
{
    public partial class UsingStatement
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast.Xenko
{
    public partial class VarType
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast.Xenko
{
    public partial class XenkoConstantBufferType
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast
{
    public partial class ArrayInitializerExpression
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast
{
    public partial class ArrayType
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast
{
    public partial class AssignmentExpression
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast
{
    public partial class BinaryExpression
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast
{
    public partial class BlockStatement
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast
{
    public partial class CaseStatement
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast
{
    public partial class CompositeEnum
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast
{
    public partial class ConditionalExpression
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast
{
    public partial class EmptyStatement
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast
{
    public partial class EmptyExpression
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast.Glsl
{
    public partial class LayoutKeyValue
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast.Glsl
{
    public partial class LayoutQualifier
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast.Glsl
{
    public partial class InterfaceType
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast.Hlsl
{
    public partial class ClassType
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast.Hlsl
{
    public partial class IdentifierGeneric
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast.Hlsl
{
    public partial class IdentifierNs
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast.Hlsl
{
    public partial class IdentifierDot
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast.Hlsl
{
    public partial class TextureType
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast.Hlsl
{
    public partial class Annotations
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast.Hlsl
{
    public partial class AsmExpression
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast.Hlsl
{
    public partial class AttributeDeclaration
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast.Hlsl
{
    public partial class CastExpression
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast.Hlsl
{
    public partial class CompileExpression
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast.Hlsl
{
    public partial class ConstantBuffer
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast.Hlsl
{
    public partial class ConstantBufferType
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast.Hlsl
{
    public partial class InterfaceType
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast.Hlsl
{
    public partial class PackOffset
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast.Hlsl
{
    public partial class Pass
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast.Hlsl
{
    public partial class RegisterLocation
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast.Hlsl
{
    public partial class Semantic
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast.Hlsl
{
    public partial class StateExpression
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast.Hlsl
{
    public partial class StateInitializer
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast.Hlsl
{
    public partial class Technique
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast.Hlsl
{
    public partial class Typedef
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast
{
    public partial class ExpressionList
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast
{
    public partial class GenericDeclaration
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast
{
    public partial class GenericParameterType
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast
{
    public partial class DeclarationStatement
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast
{
    public partial class ExpressionStatement
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast
{
    public partial class ForStatement
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast
{
    public partial class GenericType
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast
{
    public partial class Identifier
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast
{
    public partial class IfStatement
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast
{
    public partial class IndexerExpression
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast
{
    public partial class KeywordExpression
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast
{
    public partial class Literal
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast
{
    public partial class LiteralExpression
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast
{
    public partial class MatrixType
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast
{
    public partial class MemberReferenceExpression
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast
{
    public partial class MethodDeclaration
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast
{
    public partial class MethodDefinition
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast
{
    public partial class MethodInvocationExpression
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast
{
    public partial class ObjectType
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast
{
    public partial class Parameter
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast
{
    public partial class ParenthesizedExpression
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast
{
    public partial class Qualifier
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast
{
    public partial class ReturnStatement
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast
{
    public partial class ScalarType
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast
{
    public partial class Shader
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast
{
    public partial class StatementList
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast
{
    public partial class StructType
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast
{
    public partial class SwitchCaseGroup
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast
{
    public partial class SwitchStatement
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast
{
    public partial class TypeName
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast
{
    public partial class TypeReferenceExpression
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast
{
    public partial class UnaryExpression
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast
{
    public partial class Variable
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast
{
    public partial class VariableReferenceExpression
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast
{
    public partial class VectorType
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
namespace SiliconStudio.Shaders.Ast
{
    public partial class WhileStatement
    {
        public override void Accept(ShaderVisitor visitor)
        {
            visitor.Visit(this);
        }
        public override TResult Accept<TResult>(ShaderVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}

