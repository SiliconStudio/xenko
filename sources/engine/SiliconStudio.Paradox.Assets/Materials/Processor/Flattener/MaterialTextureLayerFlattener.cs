// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Paradox.Assets.Materials.Nodes;
using SiliconStudio.Paradox.Assets.Materials.Processor.Visitors;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Graphics.Data;
using SiliconStudio.Paradox.Shaders;
using SiliconStudio.Paradox.Shaders.Compiler;

namespace SiliconStudio.Paradox.Assets.Materials.Processor.Flattener
{
    public class MaterialTextureLayerFlattener : IDisposable
    {
        #region Private members

        /// <summary>
        /// Generated texture index.
        /// </summary>
        private static int textureIndex = 0;

        /// <summary>
        /// The GraphicsDevice used to perform GPU commands
        /// </summary>
        private GraphicsDevice graphicsDevice;

        /// <summary>
        /// Plane used to draw on screen
        /// </summary>
        private GeometricPrimitive plane;

        /// <summary>
        /// List of commands to reduce the trees.
        /// </summary>
        private readonly List<MaterialReductionInfo> commandList = new List<MaterialReductionInfo>();

        #endregion

        #region Public properties

        /// <summary>
        /// Current material.
        /// </summary>
        public MaterialDescription Material { get; private set; }

        /// <summary>
        /// States if there are commands to execute.
        /// </summary>
        public bool HasCommands
        {
            get
            {
                return commandList.Count > 0;
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="mat">The material to reduce.</param>
        public MaterialTextureLayerFlattener(MaterialDescription mat, GraphicsDevice device)
        {
            graphicsDevice = device;
            Material = mat.Clone();
        }

        /// <summary>
        /// Release the allocated data.
        /// </summary>
        public void Dispose()
        {
            if (plane != null)
                plane.Dispose();
        }

        /// <summary>
        /// Performs the maximal reduction.
        /// </summary>
        //public Dictionary<UFile, Image> Run(EffectCompilerBase compiler)
        public bool Run(EffectCompilerBase compiler)
        {
            var result = true;

            if (commandList.Count > 0)
            {
                if (plane == null)
                    plane = GeometricPrimitive.Plane.New(graphicsDevice, 2.0f, 2.0f);

                var assetManager = new AssetManager();
                assetManager.Serializer.RegisterSerializer(new GpuTextureSerializer2(graphicsDevice));

                var textures = new Dictionary<string, Graphics.Texture>();
                var materialTreeShaderCreator = new MaterialTreeShaderCreator(Material);
                var textureVisitor = new MaterialTextureVisitor(Material);
                var compilerParameters = new CompilerParameters { Platform = GraphicsPlatform.Direct3D11, Profile = GraphicsProfile.Level_11_0 };

                foreach (var command in commandList)
                {
                    var computeColorShader = materialTreeShaderCreator.GenerateShaderForReduction(command.OldNode);
                    if (computeColorShader == null)
                        continue;

                    var finalShader = new ShaderMixinSource();
                    finalShader.Mixins.Add(new ShaderClassSource("FlattenLayers"));
                    finalShader.Compositions.Add("outColor", computeColorShader);
                    var results = compiler.Compile(finalShader, compilerParameters);

                    if (results.HasErrors)
                        continue;

                    command.TreeEffect = new Graphics.Effect(graphicsDevice, results.MainBytecode, results.MainUsedParameters);
                    command.Parameters = new ParameterCollection();
                    var maxWidth = 0;
                    var maxHeight = 0;
                    var allTextures = textureVisitor.GetAllTextureValues(command.OldNode);
                    foreach (var texSlot in allTextures)
                    {
                        Graphics.Texture tex;
                        if (!textures.TryGetValue(texSlot.TextureName, out tex))
                        {
                            //TODO: change load so that texture can be unloaded.
                            tex = assetManager.Load<Graphics.Texture>(texSlot.TextureName);
                            textures.Add(texSlot.TextureName, tex);
                        }

                        if (tex == null)
                            throw new FileNotFoundException("Texture " + texSlot.TextureName + " not found");

                        command.Parameters.Set(texSlot.UsedParameterKey, tex);
                        maxWidth = Math.Max(maxWidth, tex.ViewWidth);
                        maxHeight = Math.Max(maxHeight, tex.ViewHeight);
                        // can take min, a user-defined size, or clamp the min/max
                        // exclude mask?
                    }

                    command.RenderTarget = Graphics.Texture.New2D(graphicsDevice, maxWidth, maxHeight, PixelFormat.R8G8B8A8_UNorm, TextureFlags.ShaderResource | TextureFlags.RenderTarget);
                    command.ToExecute = true;
                }

                // remove wrong commands
                commandList.RemoveAll(x => !x.ToExecute);

                var nodeReplacer = new MaterialNodeReplacer(Material);

                foreach (var command in commandList.Where(x => x.ToExecute))
                {
                    lock (graphicsDevice)
                    {
                        graphicsDevice.Clear(command.RenderTarget, Color4.Black);
                        graphicsDevice.SetRenderTarget(command.RenderTarget);

                        graphicsDevice.SetRasterizerState(graphicsDevice.RasterizerStates.CullNone);
                        graphicsDevice.SetDepthStencilState(graphicsDevice.DepthStencilStates.None);

                        command.TreeEffect.Apply(command.Parameters);
                        plane.Draw();

                        // save texture
                        SaveTexture(command.RenderTarget, command.TextureUrl, assetManager);
                    }
                    // make new tree
                    var newNode = new MaterialTextureNode(command.TextureUrl.FullPath, command.TexcoordIndex, Vector2.One, Vector2.Zero);

                    nodeReplacer.Replace(command.OldNode, newNode);

                    // save new material?
                    command.ToExecute = false;
                }

                foreach (var command in commandList)
                {
                    command.TreeEffect.Dispose();
                    command.RenderTarget.Dispose();
                }

                foreach (var texture in textures)
                {
                    texture.Value.Dispose();
                }
                textures.Clear();

                foreach (var tex in textures)
                {
                    assetManager.Unload(tex);
                }

                textures.Clear();
                result = commandList.All(x => !x.ToExecute);
                commandList.Clear();
            }

            return result;
        }
        
        /// <summary>
        /// Create the structures to perform the reductions.
        /// </summary>
        public void PrepareForFlattening(UDirectory baseDir = null)
        {
            var reducer = new MaterialTreeReducer(Material);
            reducer.ReduceTrees();
            var reducedTrees = reducer.ReducedTrees;
            var textureVisitor = new MaterialTextureVisitor(Material);

            foreach (var materialReference in reducedTrees)
            {
                Material.Nodes[materialReference.Key] = materialReference.Value;
                textureVisitor.AssignDefaultTextureKeys(materialReference.Value);
            }

            commandList.Clear();
            textureVisitor.ResetTextureIndices();

            var basePath = (baseDir == null || baseDir.GetDirectory() == "" ? "" : baseDir.ToString() + "/") + "__reduced_textures__";

            foreach (var materialReferenceKey in Material.Nodes)
            {
                if (materialReferenceKey.Value == null)
                    continue;

                var materialReferenceName = materialReferenceKey.Key;
                var materialNode = materialReferenceKey.Value;
                textureVisitor.AssignDefaultTextureKeys(materialNode);
                var nodesToReduce = reducer.GetReducibleSubTrees(materialNode);

                for (var i = 0; i < nodesToReduce.Count; ++i)
                {
                    var nodeToReduce = nodesToReduce[i];
                    var finalTextureUrl = new UFile(basePath, materialReferenceName + "_Texture" + (nodesToReduce.Count > 1 ? i.ToString() : ""), null);
                    var infos = new MaterialReductionInfo { ToExecute = false, TextureUrl = finalTextureUrl };
                    var canBeReduced = textureVisitor.HasUniqueTexcoord(nodeToReduce, out infos.TexcoordIndex);
                    infos.OldNode = nodeToReduce;

                    if (!canBeReduced)
                        continue; // throw new Exception("Unsolvable tree");

                    commandList.Add(infos);
                }
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Saves the texture in a png file and in the database.
        /// </summary>
        /// <param name="texture">The Texture.</param>
        /// <param name="filename">The filename.</param>
        private void SaveTexture(Paradox.Graphics.Texture texture, string filename, AssetManager assetManager)
        {
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
            using (var image = texture.GetDataAsImage())
            {
                using (var resultFileStream = File.OpenWrite("tex" + textureIndex + ".png"))
                {
                    textureIndex++;
                    image.Save(resultFileStream, ImageFileType.Png);
                }

                assetManager.Save(filename, image);
            }
#endif
        }

        #endregion

        #region Helper class

        /// <summary>
        /// Class storing rendering commands information.
        /// </summary>
        private class MaterialReductionInfo
        {
            public Graphics.Texture RenderTarget;
            public bool ToExecute;
            public Graphics.Effect TreeEffect;
            public ParameterCollection Parameters;
            public TextureCoordinate TexcoordIndex;
            public UFile TextureUrl;
            public IMaterialNode OldNode;
        }

        #endregion
    }
}
