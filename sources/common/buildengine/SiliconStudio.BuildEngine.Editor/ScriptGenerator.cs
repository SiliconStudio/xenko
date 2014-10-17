using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.CSharp;

namespace SiliconStudio.BuildEngine.Editor
{
    public static class ScriptGenerator
    {
        private static int stepCounter;
        private static int cmdCounter;

        public static void Generate(TextWriter writer, ListBuildStep rootBuildStep)
        {
            WriteSource(writer, GenerateBuildGraph(rootBuildStep));
        }

        private static void WriteSource(TextWriter writer, CodeCompileUnit graph)
        {
            var codeProvider = new CSharpCodeProvider();
            var options = new CodeGeneratorOptions { BracingStyle = "C" };
            codeProvider.GenerateCodeFromCompileUnit(graph, writer, options);
        }

        private static CodeCompileUnit GenerateBuildGraph(ListBuildStep rootBuildStep)
        {
            stepCounter = 0;
            cmdCounter = 0;

            var compileUnit = new CodeCompileUnit();
            var codeNamespace = new CodeNamespace("BuildTool");
            compileUnit.Namespaces.Add(codeNamespace);

            codeNamespace.Imports.Add(new CodeNamespaceImport("SiliconStudio.BuildTool"));

            var codeTypeDeclaration = new CodeTypeDeclaration("BuildScript");
            codeNamespace.Types.Add(codeTypeDeclaration);

            var codeMethod = new CodeMemberMethod { Attributes = MemberAttributes.Public, Name = "Execute", ReturnType = new CodeTypeReference(typeof(void)) };

            codeMethod.Parameters.Add(new CodeParameterDeclarationExpression("Builder", "builder"));
            codeMethod.Parameters.Add(new CodeParameterDeclarationExpression("ListBuildStep", "buildSteps"));

            foreach (var step in rootBuildStep.Steps)
            {
                string stepName = AddBuildStep(codeMethod.Statements, step);
                codeMethod.Statements.Add(Variable("buildSteps").Invoke("Add", Variable(stepName)));
            }

            codeTypeDeclaration.Members.Add(codeMethod);
            return compileUnit;
        }

        private static void AssignListItems(CodeStatementCollection statements, CodeExpression owner, object obj)
        {
            Type listInterface = obj.GetType().GetInterface("IList`1");
            if (listInterface != null)
            {
                foreach (var item in (IEnumerable)obj)
                {
                    Type genericArgument = listInterface.GetGenericArguments().First();
                    if (genericArgument.IsValueType || genericArgument == typeof(string))
                        statements.Add(owner.Invoke("Add", Literal(item)));
                    else if (item.GetType().IsSubclassOf(typeof(BuildStep)))
                    {
                        string stepName = AddBuildStep(statements, item as BuildStep);
                        statements.Add(owner.Invoke("Add", Variable(stepName)));
                    }
                }
            }

        }

        private static void AssignProperty(CodeStatementCollection statements, CodeExpression owner, PropertyInfo propertyInfo, object ownerObj)
        {
            if (propertyInfo.GetSetMethod() == null)
                return;

            if (propertyInfo.GetIndexParameters().Length > 0)
                return;

            object value = propertyInfo.GetValue(ownerObj);

            if (propertyInfo.PropertyType.IsValueType || propertyInfo.PropertyType == typeof(string))
            {
                statements.Add(owner.Property(propertyInfo.Name).Assign(Literal(value)));
            }
            else if (propertyInfo.PropertyType.IsSubclassOf(typeof(BuildStep)))
            {
                string stepName = AddBuildStep(statements, value as BuildStep);
                statements.Add(owner.Property(propertyInfo.Name).Assign(Variable(stepName)));
            }
            else
            {
                AssignListItems(statements, owner.Property(propertyInfo.Name), value);
            }
        }

        private static string AddBuildStep(CodeStatementCollection statements, BuildStep step)
        {
            string varName;

            var commandBuildStep = step as CommandBuildStep;
            if (commandBuildStep != null)
            {
                varName = "command" + ++cmdCounter;
                statements.Add(Declare(varName, Construct(commandBuildStep.Command)));
                foreach (PropertyInfo propertyInfo in commandBuildStep.Command.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    AssignProperty(statements, Variable(varName), propertyInfo, commandBuildStep.Command);
                }
            }
            else
            {
                varName = "step" + ++stepCounter;

                statements.Add(Declare(varName, Construct(step)));

                foreach (PropertyInfo propertyInfo in step.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {

                    AssignProperty(statements, Variable(varName), propertyInfo, step);
                }
            }
            return varName;
        }

        private static CodeObjectCreateExpression Construct(object obj, params CodeExpression[] parameters)
        {
            return new CodeObjectCreateExpression(obj.GetType(), parameters);
        }

        private static CodeVariableDeclarationStatement Declare(string typeName, string variableName)
        {
            return new CodeVariableDeclarationStatement(typeName, variableName);
        }

        private static CodeVariableDeclarationStatement Declare(string variableName, CodeExpression initialValue)
        {
            return new CodeVariableDeclarationStatement("var", variableName, initialValue);
        }

        private static CodeVariableReferenceExpression Variable(string variableName)
        {
            return new CodeVariableReferenceExpression(variableName);
        }

        private static CodePrimitiveExpression Literal(object value)
        {
            return new CodePrimitiveExpression(value);
        }

        private static CodePropertyReferenceExpression Property(this CodeExpression owner, string propertyName)
        {
            return new CodePropertyReferenceExpression(owner, propertyName);
        }

        private static CodeAssignStatement Assign(this CodeExpression left, CodeExpression right)
        {
            return new CodeAssignStatement(left, right);
        }

        private static CodeMethodInvokeExpression Invoke(this CodeExpression invoker, string methodName, params CodeExpression[] parameters)
        {
            //if (parameters == null)
            //    return new CodeMethodInvokeExpression(invoker, methodName);
            return new CodeMethodInvokeExpression(invoker, methodName, parameters);
        }
    }
}
