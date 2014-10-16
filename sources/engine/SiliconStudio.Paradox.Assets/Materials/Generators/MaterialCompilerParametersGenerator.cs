// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Core.Storage;
using SiliconStudio.Paradox.Assets.Effect;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.Data;
using SiliconStudio.Paradox.Shaders.Compiler;

namespace SiliconStudio.Paradox.Assets.Materials.Generators
{
    class MaterialCompilerParametersGenerator : CompilerParameterGeneratorBase
    {
        public override int GeneratorPriority
        {
            get
            {
                return 10;
            }
        }
        
        public override IEnumerable<CompilerParameters> Generate(AssetCompilerContext context, CompilerParameters baseParameters, ILogger log)
        {
            var allMaterialParameters = new List<ParameterCollection>();
            if (baseParameters.Get(MaterialAssetKeys.GenerateShader))
            {
                var assetManager = new AssetManager();
                var settings = new AssetManagerLoaderSettings()
                {
                    ContentFilter = AssetManagerLoaderSettings.NewContentFilterByType(typeof(MaterialData)),
                };

                var hashParameters = new HashSet<ObjectId>();

                foreach (var materialAssetItem in context.Package.Assets.Where(item => item.Asset is MaterialAsset))
                {
                    var assetPath = materialAssetItem.Location.GetDirectoryAndFileName();
                    try
                    {
                        var materialData = assetManager.Load<MaterialData>(assetPath, settings);
                        if (materialData != null && materialData.Parameters != null && materialData.Parameters.Count > 0)
                        {
                            var materialParameters = new ParameterCollection();
                            AddToParameters(materialData.Parameters, materialParameters);
                            if (materialParameters.Count > 0)
                            {
                                byte[] buffer1;
                                var id = ObjectId.FromObject(materialParameters, out buffer1);
                                if (!hashParameters.Contains(id))
                                {
                                    hashParameters.Add(id);
                                    allMaterialParameters.Add(materialParameters);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("Error while loading material [{0}]", ex, assetPath);
                    }
                }
            }

            if (allMaterialParameters.Count != 0)
            {
                foreach (var materialParams in allMaterialParameters)
                {
                    var compilerParameters = baseParameters.Clone();
                    materialParams.CopyTo(compilerParameters);
                    yield return compilerParameters;
                }
            }
            else
            {
                yield return baseParameters.Clone();
            }
        }

        [ModuleInitializer]
        internal static void Register()
        {
            // Register an instance of this generator to the effect asset compiler.
            EffectLibraryAssetCompiler.RegisterCompilerParametersGenerator(new MaterialCompilerParametersGenerator());
        }
    }
}
