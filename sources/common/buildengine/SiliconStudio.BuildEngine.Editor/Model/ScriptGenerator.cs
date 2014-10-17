using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.CSharp;

namespace SiliconStudio.BuildEngine.Editor.Model
{
    internal static class CodeDomExtensions
    {
        public static CodePropertyReferenceExpression Property(this CodeExpression owner, string propertyName)
        {
            return new CodePropertyReferenceExpression(owner, propertyName);
        }

        public static CodeAssignStatement Assign(this CodeExpression left, CodeExpression right)
        {
            return new CodeAssignStatement(left, right);
        }

        public static CodeMethodInvokeExpression Invoke(this CodeExpression invoker, string methodName, params CodeExpression[] parameters)
        {
            //if (parameters == null)
            //    return new CodeMethodInvokeExpression(invoker, methodName);
            return new CodeMethodInvokeExpression(invoker, methodName, parameters);
        }
    }

    public class ScriptGenerator
    {
        private int stepCounter;
        private int cmdCounter;

        private readonly Dictionary<Type, object> defaultObjects = new Dictionary<Type, object>();

        private readonly string buildPath;
        private readonly string outputPath;
        private readonly string sourcePath;
        private readonly string metadataDatabasePath;
        private readonly IEnumerable<SourceFolder> sourceFolders;

        public ScriptGenerator(string buildPath, string outputPath, string sourcePath, IEnumerable<SourceFolder> sourceFolders, string metadataDatabasePath)
        {
            this.buildPath = buildPath;
            this.outputPath = outputPath;
            this.sourcePath = sourcePath;
            this.sourceFolders = sourceFolders;
            this.metadataDatabasePath = metadataDatabasePath;
        }

        public void Generate(string scriptPath, TextWriter writer, ListBuildStep rootBuildStep)
        {
            WriteSource(writer, GenerateBuildGraph(scriptPath, rootBuildStep));
        }

        private static void WriteSource(TextWriter writer, CodeCompileUnit graph)
        {
            var codeProvider = new CSharpCodeProvider();
            var options = new CodeGeneratorOptions { BracingStyle = "C" };
            codeProvider.GenerateCodeFromCompileUnit(graph, writer, options);
        }

        private static IEnumerable<string> GetAssemblyFiles(AssemblyName assemblyName)
        {
            IEnumerable<string> result = Enumerable.Empty<string>();
            return AppDomain.CurrentDomain.GetAssemblies().Where(x => x.GetName().FullName == assemblyName.FullName).Aggregate(result, (r, m) => r.Concat(m.Modules.Select(x => x.Name)));
        }

        private static void RetrieveDependencies(object obj, List<string> dependencies, IEnumerable<string> excludeFiles)
        {
            // ReSharper disable PossibleMultipleEnumeration
            Assembly assembly = obj.GetType().Assembly;

            dependencies.AddRange(GetAssemblyFiles(assembly.GetName()).Where(
                x => dependencies.All(y => x.ToLowerInvariant() != y.ToLowerInvariant())
                  && excludeFiles.All(y => x.ToLowerInvariant() != y.ToLowerInvariant())));

            var buildStep = obj as BuildStep;
            if (buildStep != null)
            {
                foreach (var child in buildStep.GetChildSteps())
                {
                    RetrieveDependencies(child, dependencies, excludeFiles);
                }

                var commandBuildStep = obj as CommandBuildStep;
                if (commandBuildStep != null && commandBuildStep.Command != null)
                {
                    RetrieveDependencies(commandBuildStep.Command, dependencies, excludeFiles);
                }
            }
            // ReSharper restore PossibleMultipleEnumeration
        }

        private CodeCompileUnit GenerateBuildGraph(string scriptPath, ListBuildStep rootBuildStep)
        {
            stepCounter = 0;
            cmdCounter = 0;

            var dependencies = new List<string>();
            RetrieveDependencies(rootBuildStep, dependencies, GetAssemblyFiles(typeof(BuildStep).Assembly.GetName()));


            var compileUnit = new CodeCompileUnit();
            var codeNamespace = new CodeNamespace();
            compileUnit.Namespaces.Add(codeNamespace);

            dependencies.Sort();
            foreach (string dependency in dependencies)
            {
                codeNamespace.Comments.Add(new CodeCommentStatement(new CodeComment("#Assembly " + dependency)));
            }

            if (!string.IsNullOrWhiteSpace(sourcePath))
                codeNamespace.Comments.Add(new CodeCommentStatement(new CodeComment("#SourceBaseDirectory \"" + sourcePath + "\"")));

            if (!string.IsNullOrWhiteSpace(buildPath))
                codeNamespace.Comments.Add(new CodeCommentStatement(new CodeComment("#BuildDirectory \"" + buildPath + "\"")));

            if (!string.IsNullOrWhiteSpace(outputPath))
                codeNamespace.Comments.Add(new CodeCommentStatement(new CodeComment("#OutputDirectory \"" + outputPath + "\"")));

            if (!string.IsNullOrWhiteSpace(metadataDatabasePath))
                codeNamespace.Comments.Add(new CodeCommentStatement(new CodeComment("#MetadataDatabaseDirectory \"" + metadataDatabasePath + "\"")));

            if (!string.IsNullOrWhiteSpace(sourcePath))
            {
                string absoluteSourcePath = Path.Combine(Path.GetDirectoryName(scriptPath) ?? "", sourcePath);
                // Skip ParadoxSdkDir
                foreach (SourceFolder sourceFolder in sourceFolders.Where(sourceFolder => sourceFolder.Name.ToLowerInvariant() != "paradoxsdkdir"))
                {
                    codeNamespace.Comments.Add(new CodeCommentStatement(new CodeComment("#SourceFolder " + "\"" + sourceFolder.Name + "\" \"" + PathExt.GetRelativePath(absoluteSourcePath, sourceFolder.Path) + "\"")));
                }
            }
            codeNamespace.Imports.Add(new CodeNamespaceImport("SiliconStudio.BuildEngine"));

            var codeTypeDeclaration = new CodeTypeDeclaration("BuildScript");

            codeNamespace.Types.Add(codeTypeDeclaration);

            const MemberAttributes Attributes = (MemberAttributes)((int)MemberAttributes.Public | (int)MemberAttributes.Final);
            var codeMethod = new CodeMemberMethod { Attributes = Attributes, Name = "Execute", ReturnType = new CodeTypeReference(typeof(void)) };

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

        private void AssignListItems(CodeStatementCollection statements, CodeExpression owner, object obj)
        {
            Type listInterface = obj.GetType().GetInterface("IList`1");
            if (listInterface != null)
            {
                foreach (var item in (IEnumerable)obj)
                {
                    Type genericArgument = listInterface.GetGenericArguments().First();
                    if (genericArgument.IsValueType || genericArgument == typeof(string))
                        statements.Add(owner.Invoke("Add", Literal(item)));
                    else if (item.GetType().IsSubclassOf(typeof(BuildStep)) && item.GetType() != typeof(EmptyBuildStep))
                    {
                        string stepName = AddBuildStep(statements, item as BuildStep);
                        statements.Add(owner.Invoke("Add", Variable(stepName)));
                    }
                }
            }
        }

        private void AssignProperty(CodeStatementCollection statements, CodeExpression owner, PropertyInfo propertyInfo, object ownerObj)
        {
            if (propertyInfo.GetSetMethod() == null)
                return;

            if (propertyInfo.GetIndexParameters().Length > 0)
                return;

            object value = propertyInfo.GetValue(ownerObj);

            if (propertyInfo.PropertyType.IsPrimitive || propertyInfo.PropertyType == typeof(string))
            {
                object defaultObj;
                if (defaultObjects.TryGetValue(ownerObj.GetType(), out defaultObj))
                {
                    object defaultValue = propertyInfo.GetValue(defaultObj);
                    if ((defaultValue != null && defaultValue.Equals(value)) ||
                        (value != null && value.Equals(defaultValue)) ||
                        defaultValue == value)
                        return;
                }
                statements.Add(owner.Property(propertyInfo.Name).Assign(Literal(value)));
            }
            else if (propertyInfo.PropertyType.IsAssignableFrom(typeof(BuildStep)) && propertyInfo.PropertyType != typeof(object) && propertyInfo.PropertyType != typeof(EmptyBuildStep))
            {
                string stepName = AddBuildStep(statements, value as BuildStep);
                statements.Add(stepName.StartsWith("command") ? owner.Property(propertyInfo.Name).Assign(Construct(value, Variable(stepName))) : owner.Property(propertyInfo.Name).Assign(Variable(stepName)));
            }
            else if (propertyInfo.PropertyType == typeof(object) && ((value != null && value.GetType().IsValueType) || value is string))
            {
                object defaultObj;
                if (defaultObjects.TryGetValue(ownerObj.GetType(), out defaultObj))
                {
                    object defaultValue = propertyInfo.GetValue(defaultObj);
                    if ((defaultValue != null && defaultValue.Equals(value)) ||
                        (value.Equals(defaultValue)) || defaultValue == value)
                        return;
                }
                if (value.GetType().IsPrimitive)
                    statements.Add(owner.Property(propertyInfo.Name).Assign(Literal(value)));
                else if (value is Guid)
                    statements.Add(owner.Property(propertyInfo.Name).Assign(Construct(value, Literal(value.ToString()))));
            }
            else if (value != null)
            {
                // Special case for ListBuildStep's Steps
                if (ownerObj is ListBuildStep && propertyInfo.Name == "Steps")
                    AssignListItems(statements, owner, value);
                else if (propertyInfo.PropertyType.GetInterface("IList`1") != null)
                    AssignListItems(statements, owner.Property(propertyInfo.Name), value);
            }
        }

        private string AddBuildStep(CodeStatementCollection statements, BuildStep step)
        {
            string varName = "step" + ++stepCounter;
            if (!defaultObjects.ContainsKey(step.GetType()) && step.GetType().GetConstructors().Any(x => !x.GetParameters().Any()))
                defaultObjects.Add(step.GetType(), Activator.CreateInstance(step.GetType()));

            var commandBuildStep = step as CommandBuildStep;
            if (commandBuildStep != null)
            {
                var cmdVarName = "command" + ++cmdCounter;
                if (!defaultObjects.ContainsKey(commandBuildStep.Command.GetType()))
                    defaultObjects.Add(commandBuildStep.Command.GetType(), Activator.CreateInstance(commandBuildStep.Command.GetType()));

                statements.Add(Declare(cmdVarName, Construct(commandBuildStep.Command)));
                foreach (PropertyInfo propertyInfo in commandBuildStep.Command.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    AssignProperty(statements, Variable(cmdVarName), propertyInfo, commandBuildStep.Command);
                }
                statements.Add(Declare(varName, Construct(step, Variable(cmdVarName))));
            }
            else
            {
                statements.Add(Declare(varName, Construct(step)));
            }
            foreach (PropertyInfo propertyInfo in step.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.Name != BuildStepPropertiesEnumerator.ParentPropertyName))
            {
                AssignProperty(statements, Variable(varName), propertyInfo, step);
            }
            return varName;
        }

        private static CodeObjectCreateExpression Construct(object obj, params CodeExpression[] parameters)
        {
            return new CodeObjectCreateExpression(obj.GetType(), parameters);
        }

        //private static CodeVariableDeclarationStatement Declare(string typeName, string variableName)
        //{
        //    return new CodeVariableDeclarationStatement(typeName, variableName);
        //}

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
    }
}
