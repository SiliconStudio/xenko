// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Xenko.Shaders;
using SiliconStudio.Xenko.Shaders.Compiler;
using SiliconStudio.Shaders.Ast.Xenko;
using SiliconStudio.Xenko.Shaders.Parser.Mixins;

namespace SiliconStudio.Xenko.Assets.Materials
{
    [Obsolete]
    internal class MaterialNodeClassLoader
    {
        /// <summary>
        /// static and unique instance of the loader.
        /// </summary>
        private static MaterialNodeClassLoader materialNodeClassLoader;

        /// <summary>
        /// Get the unique instance of the class.
        /// </summary>
        /// <returns></returns>
        public static MaterialNodeClassLoader GetLoader()
        {
            if (materialNodeClassLoader == null)
                materialNodeClassLoader = new MaterialNodeClassLoader();
            return materialNodeClassLoader;
        }

        /// <summary>
        /// The source manager.
        /// </summary>
        private readonly ShaderSourceManager manager;
        
        /// <summary>
        /// The shader loader.
        /// </summary>
        private readonly ShaderLoader loader;
        
        /// <summary>
        /// The logger.
        /// </summary>
        private readonly SiliconStudio.Shaders.Utility.LoggerResult logger;

        private MaterialNodeClassLoader()
        {
            manager = new ShaderSourceManager();
            manager.LookupDirectoryList.Add(EffectCompilerBase.DefaultSourceShaderFolder);
            logger = new SiliconStudio.Shaders.Utility.LoggerResult();
            loader = new ShaderLoader(manager);
        }

        /// <summary>
        /// Get the shader.
        /// </summary>
        /// <param name="name">The name of the shader.</param>
        /// <returns>The shader.</returns>
        public ShaderClassType GetShader(string name)
        {
            try
            {
                if (!loader.ClassExists(name))
                    return null;

                var shader = loader.LoadClassSource(new ShaderClassSource(name), null, logger, false);
                if (logger.HasErrors)
                {
                    // TODO: output messages
                    logger.Messages.Clear();
                    return null;
                }
                return shader.Type;
            }
            catch
            {
                // TODO: output messages
                return null;
            }
        }
    }
}
