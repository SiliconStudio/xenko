// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Paradox.Assets
{
    internal static class PreviewerCompilerNames
    {
        // TODO: This will removed
        [Obsolete]
        private const string PreviewAssemblyName = "SiliconStudio.Paradox.GameStudio.Plugin";

        [Obsolete]
        private const string PreviewAssemblyQualifiedName = ", " + PreviewAssemblyName;

        [Obsolete]
        private const string ThumbnailCompilersNamespace = "SiliconStudio.Paradox.GameStudio.Plugin.ThumbnailCompilers.";

        [Obsolete]
        public const string AnimationThumbnailCompilerQualifiedName = ThumbnailCompilersNamespace + "AnimationThumbnailCompiler" + PreviewAssemblyQualifiedName;
        [Obsolete]
        public const string SceneThumbnailCompilerQualifiedName = ThumbnailCompilersNamespace + "SceneThumbnailCompiler" + PreviewAssemblyQualifiedName;
        [Obsolete]
        public const string MaterialThumbnailCompilerQualifiedName = ThumbnailCompilersNamespace + "MaterialThumbnailCompiler" + PreviewAssemblyQualifiedName;
        [Obsolete]
        public const string ModelThumbnailCompilerQualifiedName = ThumbnailCompilersNamespace + "ModelThumbnailCompiler" + PreviewAssemblyQualifiedName;
        [Obsolete]
        public const string SoundEffectThumbnailCompilerQualifiedName = ThumbnailCompilersNamespace + "SoundEffectThumbnailCompiler" + PreviewAssemblyQualifiedName;
        [Obsolete]
        public const string SoundMusicThumbnailCompilerQualifiedName = ThumbnailCompilersNamespace + "SoundMusicThumbnailCompiler" + PreviewAssemblyQualifiedName;
        [Obsolete]
        public const string TextureThumbnailCompilerQualifiedName = ThumbnailCompilersNamespace + "TextureThumbnailCompiler" + PreviewAssemblyQualifiedName;
        [Obsolete]
        public const string FontThumbnailCompilerQualifiedName = ThumbnailCompilersNamespace + "FontThumbnailCompiler" + PreviewAssemblyQualifiedName;
        [Obsolete]
        public const string SpriteSheetThumbnailCompilerQualifiedName = ThumbnailCompilersNamespace + "SpriteSheetThumbnailCompiler" + PreviewAssemblyQualifiedName;
        [Obsolete]
        public const string ProceduralModelThumbnailCompilerQualifiedName = ThumbnailCompilersNamespace + "ProceduralModelThumbnailCompiler" + PreviewAssemblyQualifiedName;
        [Obsolete]
        public const string GameSettingsThumbnailCompilerQualifiedName = ThumbnailCompilersNamespace + "GameSettingsThumbnailCompiler" + PreviewAssemblyQualifiedName;
        [Obsolete]
        public const string ScriptSourceFileThumbnailCompilerQualifiedName = ThumbnailCompilersNamespace + "ScriptSourceFileThumbnailCompiler" + PreviewAssemblyQualifiedName;
    }
}
