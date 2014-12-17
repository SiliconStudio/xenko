// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if PARADOX_EFFECT_COMPILER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Storage;
using SiliconStudio.Paradox.Shaders.Parser.Ast;
using SiliconStudio.Paradox.Shaders.Parser.Grammar;
using SiliconStudio.Paradox.Shaders.Parser.Utility;
using SiliconStudio.Shaders;
using SiliconStudio.Shaders.Ast;
using SiliconStudio.Shaders.Ast.Hlsl;
using SiliconStudio.Shaders.Parser;
using SiliconStudio.Shaders.Utility;

namespace SiliconStudio.Paradox.Shaders.Parser.Mixins
{
    /// <summary>
    /// Provides methods for loading a <see cref="ShaderClassType"/>.
    /// </summary>
    public class ShaderLoader
    {
        private readonly Dictionary<ShaderSourceKey, ShaderClassType> loadedShaders = new Dictionary<ShaderSourceKey, ShaderClassType>();

        /// <summary>
        /// Gets the source manager.
        /// </summary>
        /// <value>The source manager.</value>
        public ShaderSourceManager SourceManager { get; private set; }

        private readonly static Regex MatchHeader = new Regex(@"\{.*}\s*;", RegexOptions.Singleline | RegexOptions.Compiled);

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderLoader"/> class.
        /// </summary>
        /// <param name="sourceManager">The source manager.</param>
        /// <exception cref="System.ArgumentNullException">sourceManager</exception>
        public ShaderLoader(ShaderSourceManager sourceManager)
        {
            if (sourceManager == null)
                throw new ArgumentNullException("sourceManager");

            SourceManager = sourceManager;
        }

        /// <summary>
        /// Deletes the shader cache for the specified shaders.
        /// </summary>
        /// <param name="modifiedShaders">The modified shaders.</param>
        public void DeleteObsoleteCache(HashSet<string> modifiedShaders)
        {
            var keysToRemove = new HashSet<ShaderSourceKey>();
            foreach (var shaderName in modifiedShaders)
            {
                foreach (var key in loadedShaders.Keys)
                {
                    if (key.TypeName == shaderName)
                        keysToRemove.Add(key);
                }
            }

            foreach (var key in keysToRemove)
                loadedShaders.Remove(key);

            keysToRemove.Clear();

            SourceManager.DeleteObsoleteCache(modifiedShaders);
        }

        /// <summary>
        /// Loads the <see cref="ShaderClassType" />.
        /// </summary>
        /// <param name="shaderClassSource">The shader class source.</param>
        /// <param name="shaderMacros">The shader macros.</param>
        /// <param name="log">The log to output error logs.</param>
        /// <param name="autoGenericInstances"></param>
        /// <returns>A ShaderClassType or null if there was some errors.</returns>
        /// <exception cref="System.ArgumentNullException">shaderClassSource</exception>
        public ShaderClassType LoadClassSource(ShaderClassSource shaderClassSource, SiliconStudio.Shaders.Parser.ShaderMacro[] shaderMacros, LoggerResult log, bool autoGenericInstances)
        {
            if (shaderClassSource == null) throw new ArgumentNullException("shaderClassSource");

            string generics = null;
            if (shaderClassSource.GenericArguments != null)
            {
                generics = "";
                foreach (var gen in shaderClassSource.GenericArguments)
                    generics += "___" + gen;
            }
            var shaderClassType = LoadShaderClass(shaderClassSource.ClassName, generics, log, shaderMacros);

            if (shaderClassType == null)
                return null;

            // Instantiate generic class
            if (shaderClassSource.GenericArguments != null || (shaderClassType.ShaderGenerics.Count > 0 && autoGenericInstances))
            {
                if (shaderClassType.IsInstanciated)
                    return shaderClassType;

                // If we want to automatically generate a generic instance (in case we just want to parse and verify the generic)
                if (autoGenericInstances)
                {
                    shaderClassSource.GenericArguments = new string[shaderClassType.ShaderGenerics.Count];
                    for (int i = 0; i < shaderClassSource.GenericArguments.Length; i++)
                    {
                        var variableGeneric = shaderClassType.ShaderGenerics[i];
                        shaderClassSource.GenericArguments[i] = GetDefaultConstValue(variableGeneric);
                    }
                }

                if (shaderClassSource.GenericArguments.Length != shaderClassType.ShaderGenerics.Count)
                {
                    log.Error(ParadoxMessageCode.WrongGenericNumber, shaderClassType.Span, shaderClassSource.ClassName);
                    return null;
                }

                // check the name of the generics
                foreach (var generic in shaderClassType.ShaderGenerics)
                {
                    foreach (var genericCompare in shaderClassType.ShaderGenerics.Where(x => x != generic))
                    {
                        if (generic.Name.Text == genericCompare.Name.Text)
                            log.Error(ParadoxMessageCode.SameNameGenerics, generic.Span, generic, genericCompare, shaderClassSource.ClassName);
                    }
                }

                if (log.HasErrors)
                    return null;

                // When we use an actual generic instance, we replace the name with the name of the class + a hash of the generic parameters
                if (!autoGenericInstances)
                {
                    var className = GenerateGenericClassName(shaderClassSource);
                    shaderClassType.Name = new Identifier(className);
                }

                var genericAssociation = CreateGenericAssociation(shaderClassType.ShaderGenerics, shaderClassSource.GenericArguments);
                var identifierGenerics = GenerateIdentifierFromGenerics(genericAssociation);
                var expressionGenerics = GenerateGenericsExpressionValues(shaderClassType.ShaderGenerics, shaderClassSource.GenericArguments);
                ParadoxClassInstantiator.Instantiate(shaderClassType, expressionGenerics, identifierGenerics, log);
                shaderClassType.ShaderGenerics.Clear();
                shaderClassType.IsInstanciated = true;
            }
            return shaderClassType;
        }

        private static string GetDefaultConstValue(Variable variable)
        {
            var variableType = variable.Type;
            if (variableType == ScalarType.Bool)
            {
                return "false";
            }
            if (variableType != TypeBase.Void && variableType != TypeBase.String)
            {
                return "0";
            }

            return string.Empty;
        }


        Dictionary<string, object> CreateGenericAssociation(List<Variable> genericParameters, object[] genericArguments)
        {
            var result = new Dictionary<string, object>();
            for (var i = 0; i < genericParameters.Count; ++i)
            {
                result.Add(genericParameters[i].Name.Text, genericArguments[i]);
            }
            return result;
        }

        Dictionary<string, Identifier> GenerateIdentifierFromGenerics(Dictionary<string, object> generics)
        {
            var result = new Dictionary<string, Identifier>();
            foreach (var genericPair in generics)
            {
                var generic = genericPair.Value;
                if (generic is Identifier)
                    result.Add(genericPair.Key, (Identifier)generic);
                else //if (generic is string)
                {
                    var stringGeneric = generic.ToString();// generic as string;
                    var stringParts = stringGeneric.Split('.');
                    if (stringParts.Length == 1)
                        result.Add(genericPair.Key, new Identifier(stringGeneric));
                    else
                    {
                        var dotIdentifier = new IdentifierDot();
                        dotIdentifier.Identifiers = stringParts.Select(x => new Identifier(x)).ToList();
                        result.Add(genericPair.Key, dotIdentifier);
                    }
                }
                //else
                //    throw new Exception("Unsupported generic.");
            }
            return result;
        }

        private Dictionary<string, Expression> GenerateGenericsExpressionValues(List<Variable> genericParameters, object[] genericArguments)
        {
            var result = new Dictionary<string, Expression>();

            if (genericArguments.Length > 0)
            {
                var allGenerics = new StringBuilder();
                bool allEmpty = true;
                for (int i = 0; i < genericArguments.Length; i++)
                {
                    var generic = genericArguments[i];
                    var genericText = generic.ToString();

                    if (!string.IsNullOrWhiteSpace(genericText))
                    {
                        allEmpty = false;
                    }
                    if (i > 0)
                    {
                        allGenerics.Append(',');
                    }

                    // TODO: If a generic is empty, we should throw an error
                    allGenerics.Append(genericText);
                }

                if (allEmpty)
                {
                    for (var i = 0; i < genericArguments.Length; ++i)
                        result.Add(genericParameters[i].Name.Text, null);
                }
                else
                {
                    var node = CreateExpressionFromString(allGenerics.ToString());

                    if (node is ExpressionList)
                    {
                        var nodeList = (ExpressionList)node;
                        if (nodeList.Count != genericArguments.Length)
                            throw new Exception("mismatch generic length after parsing");

                        for (var i = 0; i < genericArguments.Length; ++i)
                            result.Add(genericParameters[i].Name.Text, nodeList[i]);
                    }
                    else
                    {
                        if (genericArguments.Length != 1)
                            throw new Exception("mismatch generic length after parsing");
                        result.Add(genericParameters[0].Name.Text, node);
                    }
                }
            }
            return result;
        }

        Expression CreateExpressionFromString(string name)
        {
            // TODO: catch errors
            var result = ShaderParser.GetParser<ParadoxGrammar>(ShaderParser.GetGrammar<ParadoxGrammar>().ExpressionNonTerminal).Parser.Parse(name, "");
            return (Expression)result.Root.AstNode;
        }

        private ShaderClassType LoadShaderClass(string type, string generics, LoggerResult log, SiliconStudio.Shaders.Parser.ShaderMacro[] macros = null)
        {
            if (type == null) throw new ArgumentNullException("type");
            var shaderSourceKey = new ShaderSourceKey(type, generics, macros);

            lock (loadedShaders)
            {
                // Already instantiated
                ShaderClassType shaderClass;

                if (loadedShaders.TryGetValue(shaderSourceKey, out shaderClass))
                {
                    return shaderClass;
                }

                // Load file
                var shaderSource = SourceManager.LoadShaderSource(type);

                string preprocessedSource;
                try
                {
                    preprocessedSource = PreProcessor.Run(shaderSource.Source, shaderSource.Path, macros);
                }
                catch (Exception ex)
                {
                    log.Error(MessageCode.ErrorUnexpectedException, new SourceSpan(new SourceLocation(shaderSource.Path, 0, 1, 1),1), ex);
                    return null;
                }

                byte[] byteArray = Encoding.ASCII.GetBytes(preprocessedSource);
                var hashPreprocessSource = ObjectId.FromBytes(byteArray);
   
                // Compile
                var parsingResult = ParadoxShaderParser.TryParse(preprocessedSource, shaderSource.Path);
                parsingResult.CopyTo(log);

                if (parsingResult.HasErrors)
                {
                    return null;
                }

                var shader = parsingResult.Shader;

                // As shaders can be embedded in namespaces, get only the shader class and make sure there is only one in a pdxsl.
                var shaderClassTypes = ParadoxShaderParser.GetShaderClassTypes(shader.Declarations).ToList();
                if (shaderClassTypes.Count != 1)
                {
                    var sourceSpan = new SourceSpan(new SourceLocation(shaderSource.Path, 0, 0, 0), 1);
                    if (shaderClassTypes.Count > 1)
                    {
                        sourceSpan = shaderClassTypes[1].Span;
                    }
                    log.Error(ParadoxMessageCode.ShaderMustContainSingleClassDeclaration, sourceSpan, type);
                    return null;
                }

                shaderClass = shaderClassTypes.First();
                shaderClass.SourcePath = shaderSource.Path;
                shaderClass.SourceHash = shaderSource.Hash;
                shaderClass.PreprocessedSourceHash = hashPreprocessSource;
                shaderClass.IsInstanciated = false;

                // TODO: We should not use Console. Change the way we log things here
                // Console.WriteLine("Loading Shader {0}{1}", type, macros != null && macros.Length > 0 ? String.Format("<{0}>", string.Join(", ", macros)) : string.Empty);

                // If the file name is not matching the class name, provide an error
                if (shaderClass.Name.Text != type)
                {
                    log.Error(ParadoxMessageCode.FileNameNotMatchingClassName, shaderClass.Name.Span, type, shaderClass.Name.Text);
                    return null;
                }

                loadedShaders.Add(shaderSourceKey, shaderClass);

                return shaderClass;
            }
        }

        public ShaderClassType ParseSource(string shaderSource, LoggerResult log)
        {
            var parsingResult = ParadoxShaderParser.TryParse(shaderSource, null);
            parsingResult.CopyTo(log);

            if (parsingResult.HasErrors)
            {
                return null;
            }

            var shader = parsingResult.Shader;
            var shaderClass = shader.Declarations.OfType<ShaderClassType>().Single();
            shaderClass.SourcePath = null;// shaderSource.Path;
            shaderClass.SourceHash = ObjectId.Empty;// shaderSource.Hash;
            shaderClass.PreprocessedSourceHash = ObjectId.Empty; // hashPreprocessSource;
            shaderClass.IsInstanciated = false;

            return shaderClass;
        }

        public bool ClassExists(string className)
        {
            return SourceManager.IsClassExists(className);
        }

        private static Dictionary<string, string> GenerateGenericMapping(ShaderClassType shaderClassType, IList<object> genericParameters)
        {
            var identifierGenericClass = shaderClassType.GenericParameters;
            if (identifierGenericClass.Count != genericParameters.Count)
                throw new InvalidOperationException("Number of parameters in this generic instantiation doesn't match.");

            // Build generic mapping (i.e. T => RealType, U => RealType2)
            return identifierGenericClass
                .Select((value, index) => new { value, index })
                .ToDictionary(x => x.value.Name.Text, x => genericParameters[x.index].ToString());
        }

        private static string GenerateGenericClassName(ShaderClassType shaderClassType)
        {
            // Generate class name
            return shaderClassType.Name.Text + (shaderClassType.GenericParameters == null ? string.Empty : "_" + string.Join("_", shaderClassType.GenericParameters.Select(x => x.ToString().Replace('.', '_'))));
        }

        private static string GenerateGenericClassName(ShaderClassSource source)
        {
            // Generate class name
            if (source.GenericArguments != null && source.GenericArguments.Length > 0)
            {
                var hash = source.GenericArguments[0].ToString().GetHashCode();
                for (var i = 0; i < source.GenericArguments.Length; ++i)
                {
                    hash = (hash * 397) ^ source.GenericArguments[i].ToString().GetHashCode();
                }

                if (hash < 0)
                {
                    hash = -hash;
                    return source.ClassName + "_Min" + hash.ToString();
                }

                return source.ClassName + "_" + hash.ToString();
            }
            return source.ClassName;
        }

        private class ShaderSourceKey : IEquatable<ShaderSourceKey>
        {
            public readonly string TypeName;
            private readonly string generics;
            private readonly SiliconStudio.Shaders.Parser.ShaderMacro[] shaderMacros;
            private readonly int hashCode;

            public ShaderSourceKey(string typeName, string generics, SiliconStudio.Shaders.Parser.ShaderMacro[] shaderMacros)
            {
                this.TypeName = typeName;
                this.generics = generics;
                this.shaderMacros = shaderMacros;
                unchecked
                {
                    hashCode = ((typeName != null ? typeName.GetHashCode() : 0) * 397) ^ (generics != null ? generics.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (this.shaderMacros != null ? this.shaderMacros.ComputeHash() : 0);
                }
            }

            public bool Equals(ShaderSourceKey other)
            {
                return Equals(other.TypeName, TypeName) && Equals(other.generics, generics) && ArrayExtensions.ArraysEqual(other.shaderMacros, shaderMacros);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (obj.GetType() != typeof(ShaderSourceKey)) return false;
                return Equals((ShaderSourceKey)obj);
            }

            public override int GetHashCode()
            {
                return hashCode;
            }
        }
    }
}
#endif