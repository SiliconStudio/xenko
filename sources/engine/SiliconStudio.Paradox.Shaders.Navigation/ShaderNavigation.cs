// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using SiliconStudio.Assets;
using SiliconStudio.Paradox.Shaders.Parser;
using SiliconStudio.Paradox.Shaders.Parser.Mixins;
using SiliconStudio.Shaders.Ast;

namespace SiliconStudio.Paradox.Shaders.Navigation
{
    public class ShaderNavigation
    {
        public SiliconStudio.Shaders.Ast.SourceSpan? FindDeclaration(string packagePath, string shaderSource, SiliconStudio.Shaders.Ast.SourceLocation location)
        {
            var mixer = new ShaderMixinParser();
            mixer.SourceManager.UseFileSystem = true;
            mixer.SourceManager.LookupDirectoryList.AddRange(CollectShadersDirectories(packagePath));

            var shaderSourceName = Path.GetFileNameWithoutExtension(location.FileSource);
            mixer.SourceManager.AddShaderSource(shaderSourceName, shaderSource, location.FileSource);

            var mixinSource = new ShaderMixinSource();
            mixinSource.Mixins.Add(new ShaderClassSource(shaderSourceName));

            ShaderMixinParsingResult parsingResult;
            HashSet<ModuleMixinInfo> moduleMixins;
            var result = mixer.ParseAndAnalyze(mixinSource, null, out parsingResult, out moduleMixins);

            var mixin = result.MixinInfos.FirstOrDefault(item => item.MixinName == shaderSourceName);

            // var ast = mixin.MixinAst;

            var parsingInfo = mixin.Mixin.ParsingInfo;

            var pools = new List<ReferencesPool>
            {
                parsingInfo.ClassReferences,
                parsingInfo.StaticReferences,
                parsingInfo.ExternReferences,
                parsingInfo.StageInitReferences,
            };

            var word = FindWord(shaderSource, location);

            foreach (var pool in pools)
            {
                var span = Find(pool, word, location);
                if (span.HasValue)
                {
                    return span.Value;
                }
            }

            return null;
        }

        private SourceSpan? Find(ReferencesPool pool, string matchingWord, SiliconStudio.Shaders.Ast.SourceLocation location)
        {
            foreach (var methodRef in pool.MethodsReferences)
            {
                foreach (var expression in methodRef.Value)
                {
                    if (IsExpressionMatching(expression, matchingWord, location))
                    {
                        return methodRef.Key.Span;
                    }
                }
            }

            foreach (var variableRef in pool.VariablesReferences)
            {
                foreach (var expression in variableRef.Value)
                {
                    if (IsExpressionMatching(expression.Expression, matchingWord, location))
                    {
                        return variableRef.Key.Span;
                    }
                }
            }
            return null;
        }

        private bool IsExpressionMatching(Expression expression, string matchingWord, SiliconStudio.Shaders.Ast.SourceLocation location)
        {
            var span = expression.Span;
            var startColumn = span.Location.Column;
            var endColumn = startColumn + span.Length;
            if (expression.Span.Location.Line == location.Line && location.Column >= startColumn)
            {
                var stringExpression = expression.ToString();

                if (location.Column <= endColumn || (matchingWord != null && stringExpression.Contains(matchingWord)))
                {
                    return true;
                }
            }
            return false;
        }

        private static Regex matchName = new Regex("[a-zA-Z_][a-z-A-Z_0-9]*");

        private string FindWord(string shaderSource, SiliconStudio.Shaders.Ast.SourceLocation location)
        {
            var stringReader = new StringReader(shaderSource);
            string line;
            int index = 0;
            while ((line = stringReader.ReadLine()) != null)
            {
                index++;
                if (location.Line == index)
                {
                    int startColumn = location.Column;
                    while (startColumn > 0 && char.IsLetterOrDigit(line[startColumn]))
                    {
                        startColumn--;
                    }
                    if (startColumn != 0)
                    {
                        startColumn++;
                    }

                    var result = matchName.Match(line, startColumn);
                    if (result.Success)
                    {
                        return result.Groups[0].Value;
                    }

                }
            }
            return null;
        }

        private List<string> CollectShadersDirectories(string packagePath)
        {
            if (packagePath == null)
            {
                packagePath = PackageStore.Instance.DefaultPackage.FullPath;
            }

            var defaultLoad = PackageLoadParameters.Default();
            defaultLoad.AutoCompileProjects = false;
            defaultLoad.AutoLoadTemporaryAssets = false;
            defaultLoad.ConvertUPathToAbsolute = false;
            defaultLoad.GenerateNewAssetIds = false;
            defaultLoad.LoadAssemblyReferences = false;

            var sessionResult = PackageSession.Load(packagePath, defaultLoad);

            if (sessionResult.HasErrors)
            {
                // TODO: Throw an error
                return null;
            }

            var session = sessionResult.Session;

            var assetsPaths = new List<string>();
            foreach (var package in session.Packages)
            {
                foreach (var profile in package.Profiles)
                {
                    foreach (var folder in profile.AssetFolders)
                    {
                        var fullPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(packagePath), folder.Path));

                        assetsPaths.Add(fullPath);
                        assetsPaths.AddRange(Directory.EnumerateDirectories(fullPath, "*.*", SearchOption.AllDirectories));
                    }
                }
            }
            return assetsPaths;
        }


        static void Main()
        {
            var navigation = new ShaderNavigation();

            var shaderSourcePath = @"C:\Code\Paradox\sources\engine\SiliconStudio.Paradox.Graphics\Shaders\SpriteBase.pdxsl";
            var shaderSourceCode = File.ReadAllText(shaderSourcePath);
            //var span = navigation.FindDeclaration(null, shaderSourceCode, new SourceLocation(shaderSourcePath, 0, 21, 64));
            var span = navigation.FindDeclaration(null, shaderSourceCode, new SourceLocation(shaderSourcePath, 0, 21, 25));
        }
    }
}